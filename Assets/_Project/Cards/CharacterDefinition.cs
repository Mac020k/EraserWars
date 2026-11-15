using UnityEngine;

namespace EraserWars.Cards
{
    [CreateAssetMenu(menuName = "EraserWars/Character", fileName = "Character_")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public int IdValue;
        public string NameKey;
        public Sprite Portrait;
        public EraserDefinition Eraser;
        public CardDefinition SignatureA;
        public CardDefinition SignatureB;
        public bool UnlockedByDefault;
    }
}
