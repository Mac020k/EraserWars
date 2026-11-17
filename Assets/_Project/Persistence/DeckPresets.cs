using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EraserWars.Persistence
{
    public static class DeckPresets
    {
        public const int SlotsPerCharacter = 5;
        public const string Key = "ew.deck.presets.v1";
        const int MaxStoredCards = 32;

        public static int GetActiveSlot(int characterId)
        {
            var row = Find(Read(), characterId);
            return row == null ? 0 : ClampSlot(row.ActiveSlot);
        }

        public static void SetActiveSlot(int characterId, int slot)
        {
            if (characterId < 0)
            {
                return;
            }

            var file = Read();
            var row = EnsureRow(file, characterId);
            row.ActiveSlot = ClampSlot(slot);
            Write(file);
        }

        public static bool HasSaved(int characterId, int slot)
        {
            var stored = StoredSlot(characterId, slot);
            return stored != null && stored.Saved;
        }

        public static IReadOnlyList<int> LoadCards(int characterId, int slot)
        {
            var stored = StoredSlot(characterId, slot);
            if (stored == null || !stored.Saved || stored.CardIds == null)
            {
                return Array.Empty<int>();
            }

            return stored.CardIds;
        }

        public static void SaveCards(int characterId, int slot, IReadOnlyList<int> cardIds)
        {
            if (characterId < 0)
            {
                return;
            }

            var file = Read();
            var row = EnsureRow(file, characterId);
            var stored = Slot(row, slot, true);
            stored.Saved = true;
            stored.CardIds = CopyIds(cardIds);
            Write(file);
        }

        public static string GetName(int characterId, int slot)
        {
            var stored = StoredSlot(characterId, slot);
            if (stored == null || string.IsNullOrWhiteSpace(stored.Name))
            {
                return DefaultName(slot);
            }

            return stored.Name.Trim();
        }

        public static void SetName(int characterId, int slot, string name)
        {
            if (characterId < 0)
            {
                return;
            }

            var file = Read();
            var row = EnsureRow(file, characterId);
            var stored = Slot(row, slot, true);
            stored.Name = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
            Write(file);
        }

        public static string DefaultName(int slot)
        {
            return "プリセット" + (ClampSlot(slot) + 1).ToString(CultureInfo.InvariantCulture);
        }

        static DeckPresetSlot StoredSlot(int characterId, int slot)
        {
            var row = Find(Read(), characterId);
            if (row?.Slots == null)
            {
                return null;
            }

            var index = ClampSlot(slot);
            if (index < 0 || index >= row.Slots.Length)
            {
                return null;
            }

            return row.Slots[index];
        }

        static DeckPresetFile Read()
        {
            var raw = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return new DeckPresetFile();
            }

            try
            {
                var file = JsonUtility.FromJson<DeckPresetFile>(raw);
                if (file == null)
                {
                    return new DeckPresetFile();
                }

                if (file.Rows == null)
                {
                    file.Rows = Array.Empty<DeckPresetRow>();
                }

                return file;
            }
            catch (ArgumentException)
            {
                return new DeckPresetFile();
            }
        }

        static void Write(DeckPresetFile file)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(file));
            PlayerPrefs.Save();
        }

        static DeckPresetRow Find(DeckPresetFile file, int characterId)
        {
            if (file?.Rows == null || characterId < 0)
            {
                return null;
            }

            for (var i = 0; i < file.Rows.Length; i++)
            {
                var row = file.Rows[i];
                if (row != null && row.CharacterId == characterId)
                {
                    return row;
                }
            }

            return null;
        }

        static DeckPresetRow EnsureRow(DeckPresetFile file, int characterId)
        {
            var existing = Find(file, characterId);
            if (existing != null)
            {
                return existing;
            }

            var row = new DeckPresetRow { CharacterId = characterId };
            var rows = new List<DeckPresetRow>();
            if (file.Rows != null)
            {
                for (var i = 0; i < file.Rows.Length; i++)
                {
                    if (file.Rows[i] != null)
                    {
                        rows.Add(file.Rows[i]);
                    }
                }
            }

            rows.Add(row);
            file.Rows = rows.ToArray();
            return row;
        }

        static DeckPresetSlot Slot(DeckPresetRow row, int slot, bool create)
        {
            var index = ClampSlot(slot);
            if (row.Slots == null || row.Slots.Length != SlotsPerCharacter)
            {
                var next = new DeckPresetSlot[SlotsPerCharacter];
                if (row.Slots != null)
                {
                    var count = row.Slots.Length < next.Length ? row.Slots.Length : next.Length;
                    for (var i = 0; i < count; i++)
                    {
                        next[i] = row.Slots[i];
                    }
                }

                row.Slots = next;
            }

            if (row.Slots[index] == null && create)
            {
                row.Slots[index] = new DeckPresetSlot();
            }

            return row.Slots[index];
        }

        static int[] CopyIds(IReadOnlyList<int> cardIds)
        {
            if (cardIds == null || cardIds.Count == 0)
            {
                return Array.Empty<int>();
            }

            var count = cardIds.Count > MaxStoredCards ? MaxStoredCards : cardIds.Count;
            var ids = new int[count];
            for (var i = 0; i < count; i++)
            {
                ids[i] = cardIds[i];
            }

            return ids;
        }

        static int ClampSlot(int slot)
        {
            if (slot < 0)
            {
                return 0;
            }

            if (slot >= SlotsPerCharacter)
            {
                return SlotsPerCharacter - 1;
            }

            return slot;
        }

        [Serializable]
        sealed class DeckPresetFile
        {
            public DeckPresetRow[] Rows = Array.Empty<DeckPresetRow>();
        }

        [Serializable]
        sealed class DeckPresetRow
        {
            public int CharacterId;
            public int ActiveSlot;
            public DeckPresetSlot[] Slots;
        }

        [Serializable]
        sealed class DeckPresetSlot
        {
            public string Name = "";
            public bool Saved;
            public int[] CardIds = Array.Empty<int>();
        }
    }
}
