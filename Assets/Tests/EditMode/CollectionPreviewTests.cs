using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EraserWars.Cards;
using EraserWars.Persistence;
using EraserWars.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class CollectionPreviewTests
    {
        readonly List<Object> owned = new();
        GameObject uiHost;
        bool hadGranted;
        bool hadFlashed;
        string granted;
        string flashed;

        [SetUp]
        public void ClearDebugAndUnlocks()
        {
            CharacterAccess.SetDebugUnlockAll(false);
            hadGranted = PlayerPrefs.HasKey(CharacterUnlocks.GrantedKey);
            hadFlashed = PlayerPrefs.HasKey(CharacterUnlocks.FlashedKey);
            granted = PlayerPrefs.GetString(CharacterUnlocks.GrantedKey, string.Empty);
            flashed = PlayerPrefs.GetString(CharacterUnlocks.FlashedKey, string.Empty);
            PlayerPrefs.DeleteKey(CharacterUnlocks.GrantedKey);
            PlayerPrefs.DeleteKey(CharacterUnlocks.FlashedKey);
        }

        [TearDown]
        public void Restore()
        {
            if (uiHost != null)
            {
                Object.DestroyImmediate(uiHost);
                uiHost = null;
            }

            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            CharacterAccess.SetDebugUnlockAll(false);
            if (hadGranted)
            {
                PlayerPrefs.SetString(CharacterUnlocks.GrantedKey, granted);
            }
            else
            {
                PlayerPrefs.DeleteKey(CharacterUnlocks.GrantedKey);
            }

            if (hadFlashed)
            {
                PlayerPrefs.SetString(CharacterUnlocks.FlashedKey, flashed);
            }
            else
            {
                PlayerPrefs.DeleteKey(CharacterUnlocks.FlashedKey);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void DisplayedMassScalesByOneThousandAndLeavesTheDefinition()
        {
            var eraser = ScriptableObject.CreateInstance<EraserDefinition>();
            owned.Add(eraser);
            eraser.Mass = 0.02f;

            Assert.AreEqual(20, CollectionDisplay.DisplayedMass(0.02f));
            Assert.AreEqual(32, CollectionDisplay.DisplayedMass(0.032f));
            Assert.AreEqual(20, CollectionDisplay.DisplayedMass(eraser.Mass));
            Assert.AreEqual(0.02f, eraser.Mass);
        }

        [Test]
        public void StillFrameMatchesPortraitAspectAndClips()
        {
            var stage = new VisualElement();
            CollectionDisplay.FitStillFrame(stage, 40f, 80f);

            Assert.AreEqual(170f, stage.style.width.value.value, 0.01f);
            Assert.AreEqual(340f, stage.style.height.value.value, 0.01f);
            Assert.AreEqual(Overflow.Hidden, stage.style.overflow.value);

            var uss = File.ReadAllText(Path.Combine(Application.dataPath, "UI", "StartMenu.uss"));
            var portrait = uss.IndexOf(".collection-portrait {", System.StringComparison.Ordinal);
            var stageRule = uss.IndexOf(".collection-stage {", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(portrait, 0);
            Assert.GreaterOrEqual(stageRule, 0);
            Assert.IsTrue(uss.Substring(portrait, 500).Contains("overflow: hidden"));
            Assert.IsTrue(uss.Substring(stageRule, 300).Contains("overflow: hidden"));
            var tileArt = uss.IndexOf(".char-tile-art {", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(tileArt, 0);
            var tileArtBlock = uss.Substring(tileArt, 280);
            Assert.IsTrue(tileArtBlock.Contains("background-size: 780% 780%"));
            Assert.IsFalse(tileArtBlock.Contains("scale-and-crop"));
            Assert.IsTrue(uss.Contains(".collection-strip .char-tile:hover"));
            var stripRule = uss.IndexOf(".collection-strip {", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(stripRule, 0);
            Assert.IsTrue(uss.Substring(stripRule, 280).Contains("padding-top: 16px"));
        }

        [Test]
        public void FlavorTablesCoverSubjectsAndPassiveNames()
        {
            Assert.IsTrue(CharacterFlavor.TrySubjects(2, out var strong, out var weak));
            Assert.AreEqual("算数", strong);
            Assert.AreEqual("体育", weak);
            Assert.IsFalse(CharacterFlavor.TrySubjects(99, out _, out _));
            Assert.IsTrue(CharacterFlavor.TryPlace(0, out var school, out var homeroom));
            Assert.AreEqual("ひまわり小学校", school);
            Assert.AreEqual("3年2組", homeroom);
            Assert.AreEqual("ひまわり小学校 3年2組", CharacterFlavor.TileCaption(0));
            Assert.AreEqual("留学生の先生役", CharacterFlavor.TileCaption(7));
            Assert.AreEqual("先生役", CharacterFlavor.TileCaption(15));
            Assert.AreEqual(string.Empty, CharacterFlavor.TileCaption(99));
            for (var id = 0; id <= 19; id++)
            {
                Assert.IsTrue(CharacterFlavor.TryPlace(id, out school, out homeroom), "place " + id);
                Assert.IsFalse(string.IsNullOrEmpty(homeroom), "class " + id);
                var teacher = id == 7 || id == 15 || id == 16 || id == 17;
                if (teacher)
                {
                    Assert.AreEqual(string.Empty, school, "school " + id);
                }
                else
                {
                    Assert.IsTrue(school.EndsWith("小学校"), "school " + id);
                }
            }

            PassiveText.Describe("passive.knock_plus", out var name, out var effect);
            Assert.AreEqual("どすこい", name);
            Assert.IsTrue(effect.Contains("弾き"));
            PassiveText.Describe("passive.none", out name, out effect);
            Assert.AreEqual("なし", name);
            PassiveText.Describe("passive.unknown_key", out name, out _);
            Assert.AreEqual("passive.unknown_key", name);
        }

        [Test]
        public void CollectionPreviewShowsMassSubjectsPassiveCardsAndPreviewSlots()
        {
            var catalog = Catalog();
            var panel = Open();
            var shown = new CollectionPanelController(catalog);
            shown.Bind(panel);

            var openFace = FaceNamed(panel, "水無月");
            var lockedFace = FaceNamed(panel, "？？");
            Assert.IsNotNull(openFace.Q(className: "arch-seat-take"));
            Assert.IsNull(lockedFace.Q(className: "arch-mark"));
            var lockedArt = lockedFace.Q(className: "char-tile-art");
            Assert.IsNotNull(lockedArt.Q(LockMark.ElementName));
            Assert.AreEqual(StyleKeyword.None, lockedArt.style.backgroundImage.keyword);
            Assert.IsNull(openFace.Q(className: "char-tile-art").Q(LockMark.ElementName));
            Assert.IsNotNull(panel.Q(className: "arch-all-out"));
            Assert.IsNotNull(panel.Q(className: "arch-teacher"));
            Assert.IsNotNull(panel.Q(className: "arch-afterschool"));
            Assert.IsNotNull(panel.Q(className: "arch-desk-king"));

            shown.SelectCharacterById(2);

            Assert.AreEqual("水無月", panel.Q<Label>("collection-detail-title").text);
            Assert.IsTrue(panel.Q<Label>("collection-detail-meta").text.Contains("質量 20"));
            Assert.IsFalse(panel.Q<Label>("collection-detail-meta").text.Contains("0.02"));
            Assert.AreEqual("得意 算数", panel.Q<Label>(className: "collection-subject-strong").text);
            Assert.AreEqual("苦手 体育", panel.Q<Label>(className: "collection-subject-weak").text);
            Assert.IsTrue(panel.Q<Label>("collection-passive").text.Contains("どすこい"));
            Assert.IsFalse(panel.Q<Label>("collection-passive").text.Contains("passive.knock_plus"));
            Assert.IsNotNull(panel.Q("collection-signatures").Q(className: "card-face"));
            Assert.IsTrue(panel.Q<Label>(className: "collection-signature-effect").text.Contains("追加フリック"));

            var stage = panel.Q("collection-stage");
            Assert.AreEqual(170f, stage.style.width.value.value, 0.01f);
            Assert.AreEqual(340f, stage.style.height.value.value, 0.01f);

            Click(panel.Q<Button>("collection-mode-3d"));
            Assert.AreEqual(CollectionPreviewMode.Eraser3D, shown.PreviewMode);
            Assert.IsFalse(panel.Q("collection-eraser-3d").ClassListContains("hidden"));
            Assert.IsTrue(panel.Q("collection-eraser-design").ClassListContains("hidden"));
            Assert.IsTrue(panel.Q<Label>("collection-eraser-3d-slot").text.Contains("モデル枠"));
            Assert.IsTrue(panel.Q<Label>("collection-eraser-3d-slot").text.Contains("水無月"));

            Click(panel.Q<Button>("collection-mode-design"));
            Assert.AreEqual(CollectionPreviewMode.EraserDesign, shown.PreviewMode);
            Assert.IsTrue(panel.Q<Label>("collection-eraser-design-slot").text.Contains("デザイン枠"));
            Assert.IsFalse(panel.Q("collection-eraser-design").ClassListContains("hidden"));
        }

        [Test]
        public void SelectingACharacterKeepsOneTilePerCharacter()
        {
            var catalog = Catalog();
            var panel = Open();
            var shown = new CollectionPanelController(catalog);
            shown.Bind(panel);

            Assert.AreEqual(3, TileCount(panel));
            shown.SelectCharacterById(2);
            Assert.AreEqual(3, TileCount(panel));
            Assert.AreEqual(1, SelectedCount(panel));
            Assert.IsTrue(FaceNamed(panel, "水無月").ClassListContains("selected"));

            Click(FaceNamed(panel, "赤星"));
            Assert.AreEqual(3, TileCount(panel));
            Assert.AreEqual(1, SelectedCount(panel));
            Assert.IsTrue(FaceNamed(panel, "赤星").ClassListContains("selected"));
            Assert.IsFalse(FaceNamed(panel, "水無月").ClassListContains("selected"));
            Assert.AreEqual("赤星", panel.Q<Label>("collection-detail-title").text);

            var akaboshi = FaceNamed(panel, "赤星");
            Assert.IsNull(akaboshi.Q(className: "char-tile-arch"));
            var badge = akaboshi.Q(className: "char-tile-badge");
            Assert.IsNotNull(badge);
            Assert.AreSame(akaboshi, badge.parent);
            Assert.AreNotSame(akaboshi.Q(className: "char-tile-art"), badge.parent);
            Assert.AreEqual("ひまわり小学校 3年2組", akaboshi.Q<Label>(className: "char-tile-place").text);
            Assert.AreEqual("？？？", FaceNamed(panel, "？？").Q<Label>(className: "char-tile-place").text);

            var allOut = FilterNamed(panel, "全力");
            Assert.AreEqual(string.Empty, allOut.text);
            Assert.IsTrue(allOut.ClassListContains("filter-all-out"));
            var icon = allOut.Q(className: "collection-filter-icon");
            var caption = allOut.Q<Label>(className: "collection-filter-label");
            Assert.AreEqual("全力", caption.text);
            Assert.Less(allOut.IndexOf(icon), allOut.IndexOf(caption));

            shown.ShowCards();
            var kiai = FilterNamed(panel, "気合");
            Assert.IsTrue(kiai.ClassListContains("filter-kiai"));
            Assert.AreEqual("気合", kiai.Q<Label>(className: "collection-filter-label").text);
            Assert.IsNull(kiai.Q(className: "arch-mark"));
            Assert.AreEqual(0, TileCount(panel));
        }

        [Test]
        public void CardModeShowsLargeFaceFamilyChipsAndPerformanceSlots()
        {
            var catalog = Catalog();
            var homework = ScriptableObject.CreateInstance<CardDefinition>();
            owned.Add(homework);
            homework.IdValue = 7;
            homework.DisplayNameJa = "先生の目";
            homework.FamilyKey = CardFamilies.Homework;
            homework.BuildCost = 2;
            homework.EffectKinds = new List<string> { "ModifyRule:OpponentTurnTime:0.5:1" };
            var weapon = ScriptableObject.CreateInstance<CardDefinition>();
            owned.Add(weapon);
            weapon.IdValue = 23;
            weapon.DisplayNameJa = "鉛筆の芯";
            weapon.FamilyKey = CardFamilies.Stationery;
            weapon.EffectKinds = new List<string> { "GrantWeapon:0.1:1" };
            catalog.Cards = new List<CardDefinition> { homework, weapon };

            var panel = Open();
            var shown = new CollectionPanelController(catalog);
            shown.Bind(panel);
            Assert.IsTrue(panel.Q("collection-card-slots").ClassListContains("hidden"));
            Assert.IsFalse(panel.Q("collection-preview-modes").ClassListContains("hidden"));

            shown.ShowCards();

            Assert.IsFalse(panel.Q("collection-card-slots").ClassListContains("hidden"));
            Assert.IsTrue(panel.Q("collection-preview-modes").ClassListContains("hidden"));
            Assert.AreEqual("付与演出", panel.Q<Label>("collection-grant-kicker").text);
            Assert.AreEqual("プレイ演出", panel.Q<Label>("collection-play-kicker").text);
            Assert.AreEqual("先生の目", panel.Q<Label>("collection-grant-caption").text);
            Assert.AreEqual("先生の目", panel.Q<Label>("collection-play-caption").text);
            Assert.IsNotNull(panel.Q("collection-filters").Q(className: "filter-kiai"));
            Assert.IsNotNull(panel.Q("collection-filters").Q(className: "filter-homework"));
            Assert.IsNotNull(panel.Q("collection-filters").Q(className: "filter-signature"));

            var hero = panel.Q("collection-hero-card");
            Assert.IsNotNull(hero);
            Assert.IsTrue(hero.ClassListContains("collection-hero-card"));
            Assert.IsFalse(hero.ClassListContains("card-tile"));
            Assert.AreEqual("先生の目", hero.Q<Label>(className: "card-face-name").text);
            var stage = panel.Q("collection-stage");
            Assert.AreEqual(CollectionDisplay.CardFrameWidth, stage.style.width.value.value, 0.01f);
            Assert.AreEqual(CollectionDisplay.CardFrameHeight, stage.style.height.value.value, 0.01f);

            var faces = StripCardTiles(panel);
            Assert.AreEqual(2, faces.Count);
            Assert.IsTrue(faces[0].ClassListContains("selected"));
            Click(faces[1]);
            Assert.AreEqual("鉛筆の芯", panel.Q("collection-hero-card").Q<Label>(className: "card-face-name").text);
            Assert.AreEqual("鉛筆の芯", panel.Q<Label>("collection-play-caption").text);

            var uss = File.ReadAllText(Path.Combine(Application.dataPath, "UI", "StartMenu.uss"));
            Assert.IsTrue(uss.Contains(".collection-filter.filter-kiai"));
            Assert.IsTrue(uss.Contains("rgb(214, 24, 40)"));
            Assert.IsTrue(uss.Contains(".collection-filter.filter-homework"));
            Assert.IsTrue(uss.Contains("rgb(12, 92, 214)"));
        }

        [Test]
        public void DebugUnlockOpensEveryCharacterAndEarnedUnlocksRemain()
        {
            var catalog = Catalog();
            var panel = Open();
            var shown = new CollectionPanelController(catalog);
            shown.Bind(panel);
            var home = SeatHome();
            var loadout = new HotseatLoadout();
            loadout.EnsureDefaults(catalog);
            var locked = catalog.FindCharacter(4);

            var pick = new SeatPickController(catalog, loadout);
            pick.Bind(home);
            pick.Refresh(2);
            pick.ToggleSeat(0);

            Assert.IsTrue(FaceNamed(panel, "？？").ClassListContains("locked"));
            Assert.IsFalse(loadout.TryAssignUnlocked(0, locked));
            Assert.IsNull(home.Q("seat-option-4"));

            CharacterAccess.SetDebugUnlockAll(true);
            shown.Refresh();
            pick.Refresh(2);

            Assert.IsNotNull(FaceNamed(panel, "新人"));
            Assert.IsFalse(FaceNamed(panel, "新人").ClassListContains("locked"));
            Assert.IsNull(FaceNamed(panel, "新人").Q(LockMark.ElementName));
            Assert.IsTrue(loadout.TryAssignUnlocked(0, locked));
            Assert.AreSame(locked, loadout.Seats[0]);
            Assert.IsNotNull(home.Q("seat-option-4"));

            CharacterAccess.SetDebugUnlockAll(false);
            loadout.ReleaseLockedSeats(catalog);
            shown.Refresh();

            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[0]);
            Assert.IsFalse(loadout.TryAssignUnlocked(0, locked));
            Assert.IsTrue(FaceNamed(panel, "？？").ClassListContains("locked"));

            CharacterUnlocks.Grant(4);
            shown.Refresh();

            Assert.IsFalse(FaceNamed(panel, "新人").ClassListContains("locked"));
            Assert.IsTrue(loadout.TryAssignUnlocked(1, locked));
            Assert.AreSame(locked, loadout.Seats[1]);
        }

        ContentCatalog Catalog()
        {
            var catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            owned.Add(catalog);
            var eraser = ScriptableObject.CreateInstance<EraserDefinition>();
            owned.Add(eraser);
            eraser.Mass = 0.02f;
            eraser.Friction = 0.4f;
            eraser.Power = 1f;
            eraser.Speed = 1f;
            eraser.PassiveKey = "passive.knock_plus";
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            owned.Add(card);
            card.DisplayNameJa = "もう一押し";
            card.IsSignature = true;
            card.FamilyKey = CardFamilies.Signature;
            card.EffectKinds = new List<string> { "ExtraFlick" };
            catalog.Characters = new List<CharacterDefinition>
            {
                Character(0, "赤星", true, CharacterArchetypes.AllOut, null, null),
                Character(2, "水無月", true, CharacterArchetypes.SeatTake, eraser, card),
                Character(4, "新人", false, CharacterArchetypes.Teacher, eraser, card),
            };
            return catalog;
        }

        CharacterDefinition Character(
            int id,
            string name,
            bool unlocked,
            string archetype,
            EraserDefinition eraser,
            CardDefinition signature)
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            owned.Add(character);
            character.IdValue = id;
            character.DisplayNameJa = name;
            character.UnlockedByDefault = unlocked;
            character.ArchetypeKey = archetype;
            character.Eraser = eraser;
            character.SignatureA = signature;
            character.Portrait = Portrait();
            return character;
        }

        Sprite Portrait()
        {
            var texture = new Texture2D(40, 80);
            owned.Add(texture);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 40, 80), new Vector2(0.5f, 0.5f));
            owned.Add(sprite);
            return sprite;
        }

        VisualElement SeatHome()
        {
            var home = new VisualElement { name = "home-stack" };
            var picker = new VisualElement { name = "seat-picker" };
            picker.AddToClassList("hidden");
            picker.Add(new Label { name = "seat-picker-caption" });
            picker.Add(new VisualElement { name = "seat-picker-row" });
            home.Add(new VisualElement { name = "seat-strip" });
            home.Add(picker);
            uiHost.GetComponent<UIDocument>().rootVisualElement.Add(home);
            return home;
        }

        VisualElement Open()
        {
            uiHost = new GameObject("collection-preview-ui");
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
            Assert.IsNotNull(panel.panel);
            return panel;
        }

        static int TileCount(VisualElement panel)
        {
            return panel.Q("collection-strip").Query<Button>(className: "char-tile").ToList().Count;
        }

        static int SelectedCount(VisualElement panel)
        {
            var selected = 0;
            var tiles = panel.Q("collection-strip").Query<Button>(className: "char-tile").ToList();
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].ClassListContains("selected"))
                {
                    selected++;
                }
            }

            return selected;
        }

        static Button FilterNamed(VisualElement panel, string label)
        {
            var chips = panel.Q("collection-filters").Query<Button>(className: "collection-filter").ToList();
            foreach (var chip in chips)
            {
                var caption = chip.Q<Label>(className: "collection-filter-label");
                if (caption != null && caption.text == label)
                {
                    return chip;
                }
            }

            Assert.Fail("No filter named " + label);
            return null;
        }

        static List<Button> StripCardTiles(VisualElement panel)
        {
            var tiles = new List<Button>();
            var strip = panel.Q("collection-strip");
            if (strip == null)
            {
                return tiles;
            }

            foreach (var child in strip.Children())
            {
                if (child is Button button && button.ClassListContains("card-tile"))
                {
                    tiles.Add(button);
                }
            }

            return tiles;
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

        static void Click(Button button)
        {
            var field = button.clickable.GetType().GetField(
                "clicked",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var handler = field?.GetValue(button.clickable) as System.Delegate;
            Assert.IsNotNull(handler, "The preview control has no click handler.");
            handler.DynamicInvoke();
        }
    }
}
