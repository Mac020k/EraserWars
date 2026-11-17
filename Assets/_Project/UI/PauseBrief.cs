using System;
using EraserWars.Core;

namespace EraserWars.UI
{
    public readonly struct PauseBrief
    {
        public PauseBrief(string rules, string seats, string options)
        {
            Rules = rules;
            Seats = seats;
            Options = options;
        }

        public string Rules { get; }
        public string Seats { get; }
        public string Options { get; }

        public static PauseBrief From(MatchState state, GameSettings settings)
        {
            var mode = ModeName(state.Rules.WinMode);
            var rules = mode + "  ·  " + UiText.Format(UiKey.HudRoundMark, state.Round)
                + "\n" + UiText.Get(UiKey.MenuTip);
            var motion = settings.ReduceMotion
                ? UiText.Get(UiKey.OptReduceOn)
                : UiText.Get(UiKey.OptReduceOff);
            var options = UiText.Format(UiKey.OptTurn, settings.TurnSeconds) + "  ·  " + motion;
            return new PauseBrief(rules, SeatLine(state), options);
        }

        public static string SeatLine(MatchState state)
        {
            var parts = new string[state.Players.Length];
            for (var i = 0; i < state.Players.Length; i++)
            {
                parts[i] = FormatSeat(state.Rules.WinMode, state.Players[i]);
            }

            return string.Join("  ·  ", parts);
        }

        static string FormatSeat(WinMode mode, PlayerState player)
        {
            var seat = player.Id.Value + 1;
            switch (mode)
            {
                case WinMode.Stock:
                    return UiText.Format(UiKey.HudStockLine, seat, player.Stocks);
                case WinMode.Points:
                    return UiText.Format(UiKey.HudPointsLine, seat, player.Points);
                default:
                    throw Unhandled(mode);
            }
        }

        static string ModeName(WinMode mode)
        {
            switch (mode)
            {
                case WinMode.Stock:
                    return UiText.Get(UiKey.MenuStock);
                case WinMode.Points:
                    return UiText.Get(UiKey.MenuPoints);
                default:
                    throw Unhandled(mode);
            }
        }

        static ArgumentOutOfRangeException Unhandled(WinMode mode) => mode switch
        {
            WinMode.Stock => new ArgumentOutOfRangeException(nameof(mode), mode, null),
            WinMode.Points => new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };
    }
}
