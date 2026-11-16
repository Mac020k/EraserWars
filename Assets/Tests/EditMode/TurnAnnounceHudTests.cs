using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using EraserWars.Persistence;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class TurnAnnounceHudTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";

        [Test]
        public void OpeningSeatShowsTheLineBesideTheCallout()
        {
            using var rig = new Rig();
            rig.StartMatch();
            rig.Hud.RefreshFromState();

            Assert.AreEqual("P1 のばん", rig.AnnounceText);
            Assert.AreEqual(rig.CalloutText, rig.AnnounceText);
            Assert.IsFalse(rig.AnnounceHidden);
            Assert.IsFalse(rig.CalloutHidden);
            Assert.IsTrue(rig.AnnounceHasSeat(0));
            Assert.IsFalse(rig.AnnounceHasSeat(1));
            Assert.AreEqual(PickingMode.Ignore, rig.Announce.pickingMode);

            rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
            rig.Hud.RefreshFromState();
            Assert.IsTrue(rig.AnnounceHidden);
            Assert.IsFalse(rig.CalloutHidden);
            Assert.AreEqual("P1 のばん", rig.CalloutText);
        }

        [Test]
        public void PassNamesTheNextSeatForOneSecond()
        {
            using var rig = new Rig();
            rig.StartMatch();
            rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
            rig.Director.Pass();
            rig.Hud.RefreshFromState();

            Assert.AreEqual(1, rig.Director.State.CurrentPlayerId.Value);
            Assert.AreEqual("P2 のばん", rig.AnnounceText);
            Assert.AreEqual(rig.CalloutText, rig.AnnounceText);
            Assert.IsFalse(rig.AnnounceHidden);
            Assert.IsTrue(rig.AnnounceHasSeat(1));
            Assert.IsFalse(rig.AnnounceHasSeat(0));
            Assert.IsTrue(rig.Director.TryGetTurnAnnounce(out var announce));
            Assert.AreEqual(TurnAnnounce.ShowSeconds, announce.RemainingSeconds, 0.001f);

            rig.Director.AdvanceMatchClock(TurnAnnounce.ShowSeconds);
            rig.Hud.RefreshFromState();
            Assert.IsFalse(rig.Director.TryGetTurnAnnounce(out _));
            Assert.IsTrue(rig.AnnounceHidden);
            Assert.IsFalse(rig.CalloutHidden);
        }

        [Test]
        public void CutInCoversTheAnnounceAndDoesNotSpendItsSecond()
        {
            using var rig = new Rig();
            var card = rig.GiveCommon("ワックス");
            rig.StartMatch();
            rig.Director.PlayCard(card.IdValue);
            rig.Hud.RefreshFromState();

            Assert.IsFalse(rig.CutInHidden);
            Assert.IsTrue(rig.AnnounceHidden);
            Assert.IsTrue(rig.CalloutHidden);
            Assert.IsTrue(rig.Director.TryGetTurnAnnounce(out var parked));
            Assert.AreEqual(TurnAnnounce.ShowSeconds, parked.RemainingSeconds, 0.001f);

            rig.Director.AdvanceMatchClock(0.5f);
            rig.Hud.RefreshFromState();
            Assert.IsTrue(rig.Director.IsCardCutInShowing);
            Assert.IsTrue(rig.AnnounceHidden);
            Assert.IsTrue(rig.Director.TryGetTurnAnnounce(out var stillParked));
            Assert.AreEqual(TurnAnnounce.ShowSeconds, stillParked.RemainingSeconds, 0.001f);

            rig.Director.AdvanceMatchClock(0.4f);
            rig.Hud.RefreshFromState();
            Assert.IsFalse(rig.Director.IsCardCutInShowing);
            Assert.IsFalse(rig.AnnounceHidden);
            Assert.AreEqual("P1 のばん", rig.AnnounceText);
            Assert.IsTrue(rig.Director.TryGetTurnAnnounce(out var resumed));
            Assert.AreEqual(TurnAnnounce.ShowSeconds - 0.4f, resumed.RemainingSeconds, 0.001f);
        }

        [Test]
        public void OpenMenuDoesNotSpendTheAnnounce()
        {
            using var rig = new Rig();
            rig.StartMatch();
            rig.Director.SetStartMenuOpen(true);
            rig.Director.AdvanceMatchClock(0.5f);
            Assert.IsTrue(rig.Director.TryGetTurnAnnounce(out var parked));
            Assert.AreEqual(TurnAnnounce.ShowSeconds, parked.RemainingSeconds, 0.001f);

            rig.Director.SetStartMenuOpen(false);
            rig.Director.AdvanceMatchClock(0.5f);
            Assert.IsTrue(rig.Director.TryGetTurnAnnounce(out var running));
            Assert.AreEqual(TurnAnnounce.ShowSeconds - 0.5f, running.RemainingSeconds, 0.001f);
        }

        [Test]
        public void MatchEndHidesTheAnnounceAndLeavesTheFinishCard()
        {
            var previous = PlayerPrefs.GetInt(MatchDirector.HotseatWinModeKey, (int)WinMode.Stock);
            var hadXp = PlayerPrefs.HasKey(CareerXp.TotalKey);
            var previousXp = PlayerPrefs.GetInt(CareerXp.TotalKey, 0);
            PlayerPrefs.SetInt(MatchDirector.HotseatWinModeKey, (int)WinMode.Points);
            try
            {
                using var rig = new Rig();
                rig.StartMatch();
                var guard = 0;
                while (rig.Director.State.Phase != Phase.MatchEnd && guard < 24)
                {
                    rig.Director.Pass();
                    guard++;
                }

                rig.Hud.RefreshFromState();
                Assert.AreEqual(Phase.MatchEnd, rig.Director.State.Phase);
                Assert.IsFalse(rig.Director.TryGetTurnAnnounce(out _));
                Assert.IsTrue(rig.AnnounceHidden);
                Assert.IsTrue(rig.CalloutHidden);
                Assert.IsFalse(rig.MatchEndHidden);
            }
            finally
            {
                PlayerPrefs.SetInt(MatchDirector.HotseatWinModeKey, previous);
                if (hadXp)
                {
                    PlayerPrefs.SetInt(CareerXp.TotalKey, previousXp);
                }
                else
                {
                    PlayerPrefs.DeleteKey(CareerXp.TotalKey);
                }

                PlayerPrefs.Save();
            }
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;
            readonly List<Object> owned = new();

            public Rig()
            {
                root = new GameObject("TurnAnnounceRig");
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
                Director = directorGo.AddComponent<MatchDirector>();
                SetField(Director, "spawnPoints", spawns);
                SetField(Director, "eraserPrefab", eraserPrefab);
                SetField(Director, "showDebug", false);
                directorGo.SetActive(true);

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
                Assert.NotNull(HudDocument, "MatchHud did not build a visual tree.");
            }

            public VisualElement Announce => HudDocument.Q("turn-announce");
            public string AnnounceText => HudDocument.Q<Label>("turn-announce-line")?.text;
            public bool AnnounceHidden => Announce.ClassListContains("hidden");
            public bool AnnounceHasSeat(int seat) => Announce.ClassListContains($"seat-{seat}");
            public string CalloutText => HudDocument.Q<Label>("turn")?.text;
            public bool CalloutHidden => HudDocument.Q("turn-callout").ClassListContains("hidden");
            public bool CutInHidden => HudDocument.Q("card-cutin").ClassListContains("hidden");
            public bool MatchEndHidden => HudDocument.Q("match-end").ClassListContains("hidden");

            VisualElement HudDocument
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement?.Q("root") ?? document.rootVisualElement;
                }
            }

            public CardDefinition GiveCommon(string name)
            {
                var card = ScriptableObject.CreateInstance<CardDefinition>();
                owned.Add(card);
                card.IdValue = 7;
                card.DisplayNameJa = name;
                card.NameKey = name;
                card.IsSignature = false;
                card.FamilyKey = CardFamilies.Stationery;
                card.EffectKinds = new List<string>();
                Director.SetSeatRoster(new CharacterDefinition[] { null, null });
                Director.SetSeatCommons(0, new[] { card });
                return card;
            }

            public void StartMatch()
            {
                Director.SetStartMenuOpen(false);
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                Director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, Director.State.Phase);
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

                for (var i = 0; i < owned.Count; i++)
                {
                    if (owned[i] != null)
                    {
                        Object.DestroyImmediate(owned[i]);
                    }
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
