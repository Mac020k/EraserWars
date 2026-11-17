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
    public sealed class CpuSeatTests
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
        public void HumanSeatNeverActs_AndAShortThinkStillWaits()
        {
            var state = Opened();
            var human = CpuTurn.Choose(new CpuSeatMask(0), 0, state.Phase, state.Round, 5f);
            Assert.AreEqual(CpuActKind.Wait, human.Kind);

            var mask = new CpuSeatMask(0b0001);
            var early = CpuTurn.Choose(mask, 0, state.Phase, state.Round, CpuTurn.ThinkSeconds - 0.05f);
            Assert.AreEqual(CpuActKind.Wait, early.Kind);
            Assert.AreEqual(CpuActKind.Wait, CpuTurn.Choose(mask, 0, Phase.Simulating, 2, 5f).Kind);
            Assert.AreEqual(0.4f, CpuTurn.ThinkSeconds, 0.0001f);
        }

        [Test]
        public void Seat0Passes_AndTheReducerHandsTheTurnToSeat1()
        {
            var state = Opened();
            var act = CpuTurn.Choose(new CpuSeatMask(0b0001), 0, state.Phase, state.Round, CpuTurn.ThinkSeconds);
            Assert.AreEqual(CpuActKind.Pass, act.Kind);

            var next = MatchReduce.Reduce(state, new Passed(state.CurrentPlayerId));
            Assert.AreEqual(new PlayerId(1), next.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, next.Phase);
        }

        [Test]
        public void Seat1FlicksTheCannedNudge_AndTheReducerStartsSimulating()
        {
            var state = Opened();
            state = MatchReduce.Reduce(state, new Passed(state.CurrentPlayerId));
            Assert.AreEqual(new PlayerId(1), state.CurrentPlayerId);

            var act = CpuTurn.Choose(new CpuSeatMask(0b0010), 1, state.Phase, state.Round, CpuTurn.ThinkSeconds);
            Assert.AreEqual(CpuActKind.Flick, act.Kind);
            Assert.AreEqual(new PlayerId(1), act.Command.Player);
            Assert.AreEqual(0.18f, act.Command.DragLength01, 0.0001f);
            Assert.AreEqual(0.15f, act.Command.DirectionX, 0.0001f);
            Assert.AreEqual(1f, act.Command.DirectionY, 0.0001f);
            Assert.AreEqual(0.85f, act.Command.ContactOffsetY, 0.0001f);
            Assert.Less(act.Command.DragLength01, 0.35f);

            var flicked = MatchReduce.Reduce(state, new Flicked(act.Command));
            Assert.AreEqual(Phase.Simulating, flicked.Phase);
            Assert.AreEqual(new PlayerId(1), flicked.CurrentPlayerId);
        }

        [Test]
        public void Round2Seat0Flicks_Round1Seat0DoesNot()
        {
            Assert.IsFalse(CpuTurn.FlickThisVisit(1, 0));
            Assert.IsTrue(CpuTurn.FlickThisVisit(1, 1));
            Assert.IsTrue(CpuTurn.FlickThisVisit(2, 0));

            var flick = CpuTurn.Choose(new CpuSeatMask(0b0001), 0, Phase.Aim, 2, CpuTurn.ThinkSeconds);
            Assert.AreEqual(CpuActKind.Flick, flick.Kind);
        }

        [Test]
        public void MaskKeepsFourSeatBits()
        {
            var mask = new CpuSeatMask(0b0001_0001);
            Assert.AreEqual(0b0001, mask.Bits);
            Assert.IsTrue(mask.Contains(0));
            Assert.IsFalse(mask.Contains(1));
            Assert.IsFalse(mask.Contains(-1));
        }

        [Test]
        public void MarkedSeat0PassesAfterTheCallout_AndTheHumanSeatGetsTheTableBack()
        {
            using var rig = new Rig();
            rig.StartMatch(new CpuSeatMask(0b0001));
            Assert.IsFalse(rig.Director.AcceptsHumanAim);
            Assert.IsFalse(rig.Pass.enabledSelf, "A CPU seat should not offer Pass.");
            Assert.IsTrue(rig.HandHidden, "A CPU seat should not offer the hand.");

            rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
            Assert.AreEqual(0, rig.Director.State.CurrentPlayerId.Value);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);

            rig.Director.AdvanceMatchClock(CpuTurn.ThinkSeconds);
            rig.Hud.RefreshFromState();

            Assert.AreEqual(new PlayerId(1), rig.Director.State.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);
            Assert.Greater(rig.Director.State.TurnTimeLeft, 0f);
            Assert.IsTrue(rig.Director.TryGetTurnYield(out var notice));
            Assert.AreEqual(TurnYieldKind.Pass, notice.Kind);
            Assert.AreEqual(0, notice.Seat);
            Assert.IsTrue(rig.Director.AcceptsHumanAim);
            Assert.IsTrue(rig.Pass.enabledSelf, "The next human seat should get Pass back.");
            Assert.IsFalse(rig.HandHidden, "The next human seat should get the hand back.");
        }

        [Test]
        public void MarkedSeat1WeakFlicksAfterTheHumanPasses()
        {
            using var rig = new Rig();
            rig.StartMatch(new CpuSeatMask(0b0010));
            Assert.IsTrue(rig.Director.AcceptsHumanAim);
            Assert.IsTrue(rig.Pass.enabledSelf);

            rig.Director.Pass();
            Assert.AreEqual(new PlayerId(1), rig.Director.State.CurrentPlayerId);
            Assert.IsFalse(rig.Director.AcceptsHumanAim);

            rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);

            rig.Director.AdvanceMatchClock(CpuTurn.ThinkSeconds);
            Assert.AreEqual(Phase.Simulating, rig.Director.State.Phase);
            Assert.AreEqual(new PlayerId(1), rig.Director.State.CurrentPlayerId);
        }

        [Test]
        public void UnmarkedMatchStaysOnTheHuman()
        {
            using var rig = new Rig();
            rig.StartMatch(new CpuSeatMask(0));
            var seat = rig.Director.State.CurrentPlayerId;

            rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
            rig.Director.AdvanceMatchClock(CpuTurn.ThinkSeconds);

            Assert.AreEqual(seat, rig.Director.State.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);
            Assert.IsTrue(rig.Director.AcceptsHumanAim);
            Assert.IsFalse(rig.Director.TryGetTurnYield(out _));
        }

        [Test]
        public void PinnedMaskBeatsThePref()
        {
            var had = PlayerPrefs.HasKey(MatchDirector.CpuSeatsKey);
            var prior = PlayerPrefs.GetInt(MatchDirector.CpuSeatsKey, 0);
            try
            {
                PlayerPrefs.SetInt(MatchDirector.CpuSeatsKey, 0b0001);
                using var rig = new Rig();
                rig.StartMatch(new CpuSeatMask(0b0010));
                Assert.IsFalse(rig.Director.IsCpuSeat(0));
                Assert.IsTrue(rig.Director.IsCpuSeat(1));
                Assert.IsTrue(rig.Director.AcceptsHumanAim);
            }
            finally
            {
                RestorePref(had, prior);
            }
        }

        [Test]
        public void PrefMarksSeat0_AndThatSeatPasses()
        {
            var had = PlayerPrefs.HasKey(MatchDirector.CpuSeatsKey);
            var prior = PlayerPrefs.GetInt(MatchDirector.CpuSeatsKey, 0);
            try
            {
                PlayerPrefs.SetInt(MatchDirector.CpuSeatsKey, 0b0001);
                using var rig = new Rig();
                rig.StartMatch(null);
                Assert.IsTrue(rig.Director.IsCpuSeat(0));
                Assert.IsFalse(rig.Director.AcceptsHumanAim);

                rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
                rig.Director.AdvanceMatchClock(CpuTurn.ThinkSeconds);

                Assert.AreEqual(new PlayerId(1), rig.Director.State.CurrentPlayerId);
                Assert.IsTrue(rig.Director.TryGetTurnYield(out var notice));
                Assert.AreEqual(TurnYieldKind.Pass, notice.Kind);
            }
            finally
            {
                RestorePref(had, prior);
            }
        }

        static void RestorePref(bool had, int prior)
        {
            if (had)
            {
                PlayerPrefs.SetInt(MatchDirector.CpuSeatsKey, prior);
            }
            else
            {
                PlayerPrefs.DeleteKey(MatchDirector.CpuSeatsKey);
            }
        }

        static MatchState Opened()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            return MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;

            public Rig()
            {
                root = new GameObject("CpuSeatRig");
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

            public Button Pass => HudDocument.Q<Button>("pass");
            public bool HandHidden => HudDocument.Q("hand-dock").ClassListContains("hidden");

            VisualElement HudDocument
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement?.Q("root") ?? document.rootVisualElement;
                }
            }

            public void StartMatch(CpuSeatMask? mask)
            {
                if (mask is CpuSeatMask pinned)
                {
                    Director.SetCpuSeats(pinned);
                }

                Director.SetStartMenuOpen(false);
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                Director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, Director.State.Phase);
                Hud.RefreshFromState();
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
