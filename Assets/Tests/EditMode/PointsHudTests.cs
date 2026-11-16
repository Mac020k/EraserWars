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
    public sealed class PointsHudTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";

        [UnityTest]
        public IEnumerator PointsModeShowsEachSeatScoreDuringTheMatch()
        {
            using var rig = new Rig(WinMode.Points);
            rig.Hud.RefreshFromState();
            yield return null;

            AssertLive(rig);
            AssertScores(rig, "0点", "0点");
            AssertScoreReadable(rig);
            AssertRow(rig, scoreShown: true);

            rig.KnockOut(1);
            rig.Hud.RefreshFromState();
            yield return null;

            Assert.AreEqual(1, rig.Director.State.Players[0].Points);
            Assert.AreEqual(0, rig.Director.State.Players[1].Points);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);
            AssertLive(rig);
            AssertScores(rig, "1点", "0点");
            AssertScoreReadable(rig);
            AssertRow(rig, scoreShown: true);
        }

        [UnityTest]
        public IEnumerator StockModeKeepsHeartsAndHidesTheScore()
        {
            using var rig = new Rig(WinMode.Stock);
            rig.Hud.RefreshFromState();
            yield return null;

            AssertLive(rig);
            AssertRow(rig, scoreShown: false);
            AssertHearts(rig, 0, 0);

            rig.KnockOut(1);
            rig.Hud.RefreshFromState();
            yield return null;

            Assert.AreEqual(3, rig.Director.State.Players[0].Stocks);
            Assert.AreEqual(2, rig.Director.State.Players[1].Stocks);
            Assert.AreEqual(Phase.CardSelect, rig.Director.State.Phase);
            AssertLive(rig);
            AssertRow(rig, scoreShown: false);
            AssertHearts(rig, 0, 1);
        }

        static void AssertLive(Rig rig)
        {
            Assert.AreNotEqual(Phase.MatchEnd, rig.Director.State.Phase);
            Assert.IsTrue(rig.Tree.Q("match-end").ClassListContains("hidden"));
        }

        static void AssertScores(Rig rig, string first, string second)
        {
            Assert.AreEqual(first, Score(rig.Players[0]).text);
            Assert.AreEqual(second, Score(rig.Players[1]).text);
        }

        static void AssertScoreReadable(Rig rig)
        {
            for (var i = 0; i < rig.Players.childCount; i++)
            {
                var pill = rig.Players[i];
                var scoreSize = Score(pill).resolvedStyle.fontSize;
                var nameSize = pill.Q<Label>(className: "pill-name").resolvedStyle.fontSize;
                Assert.Greater(scoreSize, nameSize * 1.6f, $"seat {i} score {scoreSize} name {nameSize}");
            }

            var lead = Score(rig.Players[0]).resolvedStyle.color;
            var other = Score(rig.Players[1]).resolvedStyle.color;
            Assert.Greater(lead.r, other.r, $"seat colors {lead} {other}");
        }

        static void AssertRow(Rig rig, bool scoreShown)
        {
            Assert.AreEqual(2, rig.Players.childCount);
            for (var i = 0; i < rig.Players.childCount; i++)
            {
                var pill = rig.Players[i];
                AssertShown(Score(pill), scoreShown, $"seat {i} score");
                AssertShown(Hearts(pill), !scoreShown, $"seat {i} hearts");
            }
        }

        static void AssertHearts(Rig rig, int lostOnFirst, int lostOnSecond)
        {
            AssertLost(rig.Players[0], lostOnFirst);
            AssertLost(rig.Players[1], lostOnSecond);
        }

        static void AssertLost(VisualElement pill, int lost)
        {
            var hearts = Hearts(pill);
            Assert.AreEqual(3, hearts.childCount);
            for (var i = 0; i < hearts.childCount; i++)
            {
                var spent = i >= hearts.childCount - lost;
                Assert.AreEqual(spent, hearts[i].ClassListContains("heart-lost"), $"heart {i}");
            }
        }

        static void AssertShown(VisualElement element, bool shown, string label)
        {
            Assert.AreEqual(!shown, element.ClassListContains("hidden"), label);
            var display = element.resolvedStyle.display;
            var expected = shown ? DisplayStyle.Flex : DisplayStyle.None;
            Assert.AreEqual(expected, display, $"{label} display {display}");
        }

        static Label Score(VisualElement pill)
        {
            var score = pill.Q<Label>(className: "pill-points");
            Assert.NotNull(score);
            return score;
        }

        static VisualElement Hearts(VisualElement pill)
        {
            var hearts = pill.Q(className: "pill-hearts");
            Assert.NotNull(hearts);
            return hearts;
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

                root = new GameObject("PointsHudRig");
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

            public VisualElement Tree
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement.Q("root") ?? document.rootVisualElement;
                }
            }

            public VisualElement Players => Tree.Q("players");

            public void KnockOut(int seat)
            {
                Director.DebugKnockOut(seat);
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

                PlayerPrefs.Save();

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
