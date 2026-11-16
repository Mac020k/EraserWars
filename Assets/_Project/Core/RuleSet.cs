using System;

namespace EraserWars.Core
{
    public enum WinMode
    {
        Stock,
        Points,
    }

    public sealed record RuleSet(
        WinMode WinMode,
        int StockCount,
        int PointRounds,
        float TurnTimeSeconds,
        bool AimAssist,
        bool ChaosSimultaneous)
    {
        public static RuleSet CasualStock => new(
            WinMode.Stock,
            StockCount: 3,
            PointRounds: 8,
            TurnTimeSeconds: 20f,
            AimAssist: true,
            ChaosSimultaneous: false);

        public static RuleSet RankedStock => CasualStock with { AimAssist = false };

        public static RuleSet PointsTrial => new(
            WinMode.Points,
            StockCount: 3,
            PointRounds: 8,
            TurnTimeSeconds: 20f,
            AimAssist: true,
            ChaosSimultaneous: false);

        public static WinMode WinModeFromPref(int raw) =>
            raw == (int)WinMode.Points ? WinMode.Points : WinMode.Stock;

        public static RuleSet ForHotseat(WinMode mode) => mode switch
        {
            WinMode.Stock => CasualStock,
            WinMode.Points => PointsTrial,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unhandled win mode."),
        };
    }
}
