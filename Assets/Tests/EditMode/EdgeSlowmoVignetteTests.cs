using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Core;
using EraserWars.Gameplay;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class EdgeSlowmoVignetteTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";

        bool hadReduce;
        int reduceBefore;
        float scaleBefore;

        [SetUp]
        public void SetUp()
        {
            scaleBefore = Time.timeScale;
            Time.timeScale = 1f;
            hadReduce = PlayerPrefs.HasKey(EdgeSlowmoCue.ReduceMotionPref);
            reduceBefore = PlayerPrefs.GetInt(EdgeSlowmoCue.ReduceMotionPref, 0);
            PlayerPrefs.DeleteKey(EdgeSlowmoCue.ReduceMotionPref);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = scaleBefore;
            if (hadReduce)
            {
                PlayerPrefs.SetInt(EdgeSlowmoCue.ReduceMotionPref, reduceBefore);
            }
            else
            {
                PlayerPrefs.DeleteKey(EdgeSlowmoCue.ReduceMotionPref);
            }
        }

        [Test]
        public void EdgeSlowmoDarkensTheRimThenClears()
        {
            using var rig = new Rig();
            rig.StartMatch();
            Assert.AreEqual(DisplayStyle.None, rig.Veil.style.display.value);
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);

            var watcher = rig.Watcher;
            var eraser = rig.ActiveEraser();
            rig.Director.SubmitFlick(new FlickCommand(rig.Director.State.CurrentPlayerId, 1f, 0f, 0.4f, 0f, 0f, 0f));
            PlaceOnTheLip(eraser);
            Assert.AreEqual(Phase.Simulating, rig.Director.State.Phase);

            var start = Time.unscaledTime;
            watcher.Poll(start);
            rig.Hud.RefreshFromState();

            Assert.IsTrue(watcher.IsSlowing, $"pose={eraser.Body.position}");
            Assert.Less(Time.timeScale, 1f);
            var veil = rig.Veil;
            Assert.AreEqual(DisplayStyle.Flex, veil.style.display.value);
            Assert.Greater(veil.style.opacity.value, 0.3f);
            Assert.Less(veil.style.opacity.value, 1f);
            Assert.AreEqual(
                watcher.CurrentCue(Time.unscaledTime, false).EdgeAlpha,
                veil.style.opacity.value,
                0.02f);
            Assert.AreEqual(PickingMode.Ignore, veil.pickingMode);
            Assert.AreEqual(0, veil.parent.IndexOf(veil));

            var mask = EdgeSlowmoVignette.Mask;
            Assert.AreSame(mask, veil.style.backgroundImage.value.texture);
            Assert.Less(mask.GetPixel(32, 32).a, 0.05f);
            Assert.Greater(mask.GetPixel(0, 0).a, 0.9f);
            Assert.Less(veil.style.unityBackgroundImageTintColor.value.maxColorComponent, 0.35f);

            var now = start;
            var steps = 0;
            while (watcher.IsSlowing && steps < 10)
            {
                now += 0.05f;
                watcher.Poll(now);
                steps++;
            }

            rig.Hud.RefreshFromState();
            Assert.IsFalse(watcher.IsSlowing);
            Assert.Greater(now - start, 0.15f);
            Assert.Less(now - start, 0.45f);
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
            Assert.AreEqual(DisplayStyle.None, rig.Veil.style.display.value);
            Assert.AreEqual(0f, rig.Veil.style.opacity.value, 0.0001f);
        }

        [Test]
        public void ReduceMotionKeepsASteadyRim()
        {
            PlayerPrefs.SetInt(EdgeSlowmoCue.ReduceMotionPref, 1);
            using var rig = new Rig();
            rig.StartMatch();

            var eraser = rig.ActiveEraser();
            rig.Director.SubmitFlick(new FlickCommand(rig.Director.State.CurrentPlayerId, 1f, 0f, 0.4f, 0f, 0f, 0f));
            PlaceOnTheLip(eraser);
            rig.Watcher.Poll(Time.unscaledTime);
            rig.Hud.RefreshFromState();

            var quiet = EdgeSlowmoCue.At(true, 0.2f, true).EdgeAlpha;
            Assert.IsTrue(rig.Watcher.IsSlowing, $"pose={eraser.Body.position}");
            Assert.Less(Time.timeScale, 1f);
            Assert.AreEqual(quiet, rig.Veil.style.opacity.value, 0.0001f);
            Assert.Less(quiet, EdgeSlowmoCue.At(true, 0f, false).EdgeAlpha);
            Assert.AreEqual(DisplayStyle.Flex, rig.Veil.style.display.value);
        }

        static void PlaceOnTheLip(EraserBody eraser)
        {
            var lip = new Vector3(0.56f, 0.9f, 0f);
            eraser.transform.SetPositionAndRotation(lip, Quaternion.identity);
            eraser.Body.position = lip;
            Physics.SyncTransforms();
            Assert.AreEqual(lip.x, eraser.Body.position.x, 0.02f);
            Assert.GreaterOrEqual(eraser.Body.position.y, 0.75f);
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;

            public Rig()
            {
                root = new GameObject("EdgeSlowmoRig");
                var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
                surface.name = "PlaySurface";
                surface.transform.SetParent(root.transform, false);

                var template = GameObject.CreatePrimitive(PrimitiveType.Cube);
                template.name = "EraserTemplate";
                template.SetActive(false);
                template.transform.SetParent(root.transform, false);
                template.AddComponent<Rigidbody>();
                template.AddComponent<FalloutDetector>();
                var eraserPrefab = template.AddComponent<EraserBody>();

                var spawns = new Transform[2];
                for (var i = 0; i < spawns.Length; i++)
                {
                    spawns[i] = new GameObject($"Spawn_{i}").transform;
                    spawns[i].SetParent(root.transform, false);
                    spawns[i].position = new Vector3(i * 0.3f, 0.8f, 0f);
                }

                var directorGo = new GameObject("Director");
                directorGo.SetActive(false);
                directorGo.transform.SetParent(root.transform, false);
                var flickApplier = directorGo.AddComponent<FlickApplier>();
                var settleWatcher = directorGo.AddComponent<SettleWatcher>();
                Director = directorGo.AddComponent<MatchDirector>();
                SetField(Director, "flickApplier", flickApplier);
                SetField(Director, "settleWatcher", settleWatcher);
                SetField(Director, "spawnPoints", spawns);
                SetField(Director, "eraserPrefab", eraserPrefab);
                SetField(Director, "showDebug", false);
                directorGo.SetActive(true);
                var physics = directorGo.GetComponent<EffectPhysicsSync>();
                if (physics == null)
                {
                    physics = directorGo.AddComponent<EffectPhysicsSync>();
                }

                SetField(Director, "effectPhysics", physics);

                var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudTreePath);
                Assert.NotNull(panel, PanelPath);
                Assert.NotNull(tree, HudTreePath);

                var menuGo = new GameObject("ClosedMenu");
                menuGo.SetActive(false);
                menuGo.transform.SetParent(root.transform, false);
                var closedMenu = menuGo.AddComponent<StartMenuController>();

                var hudGo = new GameObject("MatchHud");
                hudGo.SetActive(false);
                hudGo.transform.SetParent(root.transform, false);
                var document = hudGo.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset = tree;
                Hud = hudGo.AddComponent<MatchHudController>();
                SetField(Hud, "document", document);
                SetField(Hud, "director", Director);
                SetField(Hud, "startMenu", closedMenu);
                hudGo.SetActive(true);
            }

            public EdgeSlowmoWatcher Watcher => Director.GetComponent<EdgeSlowmoWatcher>();

            public VisualElement Veil => HudRoot.Q(EdgeSlowmoVignette.ElementName);

            VisualElement HudRoot
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement.Q("root") ?? document.rootVisualElement;
                }
            }

            public EraserBody ActiveEraser()
            {
                var seat = Director.State.CurrentPlayerId.Value;
                foreach (var eraser in Director.Erasers)
                {
                    if (eraser != null && eraser.PlayerIndex == seat)
                    {
                        return eraser;
                    }
                }

                Assert.Fail("The active eraser was not spawned.");
                return null;
            }

            public void StartMatch()
            {
                Director.SetStartMenuOpen(false);
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                Director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, Director.State.Phase);
                Hud.RefreshFromState();
                Assert.NotNull(Watcher);
                Assert.NotNull(Veil);
            }

            public void Dispose()
            {
                if (Director != null)
                {
                    foreach (var eraser in Director.Erasers)
                    {
                        if (eraser != null)
                        {
                            Object.DestroyImmediate(eraser.gameObject);
                        }
                    }
                }

                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }
            }

            static void SetField(object target, string name, object value)
            {
                var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(field, $"{target.GetType().Name}.{name} was renamed.");
                field.SetValue(target, value);
            }
        }
    }
}
