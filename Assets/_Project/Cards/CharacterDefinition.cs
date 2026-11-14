using UnityEngine;

namespace Keshipin.Cards
{
    [CreateAssetMenu(menuName = "Keshipin/Character", fileName = "Character_")]
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
