using Keshipin.Core;
using UnityEngine;

namespace Keshipin.Cards
{
    [CreateAssetMenu(menuName = "Keshipin/RuleSet", fileName = "RuleSet_")]
    public sealed class RuleSetAsset : ScriptableObject
    {
        public WinMode WinMode = WinMode.Stock;
        public int StockCount = 3;
        public int PointRounds = 8;
        public float TurnTimeSeconds = 20f;
        public bool AimAssist = true;
        public bool ChaosSimultaneous;

        public RuleSet ToRuleSet() => new(
            WinMode,
            StockCount,
            PointRounds,
            TurnTimeSeconds,
            AimAssist,
            ChaosSimultaneous);
    }
}
