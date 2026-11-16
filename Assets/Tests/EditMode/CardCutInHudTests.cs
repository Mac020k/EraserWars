using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Cards;
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
    public sealed class CardCutInHudTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";

        [Test]
        public void SignatureCutInShowsTheCardNameAndHidesTheTurnCallout()
        {
            using var rig = new Rig();
            var card = rig.GiveSignature("全力スマッシュ");
            rig.StartMatch();
            var clock = rig.Director.State.TurnTimeLeft;

            rig.Director.PlayCard(card.IdValue);
            rig.Hud.RefreshFromState();

            Assert.AreEqual(Phase.Aim, rig.Director.State.Phase);
            Assert.AreEqual("全力スマッシュ", rig.CutInName);
            Assert.AreEqual("P1  とくぎ", rig.CutInKicker);
            Assert.IsFalse(rig.CutInHidden, "The banner should be on screen.");
            Assert.IsTrue(rig.CalloutHidden, "The turn callout should hide during the banner.");
            Assert.IsTrue(rig.HandHidden, "The hand should hide during the banner.");
            Assert.IsTrue(rig.PassHidden, "Pass should hide during the banner.");
            Assert.IsNotNull(rig.FaceIcon, "The banner should paint the card icon.");

            var player = rig.Director.State.CurrentPlayerId;
            rig.Director.Pass();
            Assert.AreEqual(player, rig.Director.State.CurrentPlayerId);
            Assert.AreEqual(Phase.Aim, rig.Director.State.Phase);

            rig.Director.AdvanceMatchClock(0.9f);
            rig.Hud.RefreshFromState();
            Assert.IsTrue(rig.Director.IsCardCutInShowing, "A signature banner should still be up at 0.9 seconds.");
            Assert.AreEqual(clock, rig.Director.State.TurnTimeLeft, 0.001f);

            rig.Director.AdvanceMatchClock(0.4f);
            rig.Hud.RefreshFromState();
            Assert.IsFalse(rig.Director.IsCardCutInShowing);
            Assert.IsTrue(rig.CutInHidden);
            Assert.IsFalse(rig.CalloutHidden, "The turn callout should return when the banner ends.");
            Assert.IsFalse(rig.HandHidden);
            Assert.IsFalse(rig.PassHidden);
            Assert.AreEqual(Phase.Aim, rig.Director.State.Phase);

            var after = rig.Director.State.TurnTimeLeft;
            rig.Director.AdvanceMatchClock(0.2f);
            Assert.AreEqual(after - 0.2f, rig.Director.State.TurnTimeLeft, 0.02f);
        }

        [Test]
        public void CommonCutInEndsSoonerAndStillAcceptsAFlickCommand()
        {
            using var rig = new Rig();
            var card = rig.GiveCommon("ワックス");
            rig.StartMatch();

            rig.Director.PlayCard(card.IdValue);
            rig.Hud.RefreshFromState();
            Assert.AreEqual("ワックス", rig.CutInName);
            Assert.AreEqual("P1  カード", rig.CutInKicker);
            Assert.IsTrue(rig.CalloutHidden);

            rig.Director.SubmitFlick(new FlickCommand(rig.Director.State.CurrentPlayerId, 1f, 0f, 0.4f, 0f, 0f, 0f));
            Assert.AreEqual(Phase.Simulating, rig.Director.State.Phase, "A flick command during the banner should still land.");
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;
            readonly List<Object> owned = new();

            public Rig()
            {
                root = new GameObject("CardCutInRig");
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
                Assert.NotNull(HudDocument, "MatchHud did not build a visual tree.");
            }

            public string CutInName => HudDocument.Q<Label>("card-cutin-name")?.text;
            public string CutInKicker => HudDocument.Q<Label>("card-cutin-kicker")?.text;
            public bool CutInHidden => HudDocument.Q("card-cutin").ClassListContains("hidden");
            public bool CalloutHidden => HudDocument.Q("turn-callout").ClassListContains("hidden");
            public bool HandHidden => HudDocument.Q("hand-dock").ClassListContains("hidden");
            public bool PassHidden => HudDocument.Q<Button>("pass").ClassListContains("hidden");
            public VisualElement FaceIcon => HudDocument.Q("card-cutin-face")?.Q("card-icon");

            VisualElement HudDocument
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement?.Q("root") ?? document.rootVisualElement;
                }
            }

            public CardDefinition GiveSignature(string name)
            {
                var card = OwnedCard(name, true);
                var character = OwnedCharacter(card);
                Director.SetSeatRoster(new[] { character, character });
                return card;
            }

            public CardDefinition GiveCommon(string name)
            {
                var card = OwnedCard(name, false);
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

            CardDefinition OwnedCard(string name, bool signature)
            {
                var card = ScriptableObject.CreateInstance<CardDefinition>();
                owned.Add(card);
                card.IdValue = signature ? 42 : 7;
                card.DisplayNameJa = name;
                card.NameKey = name;
                card.IsSignature = signature;
                card.FamilyKey = signature ? CardFamilies.Signature : CardFamilies.Stationery;
                card.EffectKinds = new List<string>();
                return card;
            }

            CharacterDefinition OwnedCharacter(CardDefinition signature)
            {
                var character = ScriptableObject.CreateInstance<CharacterDefinition>();
                owned.Add(character);
                character.DisplayNameJa = "赤星";
                character.NameKey = "akaboshi";
                character.SignatureA = signature;
                return character;
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
