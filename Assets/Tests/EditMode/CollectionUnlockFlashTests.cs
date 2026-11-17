using System.Collections.Generic;
using System.IO;
using EraserWars.Cards;
using EraserWars.Persistence;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class CollectionUnlockFlashTests
    {
        readonly List<Object> owned = new();
        GameObject uiHost;
        CollectionPanelController shown;
        bool hadGranted;
        bool hadFlashed;
        string granted;
        string flashed;

        [SetUp]
        public void ClearUnlocks()
        {
            hadGranted = PlayerPrefs.HasKey(CharacterUnlocks.GrantedKey);
            hadFlashed = PlayerPrefs.HasKey(CharacterUnlocks.FlashedKey);
            granted = PlayerPrefs.GetString(CharacterUnlocks.GrantedKey, string.Empty);
            flashed = PlayerPrefs.GetString(CharacterUnlocks.FlashedKey, string.Empty);
            PlayerPrefs.DeleteKey(CharacterUnlocks.GrantedKey);
            PlayerPrefs.DeleteKey(CharacterUnlocks.FlashedKey);
            CharacterAccess.SetDebugUnlockAll(false);
        }

        [TearDown]
        public void RestoreUnlocks()
        {
            CloseHost();
            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            RestoreKey(CharacterUnlocks.GrantedKey, hadGranted, granted);
            RestoreKey(CharacterUnlocks.FlashedKey, hadFlashed, flashed);
            CharacterAccess.SetDebugUnlockAll(false);
            PlayerPrefs.Save();
        }

        [Test]
        public void GrantRemembersTheIdAndTheFlashIsConsumedOnce()
        {
            Assert.IsFalse(CharacterUnlocks.IsEarned(4));
            Assert.IsFalse(CharacterUnlocks.HasPendingFlash(4));

            CharacterUnlocks.Grant(4);
            CharacterUnlocks.Grant(4);
            CharacterUnlocks.Grant(-1);

            Assert.IsTrue(CharacterUnlocks.IsEarned(4));
            Assert.IsTrue(CharacterUnlocks.HasPendingFlash(4));
            Assert.IsFalse(CharacterUnlocks.IsEarned(-1));
            Assert.IsTrue(CharacterUnlocks.ConsumeFlash(4));
            Assert.IsFalse(CharacterUnlocks.ConsumeFlash(4));
            Assert.IsTrue(CharacterUnlocks.IsEarned(4));
            Assert.IsFalse(CharacterUnlocks.HasPendingFlash(4));

            CharacterUnlocks.Grant(4);

            Assert.IsFalse(CharacterUnlocks.HasPendingFlash(4));
        }

        [Test]
        public void GrantedFaceFlashesOnceAndStaysUnlocked()
        {
            var catalog = Catalog();
            CharacterUnlocks.Grant(4);
            var panel = Open(catalog);
            var earned = FaceNamed(panel, "新人");
            var starter = FaceNamed(panel, "赤星");

            Assert.IsNotNull(earned.panel);
            Assert.IsNotNull(earned.Q(className: "char-tile-art"));
            Assert.IsTrue(earned.ClassListContains(CollectionPanelController.UnlockFlashClass));
            Assert.IsFalse(earned.ClassListContains("locked"));
            Assert.AreEqual("新人", earned.Q<Label>(className: "char-tile-name").text);
            Assert.IsFalse(starter.ClassListContains(CollectionPanelController.UnlockFlashClass));
            Assert.IsFalse(CharacterUnlocks.HasPendingFlash(4));

            shown.SelectCharacterById(0);

            earned = FaceNamed(panel, "新人");
            Assert.IsTrue(earned.ClassListContains(CollectionPanelController.UnlockFlashClass));
            Assert.AreEqual("赤星", FaceNamed(panel, "赤星").Q<Label>(className: "char-tile-name").text);

            CharacterUnlocks.Grant(4);
            panel = Open(catalog);
            earned = FaceNamed(panel, "新人");

            Assert.IsFalse(earned.ClassListContains(CollectionPanelController.UnlockFlashClass));
            Assert.IsFalse(earned.ClassListContains("locked"));
            Assert.AreEqual("新人", earned.Q<Label>(className: "char-tile-name").text);
            Assert.IsFalse(FaceNamed(panel, "赤星").ClassListContains(CollectionPanelController.UnlockFlashClass));
        }

        [Test]
        public void UnlockFlashRuleScalesTheFace()
        {
            var uss = File.ReadAllText(Path.Combine(Application.dataPath, "UI", "StartMenu.uss"));
            var rule = uss.IndexOf(".char-tile.unlock-flash {", System.StringComparison.Ordinal);
            var face = uss.IndexOf(".char-tile.unlock-flash .char-tile-art", System.StringComparison.Ordinal);

            Assert.GreaterOrEqual(rule, 0);
            Assert.Greater(face, rule);
            Assert.IsTrue(uss.Substring(rule, face - rule).Contains("scale: 1.08 1.08"));
        }

        [Test]
        public void UngrantedRosterDoesNotFlash()
        {
            var panel = Open(Catalog());
            var starter = FaceNamed(panel, "赤星");
            var locked = LockedFace(panel);

            Assert.IsFalse(starter.ClassListContains(CollectionPanelController.UnlockFlashClass));
            Assert.IsFalse(starter.ClassListContains("locked"));
            Assert.AreEqual("赤星", starter.Q<Label>(className: "char-tile-name").text);
            Assert.IsTrue(locked.ClassListContains("locked"));
            Assert.IsFalse(locked.ClassListContains(CollectionPanelController.UnlockFlashClass));
            Assert.AreEqual("？？", locked.Q<Label>(className: "char-tile-name").text);
            Assert.IsFalse(CharacterUnlocks.HasPendingFlash(4));
        }

        ContentCatalog Catalog()
        {
            var catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            owned.Add(catalog);
            catalog.Characters = new List<CharacterDefinition>
            {
                Character(0, "赤星", true),
                Character(4, "新人", false),
            };
            return catalog;
        }

        CharacterDefinition Character(int id, string name, bool unlocked)
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            owned.Add(character);
            character.IdValue = id;
            character.DisplayNameJa = name;
            character.UnlockedByDefault = unlocked;
            character.ArchetypeKey = CharacterArchetypes.AllOut;
            return character;
        }

        VisualElement Open(ContentCatalog catalog)
        {
            CloseHost();
            uiHost = new GameObject("collection-unlock-ui");
            var document = uiHost.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            owned.Add(settings);
            document.panelSettings = settings;
            var panel = new VisualElement { name = "collection-panel" };
            panel.Add(new Button { name = "collection-tab-characters" });
            panel.Add(new Button { name = "collection-tab-cards" });
            panel.Add(new VisualElement { name = "collection-filters" });
            panel.Add(new VisualElement { name = "collection-strip" });
            panel.Add(new Label { name = "collection-detail-title" });
            panel.Add(new Label { name = "collection-detail-meta" });
            panel.Add(new Label { name = "collection-detail-body" });
            panel.Add(new Label { name = "collection-detail-unlock" });
            panel.Add(new VisualElement { name = "collection-portrait" });
            document.rootVisualElement.Add(panel);
            shown = new CollectionPanelController(catalog);
            shown.Bind(panel);
            return panel;
        }

        void CloseHost()
        {
            if (uiHost == null)
            {
                return;
            }

            Object.DestroyImmediate(uiHost);
            uiHost = null;
        }

        static Button FaceNamed(VisualElement panel, string name)
        {
            var tiles = panel.Q("collection-strip").Query<Button>(className: "char-tile").ToList();
            foreach (var tile in tiles)
            {
                var label = tile.Q<Label>(className: "char-tile-name");
                if (label != null && label.text == name)
                {
                    return tile;
                }
            }

            Assert.Fail("No face named " + name);
            return null;
        }

        static Button LockedFace(VisualElement panel)
        {
            var tiles = panel.Q("collection-strip").Query<Button>(className: "char-tile").ToList();
            foreach (var tile in tiles)
            {
                if (tile.ClassListContains("locked"))
                {
                    return tile;
                }
            }

            Assert.Fail("No locked face");
            return null;
        }

        static void RestoreKey(string key, bool had, string value)
        {
            if (had)
            {
                PlayerPrefs.SetString(key, value);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}
