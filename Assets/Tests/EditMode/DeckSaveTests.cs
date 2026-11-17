using System.Collections.Generic;
using System.Reflection;
using EraserWars.Cards;
using EraserWars.Persistence;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class DeckSaveTests
    {
        readonly List<Object> owned = new();
        GameObject uiHost;
        bool hadPresets;
        string presets;

        [SetUp]
        public void ClearPresets()
        {
            hadPresets = PlayerPrefs.HasKey(DeckPresets.Key);
            presets = PlayerPrefs.GetString(DeckPresets.Key, string.Empty);
            PlayerPrefs.DeleteKey(DeckPresets.Key);
        }

        [TearDown]
        public void TearDown()
        {
            if (uiHost != null)
            {
                Object.DestroyImmediate(uiHost);
            }

            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            if (hadPresets)
            {
                PlayerPrefs.SetString(DeckPresets.Key, presets);
            }
            else
            {
                PlayerPrefs.DeleteKey(DeckPresets.Key);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void StartMenuShowsSaveBesideTheCostWarning()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/StartMenu.uxml");
            Assert.NotNull(tree);
            var root = tree.CloneTree();
            var budget = root.Q("deck-budget");
            Assert.AreEqual("保存", budget.Q<Button>("deck-save").text);
            Assert.NotNull(budget.Q<Label>("deck-budget-warning"));
            Assert.NotNull(root.Q("deck-portrait"));
            Assert.NotNull(root.Q("deck-signatures"));
            for (var i = 0; i < DeckPresets.SlotsPerCharacter; i++)
            {
                Assert.AreEqual((i + 1).ToString(), root.Q<Button>($"deck-preset-{i}").text);
            }
        }

        [Test]
        public void SameNameDeckIsNotStoredUntilACopyIsRemoved()
        {
            var heavy = Card("重い消しゴム", 1, 1, 1);
            var hero = Hero("広い", 0, 8);
            var catalog = Catalog(new[] { hero }, new[] { heavy });
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = hero;
            loadout.Commons[0].Add(heavy);
            loadout.Commons[0].Add(heavy);
            var panel = Mount();
            var deck = new DeckPanelController(catalog, loadout);
            deck.Bind(panel);
            deck.Refresh(2);

            var save = panel.Q<Button>("deck-save");
            var warning = panel.Q<Label>("deck-budget-warning");
            Assert.IsFalse(save.enabledSelf);
            Assert.AreEqual("重い消しゴム は1枚までです。", warning.text);
            Assert.IsFalse(warning.ClassListContains("hidden"));
            Assert.AreEqual(0, loadout.Commons[0].Count);
            Assert.IsFalse(deck.TrySave());
            Assert.AreEqual(0, loadout.Commons[0].Count);

            Click(RemoveFor(panel.Q("deck-card-list"), "重い消しゴム"));

            Assert.IsTrue(save.enabledSelf);
            Assert.IsTrue(warning.ClassListContains("hidden"));
            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreSame(heavy, loadout.Commons[0][0]);
            Assert.IsTrue(deck.TrySave());
            Assert.AreEqual(1, loadout.Commons[0].Count);
        }

        [Test]
        public void SwitchingCharacterLoadsThatCharactersPreset()
        {
            var heavy = Card("重い", 1, 3, 2);
            var mid = Card("中", 2, 2, 2);
            var decoy = Card("目印", 3, 1, 2);
            var wide = Hero("広い", 0, 8);
            wide.Eraser.Mass = 0.02f;
            var narrow = Hero("狭い", 1, 2);
            DeckPresets.SaveCards(wide.IdValue, 0, new[] { heavy.IdValue, mid.IdValue });
            DeckPresets.SaveCards(narrow.IdValue, 0, new[] { mid.IdValue });
            var catalog = Catalog(new[] { wide, narrow }, new[] { heavy, mid, decoy });
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = wide;
            loadout.Commons[0].Add(decoy);
            var panel = Mount();
            var deck = new DeckPanelController(catalog, loadout);
            deck.Bind(panel);
            deck.Refresh(2);

            Assert.AreSame(wide, loadout.Seats[0]);
            Assert.AreEqual(2, loadout.Commons[0].Count);
            Assert.AreSame(heavy, loadout.Commons[0][0]);
            Assert.AreSame(mid, loadout.Commons[0][1]);
            Assert.AreEqual("広い", panel.Q<Label>("deck-profile-name").text);
            Assert.AreEqual("全力アタック型", panel.Q<Label>("deck-archetype").text);
            Assert.AreEqual("質量 20 / コスト上限 8", panel.Q<Label>("deck-stats").text);
            Assert.AreEqual("得意 体育 / 苦手 算数", panel.Q<Label>("deck-flavor").text);
            Assert.AreEqual("プリセット1", panel.Q<Label>("deck-preset-name").text);

            Click(CharacterNamed(panel.Q("deck-character-list"), "狭い"));

            Assert.AreSame(narrow, loadout.Seats[0]);
            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreSame(mid, loadout.Commons[0][0]);
            Assert.AreEqual("狭い", panel.Q<Label>("deck-profile-name").text);
            Assert.AreEqual(heavy.IdValue, DeckPresets.LoadCards(wide.IdValue, 0)[0]);

            Click(CharacterNamed(panel.Q("deck-character-list"), "広い"));

            Assert.AreSame(wide, loadout.Seats[0]);
            Assert.AreEqual(2, loadout.Commons[0].Count);
            Assert.AreSame(heavy, loadout.Commons[0][0]);
            Assert.AreSame(mid, loadout.Commons[0][1]);
        }

        [Test]
        public void OverCostPresetDoesNotReplaceTheStoredSeat()
        {
            var heavy = Card("重い", 1, 3, 2);
            var mid = Card("中", 2, 2, 2);
            var wide = Hero("広い", 0, 8);
            var narrow = Hero("狭い", 1, 2);
            DeckPresets.SaveCards(narrow.IdValue, 0, new[] { heavy.IdValue, mid.IdValue });
            var catalog = Catalog(new[] { wide, narrow }, new[] { heavy, mid });
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = wide;
            loadout.Commons[0].Add(mid);
            var panel = Mount();
            var deck = new DeckPanelController(catalog, loadout);
            deck.Bind(panel);
            deck.Refresh(2);

            Click(CharacterNamed(panel.Q("deck-character-list"), "狭い"));

            var save = panel.Q<Button>("deck-save");
            var warning = panel.Q<Label>("deck-budget-warning");
            Assert.IsFalse(save.enabledSelf);
            Assert.AreEqual("上限を超えています。カードを外してください。", warning.text);
            Assert.AreEqual("上限オーバー", panel.Q<Label>("deck-budget-remain").text);
            Assert.AreSame(wide, loadout.Seats[0]);
            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreSame(mid, loadout.Commons[0][0]);
            Assert.IsFalse(deck.TrySave());
            Assert.AreSame(wide, loadout.Seats[0]);

            Click(RemoveFor(panel.Q("deck-card-list"), "重い"));

            Assert.IsTrue(save.enabledSelf);
            Assert.IsTrue(warning.ClassListContains("hidden"));
            Assert.AreSame(narrow, loadout.Seats[0]);
            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreSame(mid, loadout.Commons[0][0]);
            Assert.AreEqual(mid.IdValue, DeckPresets.LoadCards(narrow.IdValue, 0)[0]);
        }

        [Test]
        public void PresetTabsKeepFiveDecksForOneCharacter()
        {
            var alpha = Card("甲", 1, 1, 2);
            var beta = Card("乙", 2, 1, 2);
            var hero = Hero("広い", 7, 8);
            var catalog = Catalog(new[] { hero }, new[] { alpha, beta });
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = hero;
            loadout.Commons[0].Add(alpha);
            var panel = Mount();
            var deck = new DeckPanelController(catalog, loadout);
            deck.Bind(panel);
            deck.Refresh(2);

            Assert.IsTrue(deck.TrySave());
            Assert.AreEqual(alpha.IdValue, DeckPresets.LoadCards(hero.IdValue, 0)[0]);

            Click(panel.Q<Button>("deck-preset-1"));

            Assert.AreEqual(1, DeckPresets.GetActiveSlot(hero.IdValue));
            Assert.AreEqual(0, loadout.Commons[0].Count);
            Assert.AreEqual(alpha.IdValue, DeckPresets.LoadCards(hero.IdValue, 0)[0]);
            Assert.IsTrue(panel.Q<Button>("deck-preset-1").ClassListContains("selected"));
            Assert.AreEqual("プリセット2", panel.Q<Label>("deck-preset-name").text);

            Click(AddFor(panel.Q("deck-card-list"), "乙"));

            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreSame(beta, loadout.Commons[0][0]);
            Assert.AreEqual(beta.IdValue, DeckPresets.LoadCards(hero.IdValue, 1)[0]);

            Click(panel.Q<Button>("deck-preset-0"));
            Assert.AreSame(alpha, loadout.Commons[0][0]);
            Assert.AreEqual(0, DeckPresets.GetActiveSlot(hero.IdValue));

            Click(panel.Q<Button>("deck-preset-1"));
            Assert.AreSame(beta, loadout.Commons[0][0]);
            Assert.IsFalse(DeckPresets.HasSaved(hero.IdValue, 4));
            Assert.NotNull(panel.Q<Button>("deck-preset-4"));
        }

        [Test]
        public void LegalDeckStaysStoredWhenAnotherCopyIsRefused()
        {
            var heavy = Card("重い", 1, 1, 1);
            var hero = Hero("広い", 0, 8);
            var catalog = Catalog(new[] { hero }, new[] { heavy });
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = hero;
            loadout.Commons[0].Add(heavy);
            var panel = Mount();
            var deck = new DeckPanelController(catalog, loadout);
            deck.Bind(panel);
            deck.Refresh(2);

            var save = panel.Q<Button>("deck-save");
            var warning = panel.Q<Label>("deck-budget-warning");
            Assert.IsTrue(save.enabledSelf);
            Assert.IsTrue(warning.ClassListContains("hidden"));
            Assert.AreEqual(1, loadout.Commons[0].Count);

            Click(panel.Q("deck-card-list").Q<Button>(className: "deck-action-add"));

            Assert.IsTrue(save.enabledSelf);
            Assert.AreEqual("重い は1枚までです。", warning.text);
            Assert.IsFalse(warning.ClassListContains("hidden"));
            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.IsTrue(deck.TrySave());
            Assert.AreSame(heavy, loadout.Commons[0][0]);
        }

        ContentCatalog Catalog(CharacterDefinition[] characters, CardDefinition[] cards)
        {
            var catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            owned.Add(catalog);
            catalog.Characters = new List<CharacterDefinition>(characters);
            catalog.Cards = new List<CardDefinition>(cards);
            return catalog;
        }

        CardDefinition Card(string name, int id, int cost, int maxCopies)
        {
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            owned.Add(card);
            card.IdValue = id;
            card.NameKey = "card." + id;
            card.DisplayNameJa = name;
            card.BuildCost = cost;
            card.MaxCopies = maxCopies;
            card.FamilyKey = CardFamilies.Kiai;
            return card;
        }

        CharacterDefinition Hero(string name, int id, int cap)
        {
            var eraser = ScriptableObject.CreateInstance<EraserDefinition>();
            owned.Add(eraser);
            eraser.DeckCostCap = cap;
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            owned.Add(character);
            character.IdValue = id;
            character.DisplayNameJa = name;
            character.Eraser = eraser;
            character.UnlockedByDefault = true;
            character.ArchetypeKey = CharacterArchetypes.AllOut;
            return character;
        }

        VisualElement Mount()
        {
            uiHost = new GameObject("deck-save-ui");
            var document = uiHost.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            owned.Add(settings);
            document.panelSettings = settings;
            var panel = new VisualElement { name = "deck-panel" };
            panel.Add(new ScrollView { name = "deck-character-list" });
            panel.Add(new ScrollView { name = "deck-card-list" });
            panel.Add(new Label { name = "deck-status" });
            panel.Add(new Label { name = "deck-detail" });
            panel.Add(new Label { name = "deck-budget-label" });
            panel.Add(new Label { name = "deck-budget-remain" });
            panel.Add(new VisualElement { name = "deck-budget-fill" });
            panel.Add(new Label { name = "deck-budget-warning" });
            panel.Add(new Label { name = "deck-empty" });
            panel.Add(new Button { name = "deck-save", text = "保存" });
            panel.Add(new VisualElement { name = "deck-portrait" });
            panel.Add(new Label { name = "deck-profile-name" });
            panel.Add(new Label { name = "deck-archetype" });
            panel.Add(new Label { name = "deck-stats" });
            panel.Add(new Label { name = "deck-flavor" });
            panel.Add(new VisualElement { name = "deck-signatures" });
            panel.Add(new Label { name = "deck-preset-name" });
            for (var i = 0; i < DeckPresets.SlotsPerCharacter; i++)
            {
                panel.Add(new Button { name = $"deck-preset-{i}", text = $"{i + 1}" });
            }
            for (var i = 0; i < 4; i++)
            {
                panel.Add(new Button { name = $"deck-seat-{i}", text = $"P{i + 1}" });
            }

            document.rootVisualElement.Add(panel);
            Assert.IsNotNull(panel.panel, "Deck buttons need a panel so clicks reach them.");
            return panel;
        }

        static Button CharacterNamed(VisualElement list, string name)
        {
            var buttons = list.Query<Button>().ToList();
            foreach (var button in buttons)
            {
                if (button.text != null && button.text.StartsWith(name))
                {
                    return button;
                }
            }

            Assert.Fail("No character button for " + name);
            return null;
        }

        static Button RemoveFor(VisualElement list, string cardName)
        {
            var rows = list.Query(className: "deck-row").ToList();
            foreach (var row in rows)
            {
                var label = row.Q<Label>(className: "card-face-name");
                if (label != null && label.text == cardName)
                {
                    return row.Q<Button>(className: "deck-action-remove");
                }
            }

            Assert.Fail("No remove button for " + cardName);
            return null;
        }

        static Button AddFor(VisualElement list, string cardName)
        {
            var rows = list.Query(className: "deck-row").ToList();
            foreach (var row in rows)
            {
                var label = row.Q<Label>(className: "card-face-name");
                if (label != null && label.text == cardName)
                {
                    return row.Q<Button>(className: "deck-action-add");
                }
            }

            Assert.Fail("No add button for " + cardName);
            return null;
        }

        static void Click(Button button)
        {
            var field = button.clickable.GetType().GetField(
                "clicked",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var handler = field?.GetValue(button.clickable) as System.Delegate;
            Assert.IsNotNull(handler, "The deck control has no click handler.");
            handler.DynamicInvoke();
        }
    }
}
