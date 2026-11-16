using System.Collections.Generic;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Cards
{
    [CreateAssetMenu(menuName = "EraserWars/Card", fileName = "Card_")]
    public sealed class CardDefinition : ScriptableObject
    {
        public int IdValue;
        public string NameKey;
        public string DisplayNameJa;
        public string FamilyKey = CardFamilies.Kiai;
        public int BuildCost = 1;
        public bool IsSignature;
        public int MaxCopies = 1;
        public int UsesPerMatch = 1;
        public int CooldownTurns;
        public int DurationTurns = 1;
        public List<string> EffectKinds = new();

        public CardId Id => new(IdValue);

        public string DisplayName =>
            string.IsNullOrWhiteSpace(DisplayNameJa) ? NameKey : DisplayNameJa;
    }

    public static class CardFamilies
    {
        public const string Kiai = "kiai";
        public const string Homework = "homework";
        public const string Seat = "seat";
        public const string Stationery = "stationery";
        public const string Cover = "cover";
        public const string Signature = "signature";

        public static string DisplayJa(string key) => key switch
        {
            Kiai => "気合",
            Homework => "宿題",
            Seat => "席替え",
            Stationery => "文房具",
            Cover => "カバー",
            Signature => "必殺",
            _ => key ?? string.Empty,
        };
    }
}
