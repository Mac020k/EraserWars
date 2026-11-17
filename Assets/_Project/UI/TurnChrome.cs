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
            bool timerUrgent,
            float timerFill)
        {
            Seat = seat;
            Headline = headline;
            PassHint = passHint;
            ShowPassHint = showPassHint;
            TimerDigits = timerDigits;
            TimerTicking = timerTicking;
            TimerUrgent = timerUrgent;
            TimerFill = timerFill;
        }

        public int Seat { get; }
        public string Headline { get; }
        public string PassHint { get; }
        public bool ShowPassHint { get; }
        public string TimerDigits { get; }
        public bool TimerTicking { get; }
        public bool TimerUrgent { get; }
        public float TimerFill { get; }
        public bool ShowCallout => Seat >= 0;

        public static float SweepDegrees(float fill)
        {
            if (fill <= 0f)
            {
                return 0f;
            }

            if (fill >= 1f)
            {
                return 360f;
            }

            return fill * 360f;
        }

        public static string SeatLine(int seat) => UiText.Format(UiKey.HudSeatTurn, seat + 1);

        public static TurnChrome From(MatchState state)
        {
            if (state.Phase == Phase.MatchEnd)
            {
                return new TurnChrome(-1, UiText.Get(UiKey.HudTurnOver), string.Empty, false, "--", false, false, 0f);
            }

            var seat = state.CurrentPlayerId.Value;
            var deviceTurn = IsDeviceTurn(state.Phase);
            var budget = state.Rules.TurnTimeSeconds;
            var ticking = deviceTurn && budget > 0f;
            var seconds = (int)Math.Ceiling(Math.Max(0f, state.TurnTimeLeft));
            var fill = 0f;
            if (ticking)
            {
                fill = Math.Min(1f, Math.Max(0f, state.TurnTimeLeft / budget));
            }

            return new TurnChrome(
                seat,
                SeatLine(seat),
                UiText.Format(UiKey.HudPassHint, seat + 1),
                deviceTurn,
                ticking ? seconds.ToString() : "--",
                ticking,
                ticking && state.TurnTimeLeft <= UrgentSeconds,
                fill);
        }

        static bool IsDeviceTurn(Phase phase) =>
            phase is Phase.TurnStart or Phase.CardSelect or Phase.Aim;
    }
}
