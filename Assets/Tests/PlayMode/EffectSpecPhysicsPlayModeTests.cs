using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.PlayMode
{
    /// <summary>
    /// Plays real card effect strings through MatchDirector on a classroom-sized desk and measures the shot.
    /// </summary>
    public sealed class EffectSpecPhysicsPlayModeTests
    {
        const float DeskCenterY = 0.75f;
        const float DeskThickness = 0.04f;
        const float SpawnY = 0.78f;
        const float FellBelowY = 0.70f;
        const float SettleTimeoutSeconds = 10f;

        sealed class Shot
        {
            public float Travel;
            public bool Fell;
            public int DeskObjects;
        }

        GameObject root;
        MatchDirector director;
        EffectPhysicsSync effectPhysics;
        Transform[] spawns;
        CharacterDefinition[] roster;
        int nextCardId = 900;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("EffectSpecRig");
            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "PlaySurface";
            desk.transform.SetParent(root.transform, false);
            desk.transform.localPosition = new Vector3(0f, DeskCenterY, 0f);
            desk.transform.localScale = new Vector3(1.2f, DeskThickness, 0.8f);

            var template = GameObject.CreatePrimitive(PrimitiveType.Cube);
            template.name = "EraserTemplate";
            template.SetActive(false);
            template.transform.SetParent(root.transform, false);
            template.transform.localScale = new Vector3(0.043f, 0.011f, 0.017f);
            template.AddComponent<Rigidbody>();
            template.AddComponent<FalloutDetector>();
            var eraserPrefab = template.AddComponent<EraserBody>();

            spawns = new Transform[4];
            for (var i = 0; i < spawns.Length; i++)
            {
                spawns[i] = new GameObject($"Spawn_{i}").transform;
                spawns[i].SetParent(root.transform, false);
            }

            var directorGo = new GameObject("Director");
            directorGo.SetActive(false);
            directorGo.transform.SetParent(root.transform, false);
            var flickApplier = directorGo.AddComponent<FlickApplier>();
            var settleWatcher = directorGo.AddComponent<SettleWatcher>();
            director = directorGo.AddComponent<MatchDirector>();
            SetField(director, "flickApplier", flickApplier);
            SetField(director, "settleWatcher", settleWatcher);
            SetField(director, "spawnPoints", spawns);
            SetField(director, "eraserPrefab", eraserPrefab);
            SetField(director, "showDebug", false);
            directorGo.SetActive(true);
            effectPhysics = directorGo.GetComponent<EffectPhysicsSync>();
            Assert.NotNull(effectPhysics, "MatchDirector did not attach EffectPhysicsSync.");

            roster = new[] { Character("seat0"), Character("seat1") };
            director.SetSeatRoster(roster);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var eraser in director.Erasers)
            {
                if (eraser != null)
                {
                    Object.Destroy(eraser.gameObject);
                }
            }

            var deskObjects = GameObject.Find("DeskObjects");
            if (deskObjects != null)
            {
                Object.Destroy(deskObjects);
            }

            Object.Destroy(root);
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator FrictionAndSpeedCardsChangeSlideDistance()
        {
            var start = new Vector2(-0.4f, -0.25f);
            var dir = new Vector2(1f, 0f);
            var baseline = new Shot();
            var wax = new Shot();
            var sticky = new Shot();
            var warmup = new Shot();
            yield return Play(null, start, dir, 0.3f, baseline);
            yield return Play("ModifyStat:Friction:0.5:1", start, dir, 0.3f, wax);
            yield return Play("ModifyStat:Friction:3:1", start, dir, 0.3f, sticky);
            yield return Play("ModifyStat:Speed:1.3:1", start, dir, 0.3f, warmup);

            Debug.Log($"[EraserWars] effect-probe slide baseline={baseline.Travel:0.000} wax={wax.Travel:0.000} sticky={sticky.Travel:0.000} warmup={warmup.Travel:0.000}");
            Assert.IsFalse(baseline.Fell || wax.Fell || sticky.Fell || warmup.Fell, "A slide probe left the desk.");
            Assert.Greater(wax.Travel, baseline.Travel * 1.1f, "Wax (friction x0.5) should slide farther.");
            Assert.Less(sticky.Travel, baseline.Travel * 0.9f, "Sticky seal (friction x3) should stop sooner.");
            Assert.Greater(warmup.Travel, baseline.Travel * 1.02f, "Warm-up run (speed x1.3) should slide farther.");
        }

        [UnityTest]
        public IEnumerator RulerWallKeepsEraserOnDesk()
        {
            var start = new Vector2(-0.45f, -0.1f);
            var dir = new Vector2(-1f, 0f);
            var baseline = new Shot();
            var walled = new Shot();
            yield return Play(null, start, dir, 0.5f, baseline);
            yield return Play("PlaceDeskObject:RulerWall:2", start, dir, 0.5f, walled);

            Debug.Log($"[EraserWars] effect-probe ruler baseline fell={baseline.Fell} travel={baseline.Travel:0.000} | walled fell={walled.Fell} travel={walled.Travel:0.000} objects={walled.DeskObjects}");
            Assert.IsTrue(baseline.Fell, "Baseline edge shot should fall so the wall has something to stop.");
            Assert.AreEqual(1, walled.DeskObjects, "RulerWall should still stand through the opponent turn.");
            Assert.IsFalse(walled.Fell, "RulerWall should keep the eraser on the desk.");
        }

        [UnityTest]
        public IEnumerator DustPileSlowsEraserBeforeEdgeAndCleaningDutyClearsIt()
        {
            var start = new Vector2(-0.45f, -0.1f);
            var dir = new Vector2(-1f, 0f);
            var baseline = new Shot();
            var dusted = new Shot();
            yield return Play(null, start, dir, 0.4f, baseline);
            var clear = Card("PlaceDeskObject:ClearDust:1");
            director.SetSeatCommons(1, new[] { clear });
            yield return Play("PlaceDeskObject:DustPile:2", start, dir, 0.4f, dusted);

            Debug.Log($"[EraserWars] effect-probe dust baseline fell={baseline.Fell} travel={baseline.Travel:0.000} | dusted fell={dusted.Fell} travel={dusted.Travel:0.000} objects={dusted.DeskObjects}");
            Assert.IsTrue(baseline.Fell, "Baseline edge shot should fall so the dust has something to stop.");
            Assert.AreEqual(1, dusted.DeskObjects, "DustPile should still cover the edge on the opponent turn.");
            Assert.IsFalse(dusted.Fell, "DustPile should bleed enough speed to keep the eraser on the desk.");

            Assert.AreEqual(Phase.CardSelect, director.State.Phase);
            Assert.AreEqual(1, director.State.CurrentPlayerId.Value);
            director.PlayCard(clear.IdValue);
            yield return null;
            Debug.Log($"[EraserWars] effect-probe cleaning-duty objects={effectPhysics.DeskObjectCount} effects={director.State.Effects.Length}");
            Assert.AreEqual(0, effectPhysics.DeskObjectCount, "ClearDust should remove the dust pile.");
            director.SetSeatCommons(1, null);
        }

        [UnityTest]
        public IEnumerator ConeBlocksPathToRival()
        {
            var start = new Vector2(-0.3f, -0.2f);
            var dir = new Vector2(0.6f, 0.4f).normalized;
            spawns[1].position = new Vector3(0.3f, SpawnY, 0.2f);
            var baseline = new Shot();
            var coned = new Shot();
            yield return Play(null, start, dir, 0.45f, baseline);
            yield return Play("PlaceDeskObject:Cone:2", start, dir, 0.45f, coned);

            var halfway = Vector2.Distance(start, new Vector2(0.3f, 0.2f)) * 0.5f;
            Debug.Log($"[EraserWars] effect-probe cone baseline travel={baseline.Travel:0.000} | coned travel={coned.Travel:0.000} halfway={halfway:0.000} objects={coned.DeskObjects}");
            Assert.Greater(baseline.Travel, halfway + 0.05f, "Baseline shot should pass the cone spot.");
            Assert.Less(coned.Travel, halfway, "Cone at the midpoint should stop the striker short.");
        }

        IEnumerator Play(string effectKind, Vector2 start, Vector2 dir, float drag, Shot shot)
        {
            spawns[0].position = new Vector3(start.x, SpawnY, start.y);
            if (spawns[1].position == Vector3.zero)
            {
                spawns[1].position = new Vector3(0.35f, SpawnY, 0.3f);
            }

            var card = effectKind != null ? Card(effectKind) : null;
            director.SetSeatCommons(0, card != null ? new[] { card } : null);
            director.StartHotseat(2);
            director.SetStartMenuOpen(false);
            for (var i = 0; i < 40; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            if (card != null)
            {
                director.PlayCard(card.IdValue);
                Assert.AreEqual(Phase.Aim, director.State.Phase, $"{effectKind} was not played.");
            }

            var eraser = director.Erasers[0];
            var fell = false;
            eraser.GetComponent<FalloutDetector>().Fell += _ => fell = true;
            var from = eraser.Body.position;
            director.SubmitFlick(new FlickCommand(new PlayerId(0), dir.x, dir.y, drag, 0f, 0f, 0f));

            var elapsed = 0f;
            while (director.State.Phase == Phase.Simulating && elapsed < SettleTimeoutSeconds)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }

            yield return null;
            var to = eraser.Body.position;
            shot.Travel = new Vector2(to.x - from.x, to.z - from.z).magnitude;
            shot.Fell = fell || !eraser.gameObject.activeInHierarchy || to.y < FellBelowY;
            shot.DeskObjects = effectPhysics.DeskObjectCount;
        }

        CardDefinition Card(string effectKind)
        {
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            card.IdValue = nextCardId++;
            card.NameKey = effectKind;
            card.EffectKinds = new List<string> { effectKind };
            return card;
        }

        static CharacterDefinition Character(string name)
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            character.NameKey = name;
            character.Eraser = ScriptableObject.CreateInstance<EraserDefinition>();
            return character;
        }

        static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{target.GetType().Name}.{name} was renamed.");
            field.SetValue(target, value);
        }
    }
}
