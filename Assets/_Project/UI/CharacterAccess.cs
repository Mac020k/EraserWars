using EraserWars.Cards;
using EraserWars.Persistence;

namespace EraserWars.UI
{
    public static class CharacterAccess
    {
        public static bool DebugUnlockAll { get; private set; }

        public static void SetDebugUnlockAll(bool enabled)
        {
            DebugUnlockAll = enabled;
        }

        public static bool IsUnlocked(CharacterDefinition character)
        {
            if (character == null)
            {
                return false;
            }

            if (DebugUnlockAll)
            {
                return true;
            }

            return character.UnlockedByDefault || CharacterUnlocks.IsEarned(character.IdValue);
        }
    }
}
