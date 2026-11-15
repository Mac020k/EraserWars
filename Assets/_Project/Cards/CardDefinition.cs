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
        public int BuildCost = 1;
        public bool IsSignature;
        public int MaxCopies = 1;
        public int UsesPerMatch = 1;
        public int CooldownTurns;
        public int DurationTurns = 1;
        public List<string> EffectKinds = new();

        public CardId Id => new(IdValue);
    }
}
