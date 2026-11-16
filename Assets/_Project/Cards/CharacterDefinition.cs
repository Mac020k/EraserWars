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
        public Sprite Portrait;
        public EraserDefinition Eraser;
        public CardDefinition SignatureA;
        public CardDefinition SignatureB;
        public bool UnlockedByDefault;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(DisplayNameJa) ? NameKey : DisplayNameJa;
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
