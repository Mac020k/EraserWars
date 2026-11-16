using System;
using EraserWars.Core;

namespace EraserWars.UI
{
    public readonly struct TurnChrome
    {
        public const float UrgentSeconds = 5f;

        public TurnChrome(
            int seat,
            string headline,
            string passHint,
            bool showPassHint,
            string timerDigits,
            bool timerTicking,
            bool timerUrgent)
        {
            Seat = seat;
            Headline = headline;
            PassHint = passHint;
            ShowPassHint = showPassHint;
            TimerDigits = timerDigits;
            TimerTicking = timerTicking;
            TimerUrgent = timerUrgent;
        }

        public int Seat { get; }
        public string Headline { get; }
        public string PassHint { get; }
        public bool ShowPassHint { get; }
        public string TimerDigits { get; }
        public bool TimerTicking { get; }
        public bool TimerUrgent { get; }
        public bool ShowCallout => Seat >= 0;

        public static string SeatLine(int seat) => "P" + (seat + 1) + " のばん";

        public static TurnChrome From(MatchState state)
        {
            if (state.Phase == Phase.MatchEnd)
            {
                return new TurnChrome(-1, "おわり", string.Empty, false, "--", false, false);
            }

            var seat = state.CurrentPlayerId.Value;
            var label = "P" + (seat + 1);
            var deviceTurn = IsDeviceTurn(state.Phase);
            var ticking = deviceTurn && state.Rules.TurnTimeSeconds > 0f;
            var seconds = (int)Math.Ceiling(Math.Max(0f, state.TurnTimeLeft));
            return new TurnChrome(
                seat,
                SeatLine(seat),
                $"このきを {label} にわたして",
                deviceTurn,
                ticking ? seconds.ToString() : "--",
                ticking,
                ticking && state.TurnTimeLeft <= UrgentSeconds);
        }

        static bool IsDeviceTurn(Phase phase) =>
            phase is Phase.TurnStart or Phase.CardSelect or Phase.Aim;
    }
}
