using System.Collections;
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
    public sealed class MatchEndHudTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";

        [UnityTest]
        public IEnumerator StockWinShowsTheWinnerSeatAndStockLine()
        {
            using var rig = new Rig(WinMode.Stock);
            rig.KnockOut(0, 3);
            rig.FinishRound();
            rig.Hud.RefreshFromState();
            yield return null;

            AssertFinish(rig, "P2 のかち!", "P1 ストック 0  ·  P2 ストック 3", 1);
            var winner = rig.Result.resolvedStyle.fontSize;
            var callout = rig.Turn.resolvedStyle.fontSize;
            var bar = rig.SeatBar.resolvedStyle.height;
            Assert.Greater(winner, callout, $"winner {winner} callout {callout}");
            Assert.That(bar / winner, Is.EqualTo(0.5f).Within(0.08f), $"bar {bar} winner {winner}");
        }

        [UnityTest]
        public IEnumerator DrawShowsNoSeatClass()
        {
            using var rig = new Rig(WinMode.Stock);
            rig.KnockOut(0, 3);
            rig.KnockOut(1, 3);
            rig.FinishRound();
            rig.Hud.RefreshFromState();
            yield return null;

            AssertFinish(rig, "ひきわけ!", "P1 ストック 0  ·  P2 ストック 0", -1);
            var bar = rig.SeatBar.resolvedStyle.height;
            var winner = rig.Result.resolvedStyle.fontSize;
            Assert.Greater(bar, 0f, $"draw bar {bar}");
            Assert.Less(bar, winner, $"draw bar {bar} winner {winner}");
        }

        [Test]
        public void PointsWinShowsThePointLine()
        {
            using var rig = new Rig(WinMode.Points);
            rig.KnockOut(1, 3);
            rig.FinishRound();
            rig.Hud.RefreshFromState();

            AssertFinish(rig, "P1 のかち!", "P1 3点  ·  P2 0点", 0);
        }

        static void AssertFinish(Rig rig, string title, string summary, int seat)
        {
            Assert.AreEqual(Phase.MatchEnd, rig.Director.State.Phase);
            Assert.AreEqual(title, rig.Result.text);
            Assert.AreEqual(summary, rig.Summary.text);
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(i == seat, rig.Card.ClassListContains($"seat-{i}"), $"seat-{i}");
            }

            Assert.IsFalse(rig.MatchEnd.ClassListContains("hidden"));
            Assert.IsTrue(rig.Callout.ClassListContains("hidden"));
            Assert.IsTrue(rig.CutIn.ClassListContains("hidden"));
            Assert.IsNotNull(rig.Praise, "Knock praise should stay on the HUD.");
            Assert.AreEqual("再戦", rig.Rematch.text);
            Assert.AreEqual("メニューへ", rig.Menu.text);
            Assert.IsTrue(rig.Rematch.ClassListContains("match-end-primary"));
            Assert.IsTrue(rig.Menu.ClassListContains("match-end-secondary"));
            Assert.IsFalse(rig.Rematch.ClassListContains("hidden"));
            Assert.IsFalse(rig.Menu.ClassListContains("hidden"));
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;
            readonly bool hadWinMode;
            readonly int previousWinMode;

            public Rig(WinMode mode)
            {
                hadWinMode = PlayerPrefs.HasKey(MatchDirector.HotseatWinModeKey);
                previousWinMode = PlayerPrefs.GetInt(MatchDirector.HotseatWinModeKey, (int)WinMode.Stock);
                PlayerPrefs.SetInt(MatchDirector.HotseatWinModeKey, (int)mode);

                root = new GameObject("MatchEndRig");
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

                var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudTreePath);
                Assert.NotNull(panel, PanelPath);
                Assert.NotNull(tree, HudTreePath);

                var hudGo = new GameObject("MatchHud");
                hudGo.SetActive(false);
                hudGo.transform.SetParent(root.transform, false);
                var document = hudGo.AddComponent<UIDocument>();
                document.panelSettings = panel;
                document.visualTreeAsset = tree;
                Hud = hudGo.AddComponent<MatchHudController>();
                SetField(Hud, "document", document);
                SetField(Hud, "director", Director);
                hudGo.SetActive(true);
                Assert.NotNull(Tree, "MatchHud did not build a visual tree.");

                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                Director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, Director.State.Phase);
            }

            public Label Result => Tree.Q<Label>("result-text");
            public Label Summary => Tree.Q<Label>("result-summary");
            public Label Turn => Tree.Q<Label>("turn");
            public VisualElement Card => Tree.Q("match-end-card");
            public VisualElement SeatBar => Tree.Q("result-seat-bar");
            public VisualElement MatchEnd => Tree.Q("match-end");
            public VisualElement Callout => Tree.Q("turn-callout");
            public VisualElement CutIn => Tree.Q("card-cutin");
            public VisualElement Praise => Tree.Q("knock-praise");
            public Button Rematch => Tree.Q<Button>("rematch");
            public Button Menu => Tree.Q<Button>("match-end-menu");

            VisualElement Tree
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement.Q("root") ?? document.rootVisualElement;
                }
            }

            public void KnockOut(int seat, int times)
            {
                for (var i = 0; i < times; i++)
                {
                    Director.DebugKnockOut(seat);
                }
            }

            public void FinishRound()
            {
                for (var i = 0; i < 8 && Director.State.Phase != Phase.MatchEnd; i++)
                {
                    Director.Pass();
                }
            }

            public void Dispose()
            {
                if (hadWinMode)
                {
                    PlayerPrefs.SetInt(MatchDirector.HotseatWinModeKey, previousWinMode);
                }
                else
                {
                    PlayerPrefs.DeleteKey(MatchDirector.HotseatWinModeKey);
                }

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
