using System.Globalization;
using UnityEngine;

namespace EraserWars.Persistence
{
    public static class CharacterUnlocks
    {
        public const string GrantedKey = "eraserwars.unlock.granted";
        public const string FlashedKey = "eraserwars.unlock.flashed";

        public static bool IsEarned(int characterId)
        {
            return Contains(GrantedKey, characterId);
        }

        public static bool HasPendingFlash(int characterId)
        {
            return IsEarned(characterId) && !Contains(FlashedKey, characterId);
        }

        public static void Grant(int characterId)
        {
            if (characterId < 0)
            {
                return;
            }

            Add(GrantedKey, characterId);
        }

        public static bool ConsumeFlash(int characterId)
        {
            if (!HasPendingFlash(characterId))
            {
                return false;
            }

            Add(FlashedKey, characterId);
            return true;
        }

        static bool Contains(string key, int characterId)
        {
            var raw = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return false;
            }

            var parts = raw.Split(',');
            for (var i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                    && id == characterId)
                {
                    return true;
                }
            }

            return false;
        }

        static void Add(string key, int characterId)
        {
            if (Contains(key, characterId))
            {
                return;
            }

            var raw = PlayerPrefs.GetString(key, string.Empty);
            var id = characterId.ToString(CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(key, string.IsNullOrEmpty(raw) ? id : raw + "," + id);
            PlayerPrefs.Save();
        }
    }
}
