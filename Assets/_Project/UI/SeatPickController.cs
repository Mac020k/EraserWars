using System.Collections.Generic;
using EraserWars.Cards;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class SeatPickController
    {
        readonly ContentCatalog catalog;
        readonly HotseatLoadout loadout;
        VisualElement strip;
        VisualElement picker;
        Label caption;
        VisualElement row;
        int playerCount = 2;
        int editingSeat = -1;

        public SeatPickController(ContentCatalog catalog, HotseatLoadout loadout)
        {
            this.catalog = catalog;
            this.loadout = loadout;
        }

        public bool IsPickerOpen => editingSeat >= 0;

        public void Bind(VisualElement home)
        {
            strip = home?.Q("seat-strip");
            picker = home?.Q("seat-picker");
            caption = home?.Q<Label>("seat-picker-caption");
            row = home?.Q("seat-picker-row");
        }

        public void Refresh(int matchPlayerCount)
        {
            playerCount = matchPlayerCount >= 4 ? 4 : 2;
            if (editingSeat >= playerCount)
            {
                editingSeat = -1;
            }

            RebuildSeats();
            if (editingSeat >= 0)
            {
                ShowPicker();
            }
            else
            {
                HidePicker();
            }
        }

        public bool ClosePicker()
        {
            if (editingSeat < 0)
            {
                return false;
            }

            editingSeat = -1;
            RebuildSeats();
            HidePicker();
            return true;
        }

        public void ToggleSeat(int seat)
        {
            if (seat < 0 || seat >= playerCount)
            {
                return;
            }

            if (editingSeat == seat)
            {
                ClosePicker();
                return;
            }

            editingSeat = seat;
            RebuildSeats();
            ShowPicker();
        }

        public bool Pick(CharacterDefinition character)
        {
            if (editingSeat < 0)
            {
                return false;
            }

            if (!loadout.TryAssignUnlocked(editingSeat, character))
            {
                return false;
            }

            editingSeat = -1;
            RebuildSeats();
            HidePicker();
            return true;
        }

        void RebuildSeats()
        {
            if (strip == null)
            {
                return;
            }

            strip.Clear();
            for (var i = 0; i < playerCount; i++)
            {
                strip.Add(BuildSeatChip(i));
            }
        }

        Button BuildSeatChip(int seat)
        {
            var character = loadout.Seats[seat];
            var chip = new Button { name = $"seat-chip-{seat}" };
            chip.AddToClassList("seat-chip");
            chip.EnableInClassList("selected", seat == editingSeat);
            var tag = new Label($"P{seat + 1}") { pickingMode = PickingMode.Ignore };
            tag.AddToClassList("seat-chip-tag");
            var art = new VisualElement { pickingMode = PickingMode.Ignore };
            art.AddToClassList("char-tile-art");
            if (character?.Portrait != null)
            {
                UiImageUtil.ApplyFaceCrop(art, character.Portrait);
            }

            var name = new Label(character != null ? character.DisplayName : "未設定")
            {
                pickingMode = PickingMode.Ignore,
            };
            name.AddToClassList("seat-chip-name");
            chip.Add(tag);
            chip.Add(art);
            chip.Add(name);
            var captured = seat;
            chip.clicked += () => ToggleSeat(captured);
            return chip;
        }

        void ShowPicker()
        {
            if (picker != null)
            {
                picker.EnableInClassList("hidden", false);
            }

            if (caption != null)
            {
                caption.text = $"P{editingSeat + 1} のキャラ";
            }

            RebuildOptions();
        }

        void HidePicker()
        {
            picker?.EnableInClassList("hidden", true);
            row?.Clear();
        }

        void RebuildOptions()
        {
            if (row == null)
            {
                return;
            }

            row.Clear();
            var unlocked = UnlockedCharacters();
            if (unlocked.Count == 0)
            {
                var empty = new Label("解放済みのキャラがいません。");
                empty.AddToClassList("collection-empty");
                row.Add(empty);
                return;
            }

            var current = editingSeat >= 0 ? loadout.Seats[editingSeat] : null;
            foreach (var character in unlocked)
            {
                var tile = BuildFaceTile(character, character == current);
                var captured = character;
                tile.clicked += () => Pick(captured);
                row.Add(tile);
            }
        }

        List<CharacterDefinition> UnlockedCharacters()
        {
            var list = new List<CharacterDefinition>();
            if (catalog?.Characters == null)
            {
                return list;
            }

            foreach (var character in catalog.Characters)
            {
                if (character != null && character.UnlockedByDefault)
                {
                    list.Add(character);
                }
            }

            return list;
        }

        static Button BuildFaceTile(CharacterDefinition character, bool selected)
        {
            var tile = new Button { name = $"seat-option-{character.IdValue}" };
            tile.AddToClassList("char-tile");
            tile.EnableInClassList("selected", selected);
            var art = new VisualElement();
            art.AddToClassList("char-tile-art");
            if (character.Portrait != null)
            {
                UiImageUtil.ApplyFaceCrop(art, character.Portrait);
            }

            var name = new Label(character.DisplayName);
            name.AddToClassList("char-tile-name");
            var archetype = new Label(CharacterArchetypes.DisplayJa(character.ArchetypeKey));
            archetype.AddToClassList("char-tile-arch");
            tile.Add(art);
            tile.Add(name);
            tile.Add(archetype);
            return tile;
        }
    }
}
