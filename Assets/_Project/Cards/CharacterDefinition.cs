using UnityEngine;

namespace EraserWars.Cards
{
    [CreateAssetMenu(menuName = "EraserWars/Character", fileName = "Character_")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public int IdValue;
        public string NameKey;
        public string DisplayNameJa;
        public string ArchetypeKey = CharacterArchetypes.AllOut;
        public string UnlockHintJa;
        [TextArea(3, 8)] public string BioJa;
        public Sprite Portrait;
        [Tooltip("Square head-and-shoulders crop baked by tools/bake_faces.py for card faces.")]
        public Sprite FacePortrait;
        public EraserDefinition Eraser;
        public CardDefinition SignatureA;
        public CardDefinition SignatureB;
        public bool UnlockedByDefault;

        public Sprite FaceSprite => FacePortrait != null ? FacePortrait : Portrait;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(DisplayNameJa) ? NameKey : DisplayNameJa;

        public string MaskedDisplayName => CharacterDisplay.MaskName(DisplayName);

        public string UnlockHint =>
            string.IsNullOrWhiteSpace(UnlockHintJa) ? "条件未設定" : UnlockHintJa;

        public string Bio =>
            string.IsNullOrWhiteSpace(BioJa) ? "自己紹介はこれから。" : BioJa;
    }

    public static class CharacterDisplay
    {
        public static string MaskName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "？？？";
            }

            var chars = name.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == ' ' || chars[i] == '　')
                {
                    continue;
                }

                chars[i] = '？';
            }

            return new string(chars);
        }
    }

    public static class CharacterArchetypes
    {
        public const string AllOut = "all_out";
        public const string SeatTake = "seat_take";
        public const string Teacher = "teacher";
        public const string Afterschool = "afterschool";
        public const string DeskKing = "desk_king";

        public static string DisplayJa(string key) => key switch
        {
            AllOut => "全力アタック型",
            SeatTake => "席取り上手型",
            Teacher => "先生役型",
            Afterschool => "放課後実験型",
            DeskKing => "机の王様型",
            _ => key ?? string.Empty,
        };
    }
}
