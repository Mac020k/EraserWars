using UnityEngine;

namespace Keshipin.Cards
{
    [CreateAssetMenu(menuName = "Keshipin/Eraser", fileName = "Eraser_")]
    public sealed class EraserDefinition : ScriptableObject
    {
        public string NameKey;
        public float Mass = 0.02f;
        public float Friction = 0.4f;
        public float Bounciness = 0.05f;
        public Vector3 SizeMeters = new(0.043f, 0.011f, 0.017f);
        public Vector3 CenterOfMassOffset;
        public float Power = 1f;
        public float Speed = 1f;
        public int DeckCostCap = 8;
        public int HandLimit = 3;
        public string PassiveKey;
    }
}
