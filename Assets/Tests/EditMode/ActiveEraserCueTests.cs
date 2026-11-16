using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Audio;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.EditMode
{
    public sealed class ActiveEraserCueTests
    {
        readonly List<UnityEngine.Object> owned = new();

        [TearDown]
        public void TearDown()
        {
            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            var lines = UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null && lines[i].name.StartsWith("ActiveEraserOutline_"))
                {
                    UnityEngine.Object.DestroyImmediate(lines[i].gameObject);
                }
            }

            if (AudioBus.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }
        }

        [Test]
        public void CueShowsOnlyWhileTheSeatCanAct()
        {
            foreach (Phase phase in Enum.GetValues(typeof(Phase)))
            {
                var expected = phase switch
                {
                    Phase.TurnStart => true,
                    Phase.CardSelect => true,
                    Phase.Aim => true,
                    Phase.Simulating => false,
                    Phase.Resolve => false,
                    Phase.RoundEnd => false,
                    Phase.MatchEnd => false,
                    _ => throw new AssertionException($"Classify {phase}."),
                };
                Assert.AreEqual(expected, ActiveEraserCue.PhaseShowsCue(phase), phase.ToString());
            }
        }

        [Test]
        public void PulseIsBrighterAtThePeakThanAtTheTrough()
        {
            var seat = new Color(0.92f, 0.28f, 0.35f);
            var peak = ActiveEraserCue.At(true, seat, 1f / (4f * 1.15f));
            var trough = ActiveEraserCue.At(true, seat, 3f / (4f * 1.15f));
            var hidden = ActiveEraserCue.At(false, seat, 1f / (4f * 1.15f));

            Assert.IsTrue(peak.Show);
            Assert.AreEqual(0.92f * 1.05f, peak.Emission.r, 0.01f);
            Assert.AreEqual(0.7f, peak.Outline.a, 0.01f);
            Assert.AreEqual(seat.g, peak.Outline.g, 0.01f);
            Assert.AreEqual(0.92f * 1.05f * 0.42f, trough.Emission.r, 0.01f);
            Assert.Greater(peak.Emission.r, trough.Emission.r);
            Assert.Greater(peak.Outline.a, trough.Outline.a);
            Assert.IsFalse(hidden.Show);
            Assert.AreEqual(0f, hidden.Emission.maxColorComponent, 0.001f);
            Assert.AreEqual(0f, hidden.Outline.a, 0.001f);
        }

        [Test]
        public void ApplyEasesScaleAndGlowOntoTheShownEraserOnly()
        {
            var view = new ActiveEraserCueView();
            var shown = Eraser(0, new Vector3(0.06f, 0.018f, 0.028f));
            var other = Eraser(1, new Vector3(0.06f, 0.018f, 0.028f));
            var seat = new Color(0.92f, 0.28f, 0.35f);
            var peak = ActiveEraserCue.At(true, seat, 1f / (4f * 1.15f));
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));

            view.Apply(shown, peak, 0f);
            Assert.AreEqual(0.06f, shown.transform.localScale.x, 0.0001f);
            Assert.IsFalse(Body(shown).material.IsKeywordEnabled("_EMISSION"));

            var blend = (float)(1d - Math.Exp(-1d));
            view.Apply(shown, peak, 1f / 12f);
            view.Apply(other, ActiveEraserCue.At(false, Color.blue, 0f), 1f / 12f);
            Assert.AreEqual(0.06f * Mathf.Lerp(1f, 1.14f, blend), shown.transform.localScale.x, 0.001f);
            Assert.AreEqual(0.06f, other.transform.localScale.x, 0.0001f);
            Assert.AreEqual(peak.Emission.r * blend, Body(shown).material.GetColor("_EmissionColor").r, 0.02f);
            Assert.AreEqual(0f, Body(other).material.GetColor("_EmissionColor").maxColorComponent, 0.02f);

            view.Apply(shown, peak, 1f);
            view.Apply(other, ActiveEraserCue.At(false, Color.blue, 0f), 1f);
            Assert.AreEqual(0.06f * 1.14f, shown.transform.localScale.x, 0.001f);
            Assert.AreEqual(0.06f, other.transform.localScale.x, 0.0001f);
            Assert.IsTrue(Body(shown).material.IsKeywordEnabled("_EMISSION"));
            Assert.AreEqual(peak.Emission.r, Body(shown).material.GetColor("_EmissionColor").r, 0.01f);
            Assert.IsFalse(Body(other).material.IsKeywordEnabled("_EMISSION"));
            var ring = GameObject.Find("ActiveEraserOutline_0");
            Assert.NotNull(ring);
            var line = ring.GetComponent<LineRenderer>();
            Assert.IsTrue(line.enabled);
            Assert.AreEqual(peak.Outline.a, line.startColor.a, 0.01f);
            Assert.AreEqual(seat.r, line.startColor.r, 0.01f);
            Assert.IsNull(GameObject.Find("ActiveEraserOutline_1"));

            view.Apply(shown, ActiveEraserCue.At(false, seat, 0f), 1f);
            Assert.AreEqual(0.06f, shown.transform.localScale.x, 0.001f);
            Assert.AreEqual(0f, Body(shown).material.GetColor("_EmissionColor").maxColorComponent, 0.02f);
            Assert.IsFalse(Body(shown).material.IsKeywordEnabled("_EMISSION"));
            Assert.IsFalse(line.enabled);
        }

        [Test]
        public void MatchDirectorMovesTheCueWhenTheTurnPasses()
        {
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
            var director = Director();
            director.SetStartMenuOpen(false);
            director.StartHotseat(2);
            for (var i = 0; i < director.Erasers.Count; i++)
            {
                owned.Add(director.Erasers[i].gameObject);
            }

            Assert.AreEqual(Phase.CardSelect, director.State.Phase);
            var peakTime = 1f / (4f * 1.15f);
            director.PresentActiveEraserCue(1f, peakTime);
            AssertCue(director.Erasers[0], shown: true);
            AssertCue(director.Erasers[1], shown: false);

            director.Pass();
            Assert.AreEqual(1, director.State.CurrentPlayerId.Value);
            Assert.AreEqual(Phase.CardSelect, director.State.Phase);
            director.PresentActiveEraserCue(1f, peakTime);
            AssertCue(director.Erasers[0], shown: false);
            AssertCue(director.Erasers[1], shown: true);
        }

        static void AssertCue(EraserBody eraser, bool shown)
        {
            var material = Body(eraser).material;
            var emission = material.GetColor("_EmissionColor").maxColorComponent;
            var lineObject = GameObject.Find($"ActiveEraserOutline_{eraser.PlayerIndex}");
            if (shown)
            {
                Assert.Greater(eraser.transform.localScale.x, 0.06f);
                Assert.Greater(emission, 0.5f);
                Assert.IsTrue(material.IsKeywordEnabled("_EMISSION"));
                Assert.NotNull(lineObject);
                Assert.IsTrue(lineObject.GetComponent<LineRenderer>().enabled);
                return;
            }

            Assert.AreEqual(0.06f, eraser.transform.localScale.x, 0.002f);
            Assert.Less(emission, 0.05f);
            Assert.IsFalse(material.IsKeywordEnabled("_EMISSION"));
            if (lineObject != null)
            {
                Assert.IsFalse(lineObject.GetComponent<LineRenderer>().enabled);
            }
        }

        EraserBody Eraser(int seat, Vector3 scale)
        {
            var go = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            go.name = $"Eraser_{seat}";
            go.transform.localScale = scale;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Assert.NotNull(shader);
            go.GetComponent<MeshRenderer>().sharedMaterial = new Material(shader);
            go.AddComponent<Rigidbody>();
            var body = go.AddComponent<EraserBody>();
            body.PlayerIndex = seat;
            return body;
        }

        MatchDirector Director()
        {
            var root = Track(new GameObject("ActiveCueRig"));
            var template = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            template.name = "EraserTemplate";
            template.SetActive(false);
            template.transform.SetParent(root.transform, false);
            template.transform.localScale = new Vector3(0.06f, 0.018f, 0.028f);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Assert.NotNull(shader);
            template.GetComponent<MeshRenderer>().sharedMaterial = new Material(shader);
            template.AddComponent<Rigidbody>();
            template.AddComponent<FalloutDetector>();
            var prefab = template.AddComponent<EraserBody>();

            var spawns = new Transform[2];
            for (var i = 0; i < spawns.Length; i++)
            {
                var spawn = Track(new GameObject($"Spawn_{i}"));
                spawn.transform.SetParent(root.transform, false);
                spawn.transform.position = new Vector3(i * 0.3f, 0.8f, 0f);
                spawns[i] = spawn.transform;
            }

            var directorGo = Track(new GameObject("Director"));
            directorGo.SetActive(false);
            directorGo.transform.SetParent(root.transform, false);
            var flick = directorGo.AddComponent<FlickApplier>();
            var settle = directorGo.AddComponent<SettleWatcher>();
            var director = directorGo.AddComponent<MatchDirector>();
            SetField(director, "flickApplier", flick);
            SetField(director, "settleWatcher", settle);
            SetField(director, "spawnPoints", spawns);
            SetField(director, "eraserPrefab", prefab);
            SetField(director, "showDebug", false);
            directorGo.SetActive(true);
            return director;
        }

        static Renderer Body(EraserBody eraser)
        {
            var renderers = eraser.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] is not TrailRenderer and not LineRenderer)
                {
                    return renderers[i];
                }
            }

            Assert.Fail("Eraser body renderer missing.");
            return null;
        }

        T Track<T>(T obj) where T : UnityEngine.Object
        {
            owned.Add(obj);
            return obj;
        }

        static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{target.GetType().Name}.{name} was renamed.");
            field.SetValue(target, value);
        }
    }
}
