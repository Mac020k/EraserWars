using System.Collections.Generic;
using System.Reflection;
using EraserWars.Cards;
using EraserWars.Gameplay;
using EraserWars.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class SeatPickTests
    {
        readonly List<Object> owned = new();
        GameObject uiHost;
        GameObject directorHost;

        [SetUp]
        public void ClearDebugUnlock()
        {
            CharacterAccess.SetDebugUnlockAll(false);
        }

        [TearDown]
        public void TearDown()
        {
            if (uiHost != null)
            {
                Object.DestroyImmediate(uiHost);
            }

            if (directorHost != null)
            {
                Object.DestroyImmediate(directorHost);
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
        }

        [Test]
        public void SkippedPickKeepsTheDefaultRosterOnEachSeat()
        {
            var catalog = Catalog();
            var loadout = new HotseatLoadout();
            loadout.EnsureDefaults(catalog);
            var director = Director();

            director.SetSeatRoster(loadout.Seats);

            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[0]);
            Assert.AreSame(catalog.FindCharacter(1), loadout.Seats[1]);
            Assert.AreSame(catalog.FindCharacter(2), director.RosterAt(2));
            Assert.AreSame(catalog.FindCharacter(3), director.RosterAt(3));
            Assert.AreSame(loadout.Seats[0], director.RosterAt(0));
            Assert.IsNull(director.RosterAt(4));
        }

        [Test]
        public void AssigningAnUnlockedCharacterReplacesOnlyThatSeat()
        {
            var catalog = Catalog();
            var loadout = new HotseatLoadout();
            loadout.EnsureDefaults(catalog);
            var director = Director();
            var madoka = catalog.FindCharacter(3);
            var locked = catalog.FindCharacter(4);

            Assert.IsTrue(loadout.TryAssignUnlocked(0, madoka));
            Assert.IsFalse(loadout.TryAssignUnlocked(1, locked));
            Assert.IsFalse(loadout.TryAssignUnlocked(4, madoka));
            Assert.IsFalse(loadout.TryAssignUnlocked(0, null));
            director.SetSeatRoster(loadout.Seats);

            Assert.AreSame(madoka, director.RosterAt(0));
            Assert.AreSame(catalog.FindCharacter(1), director.RosterAt(1));
            Assert.AreSame(locked, catalog.FindCharacter(4));
            Assert.IsFalse(locked.UnlockedByDefault);
        }

        [Test]
        public void SeatStripShowsDefaultsAndPickerOffersOnlyUnlockedFaces()
        {
            var catalog = Catalog();
            var loadout = new HotseatLoadout();
            loadout.EnsureDefaults(catalog);
            var home = MountedHome();
            var pick = new SeatPickController(catalog, loadout);
            pick.Bind(home);
            pick.Refresh(2);

            Assert.AreEqual("赤星", home.Q<Button>("seat-chip-0").Q<Label>(className: "seat-chip-name").text);
            Assert.AreEqual("轟", home.Q<Button>("seat-chip-1").Q<Label>(className: "seat-chip-name").text);
            Assert.IsNull(home.Q("seat-chip-2"));
            Assert.IsTrue(home.Q("seat-picker").ClassListContains("hidden"));

            Click(home.Q<Button>("seat-chip-0"));

            Assert.IsTrue(pick.IsPickerOpen);
            Assert.AreEqual("P1 のキャラ", home.Q<Label>("seat-picker-caption").text);
            Assert.IsFalse(home.Q("seat-picker").ClassListContains("hidden"));
            Assert.IsNotNull(home.Q<Button>("seat-option-0"));
            Assert.IsNotNull(home.Q<Button>("seat-option-3"));
            Assert.IsNull(home.Q("seat-option-4"));
            var art = home.Q<Button>("seat-option-0").Q(className: "char-tile-art");
            Assert.IsTrue(art.ClassListContains("char-tile-art"));
            Assert.AreEqual(catalog.FindCharacter(0).FaceSprite, art.style.backgroundImage.value.sprite);
            Assert.AreEqual(catalog.FindCharacter(0).FacePortrait, art.style.backgroundImage.value.sprite);
            Assert.AreEqual(BackgroundSizeType.Cover, art.style.backgroundSize.value.sizeType);
            Assert.AreEqual(BackgroundPositionKeyword.Top, art.style.backgroundPositionY.value.keyword);
            var tile = home.Q<Button>("seat-option-0");
            Assert.Less(tile.IndexOf(art), tile.IndexOf(tile.Q(className: "char-tile-place")));
            Assert.Less(tile.IndexOf(tile.Q(className: "char-tile-place")), tile.IndexOf(tile.Q(className: "char-tile-name")));
            Assert.AreEqual("3年2組", tile.Q<Label>(className: "char-tile-place").text);
            Assert.IsNull(home.Q<Button>("seat-option-0").Q(className: "char-tile-arch"));
            Assert.AreSame(home.Q<Button>("seat-option-0"), home.Q<Button>("seat-option-0").Q(className: "char-tile-badge").parent);

            Click(home.Q<Button>("seat-option-3"));

            Assert.IsFalse(pick.IsPickerOpen);
            Assert.AreSame(catalog.FindCharacter(3), loadout.Seats[0]);
            Assert.AreSame(catalog.FindCharacter(1), loadout.Seats[1]);
            Assert.AreEqual("花咲", home.Q<Button>("seat-chip-0").Q<Label>(className: "seat-chip-name").text);
            Assert.IsTrue(home.Q("seat-picker").ClassListContains("hidden"));

            Click(home.Q<Button>("seat-chip-1"));
            Click(home.Q<Button>("seat-option-0"));
            Assert.AreSame(catalog.FindCharacter(3), loadout.Seats[0]);
            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[1]);

            Click(home.Q<Button>("seat-chip-0"));
            Click(home.Q<Button>("seat-option-0"));
            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[0]);
            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[1]);
        }

        [Test]
        public void FourPlayerModeShowsFourSeatsAndEscapeClosesThePicker()
        {
            var catalog = Catalog();
            var loadout = new HotseatLoadout();
            loadout.EnsureDefaults(catalog);
            var home = MountedHome();
            var pick = new SeatPickController(catalog, loadout);
            pick.Bind(home);
            pick.Refresh(4);
            Click(home.Q<Button>("seat-chip-3"));
            Assert.IsNotNull(home.Q("seat-chip-2"));
            Assert.IsNotNull(home.Q("seat-chip-3"));
            Assert.IsTrue(pick.IsPickerOpen);

            pick.Refresh(2);

            Assert.IsNull(home.Q("seat-chip-2"));
            Assert.IsFalse(pick.IsPickerOpen);

            Click(home.Q<Button>("seat-chip-0"));
            Assert.IsTrue(pick.ClosePicker());
            Assert.IsFalse(pick.IsPickerOpen);
            Assert.IsFalse(pick.ClosePicker());
            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[0]);
        }

        [Test]
        public void LockedPickDoesNotChangeTheOpenSeat()
        {
            var catalog = Catalog();
            var loadout = new HotseatLoadout();
            loadout.EnsureDefaults(catalog);
            var home = MountedHome();
            var pick = new SeatPickController(catalog, loadout);
            pick.Bind(home);
            pick.Refresh(2);
            pick.ToggleSeat(0);

            Assert.IsFalse(pick.Pick(catalog.FindCharacter(4)));
            Assert.IsTrue(pick.IsPickerOpen);
            Assert.AreSame(catalog.FindCharacter(0), loadout.Seats[0]);
            Assert.AreEqual("赤星", home.Q<Button>("seat-chip-0").Q<Label>(className: "seat-chip-name").text);
        }

        ContentCatalog Catalog()
        {
            var catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            owned.Add(catalog);
            var portrait = Portrait();
            catalog.Characters = new List<CharacterDefinition>
            {
                Character("赤星", 0, true, portrait),
                Character("轟", 1, true, portrait),
                Character("水無月", 2, true, null),
                Character("花咲", 3, true, null),
                Character("Alex", 4, false, null),
            };
            return catalog;
        }

        Sprite Portrait()
        {
            var texture = new Texture2D(4, 8);
            owned.Add(texture);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 8), new Vector2(0.5f, 0.5f));
            owned.Add(sprite);
            return sprite;
        }

        CharacterDefinition Character(string name, int id, bool unlocked, Sprite portrait)
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            owned.Add(character);
            character.IdValue = id;
            character.DisplayNameJa = name;
            character.UnlockedByDefault = unlocked;
            character.ArchetypeKey = CharacterArchetypes.AllOut;
            character.Portrait = portrait;
            character.FacePortrait = portrait != null ? Portrait() : null;
            return character;
        }

        MatchDirector Director()
        {
            directorHost = new GameObject("seat-roster-director");
            return directorHost.AddComponent<MatchDirector>();
        }

        VisualElement MountedHome()
        {
            uiHost = new GameObject("seat-pick-ui");
            var document = uiHost.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            owned.Add(settings);
            document.panelSettings = settings;
            var home = new VisualElement { name = "home-stack" };
            var strip = new VisualElement { name = "seat-strip" };
            var picker = new VisualElement { name = "seat-picker" };
            picker.AddToClassList("hidden");
            var caption = new Label { name = "seat-picker-caption" };
            var row = new VisualElement { name = "seat-picker-row" };
            picker.Add(caption);
            picker.Add(row);
            home.Add(strip);
            home.Add(picker);
            document.rootVisualElement.Add(home);
            Assert.IsNotNull(home.panel, "Seat buttons need a panel so clicks reach them.");
            return home;
        }

        static void Click(Button button)
        {
            var clickable = button.clickable;
            var field = clickable.GetType().GetField(
                "clicked",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var handler = field?.GetValue(clickable) as System.Delegate;
            Assert.IsNotNull(handler, "The seat control has no click handler.");
            handler.DynamicInvoke();
        }
    }
}
