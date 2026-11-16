using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Audio;
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
    public sealed class PassTimerHudTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";

        [TearDown]
        public void TearDown()
        {
            if (AudioBus.Instance != null)
            {
                Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }
        }

        [Test]
        public void PassStampsTheClockAndPlaysTheHandoffCue()
        {
            AssertBriefStamp();
            using var rig = new Rig();
            rig.StartMatch();
            var seat = rig.Director.State.CurrentPlayerId.Value;

            rig.Director.Pass();
            rig.Hud.RefreshFromState();

            Assert.AreNotEqual(seat, rig.Director.State.CurrentPlayerId.Value);
            AssertStamp(rig, TurnYieldKind.Pass, seat);
            Assert.IsFalse(rig.CalloutHidden, "The next turn stays on screen.");
            Assert.That(rig.TurnText, Does.Contain("のばん"));
            Assert.AreEqual("turn-pass", CueName());

            rig.Director.AdvanceMatchClock(TurnYieldNotice.Seconds * 0.5f);
            rig.Hud.RefreshFromState();
            Assert.IsFalse(rig.YieldHidden, "Half of the stamp window should still be readable.");

            rig.Director.AdvanceMatchClock(TurnYieldNotice.Seconds * 0.5f);
            rig.Hud.RefreshFromState();
            Assert.IsTrue(rig.YieldHidden, "The stamp should leave once its window ends.");
        }

        [Test]
        public void TimerExpiryStampsTheClockAndPlaysTheSameCue()
        {
            AssertBriefStamp();
            using var rig = new Rig();
            rig.StartMatch();
            var seat = rig.Director.State.CurrentPlayerId.Value;
            var left = rig.Director.State.TurnTimeLeft;

            rig.Director.AdvanceMatchClock(left + 0.05f);
            rig.Hud.RefreshFromState();

            Assert.AreNotEqual(seat, rig.Director.State.CurrentPlayerId.Value);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);
            AssertStamp(rig, TurnYieldKind.Time, seat);
            Assert.IsFalse(rig.CalloutHidden, "The next turn stays on screen.");
            Assert.AreEqual("turn-pass", CueName());
        }

        [Test]
        public void PassDuringAFlickDoesNotStampOrChangeTheCue()
        {
            using var rig = new Rig();
            rig.StartMatch();
            var seat = rig.Director.State.CurrentPlayerId;

            rig.Director.SubmitFlick(new FlickCommand(seat, 1f, 0f, 0.4f, 0f, 0f, 0f));
            Assert.AreEqual("flick", CueName());
            rig.Director.Pass();
            rig.Hud.RefreshFromState();

            Assert.AreEqual(Phase.Simulating, rig.Director.State.Phase);
            Assert.AreEqual(seat, rig.Director.State.CurrentPlayerId);
            Assert.IsTrue(rig.YieldHidden);
            Assert.AreEqual("flick", CueName());
        }

        [Test]
        public void SettleCutoffStampsTheFlickerAndPlaysItsOwnCue()
        {
            using var rig = new Rig();
            rig.StartMatch();
            var seat = rig.Director.State.CurrentPlayerId;
            var watcher = ArmedWatcher(rig.Director);
            var cutoff = SerializedSeconds(watcher, "maxSimSeconds");
            Assert.AreEqual(8f, cutoff, 0.001f);

            rig.Director.SubmitFlick(new FlickCommand(seat, 1f, 0f, 0.4f, 0f, 0f, 0f));
            Assert.AreEqual(Phase.Simulating, rig.Director.State.Phase);
            SetSlide(rig.Director, moving: true);
            Pump(watcher, cutoff + 0.02f);

            Assert.AreNotEqual(Phase.Simulating, rig.Director.State.Phase);
            rig.Hud.RefreshFromState();
            AssertStamp(rig, TurnYieldKind.Cutoff, seat.Value);
            Assert.AreEqual("settle-cutoff", CueName());

            rig.Director.AdvanceMatchClock(TurnYieldNotice.Seconds);
            rig.Hud.RefreshFromState();
            Assert.IsTrue(rig.YieldHidden, "The cutoff stamp should leave with the other stamps.");
        }

        [Test]
        public void NaturalSettleEndsTheFlickWithoutTheCutoffStamp()
        {
            using var rig = new Rig();
            rig.StartMatch();
            var seat = rig.Director.State.CurrentPlayerId;
            var watcher = ArmedWatcher(rig.Director);
            var hold = SerializedSeconds(watcher, "settleHoldSeconds");

            rig.Director.SubmitFlick(new FlickCommand(seat, 1f, 0f, 0.4f, 0f, 0f, 0f));
            Assert.AreEqual("flick", CueName());
            SetSlide(rig.Director, moving: false);
            Pump(watcher, hold + 0.02f);

            Assert.AreNotEqual(Phase.Simulating, rig.Director.State.Phase);
            rig.Hud.RefreshFromState();
            Assert.IsTrue(rig.YieldHidden, "A natural stop should not stamp the cutoff.");
            Assert.AreEqual("flick", CueName());
        }

        static void AssertBriefStamp()
        {
            Assert.GreaterOrEqual(TurnYieldNotice.Seconds, 0.45f);
            Assert.LessOrEqual(TurnYieldNotice.Seconds, 0.8f);
        }

        static void AssertStamp(Rig rig, TurnYieldKind kind, int seat)
        {
            var stamp = rig.Yield;
            Assert.IsFalse(rig.YieldHidden);
            Assert.AreEqual(new TurnYieldNotice(kind, seat).Label, stamp.text);
            var expected = YieldClass(kind);
            Assert.IsTrue(stamp.ClassListContains(expected));
            AssertAbsent(stamp, "yield-pass", expected);
            AssertAbsent(stamp, "yield-time", expected);
            AssertAbsent(stamp, "yield-cutoff", expected);
            Assert.IsTrue(stamp.ClassListContains($"seat-{seat}"));
        }

        static void AssertAbsent(Label stamp, string className, string expected)
        {
            if (className != expected)
            {
                Assert.IsFalse(stamp.ClassListContains(className));
            }
        }

        static string YieldClass(TurnYieldKind kind)
        {
            switch (kind)
            {
                case TurnYieldKind.Pass:
                    return "yield-pass";
                case TurnYieldKind.Time:
                    return "yield-time";
                case TurnYieldKind.Cutoff:
                    return "yield-cutoff";
                default:
                    Assert.Fail($"Unhandled {kind}");
                    return "";
            }
        }

        static SettleWatcher ArmedWatcher(MatchDirector director)
        {
            director.ListenForSettle();
            var field = typeof(MatchDirector).GetField(
                "settleWatcher",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, "MatchDirector.settleWatcher was renamed.");
            var watcher = (SettleWatcher)field.GetValue(director);
            Assert.NotNull(watcher, "The director has no settle watcher.");
            Assert.AreSame(watcher, director.GetComponent<SettleWatcher>());
            return watcher;
        }

        static float SerializedSeconds(SettleWatcher watcher, string property)
        {
            var so = new SerializedObject(watcher);
            var prop = so.FindProperty(property);
            Assert.NotNull(prop, property);
            return prop.floatValue;
        }

        static void SetSlide(MatchDirector director, bool moving)
        {
            var velocity = moving ? new Vector3(1.5f, 0f, 0f) : Vector3.zero;
            var count = 0;
            foreach (var eraser in director.Erasers)
            {
                Assert.NotNull(eraser.Body, "The watched eraser needs a body.");
                eraser.Body.linearVelocity = velocity;
                eraser.Body.angularVelocity = Vector3.zero;
                count++;
            }

            Assert.Greater(count, 0);
        }

        static void Pump(SettleWatcher watcher, float seconds)
        {
            const float dt = 0.02f;
            var steps = Mathf.CeilToInt(seconds / dt);
            for (var i = 0; i < steps; i++)
            {
                watcher.Tick(dt);
            }
        }

        static string CueName()
        {
            Assert.NotNull(AudioBus.Instance, "The handoff should have started the audio bus.");
            var sources = AudioBus.Instance.GetComponents<AudioSource>();
            for (var i = 0; i < sources.Length; i++)
            {
                if (!sources[i].loop)
                {
                    return sources[i].clip.name;
                }
            }

            Assert.Fail("SE source missing");
            return null;
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;

            public Rig()
            {
                root = new GameObject("PassTimerRig");
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
                Director.ListenForSettle();
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

            public string TurnText => HudDocument.Q<Label>("turn")?.text;
            public bool CalloutHidden => HudDocument.Q("turn-callout").ClassListContains("hidden");
            public Label Yield => HudDocument.Q<Label>("turn-yield");
            public bool YieldHidden => Yield.ClassListContains("hidden");

            VisualElement HudDocument
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement?.Q("root") ?? document.rootVisualElement;
                }
            }

            public void StartMatch()
            {
                Director.SetStartMenuOpen(false);
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                Director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, Director.State.Phase);
                Hud.RefreshFromState();
                Assert.IsTrue(YieldHidden, "A fresh turn should not show a stamp.");
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
