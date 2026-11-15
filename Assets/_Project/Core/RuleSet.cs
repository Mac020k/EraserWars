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
    }
}
