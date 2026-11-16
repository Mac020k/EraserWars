using EraserWars.Core;

namespace EraserWars.Gameplay
{
    public readonly struct TurnAnnounce
    {
        public const float ShowSeconds = 1f;

        public int Seat { get; }
        public float RemainingSeconds { get; }

        public bool IsShowing => RemainingSeconds > 0f;

        public bool IsOnScreen(bool covered) => IsShowing && !covered;

        public static TurnAnnounce Begin(int seat) => new(seat, ShowSeconds);

        public static TurnAnnounce? Next(
            TurnAnnounce? current,
            int previousSeat,
            int seat,
            Phase phase,
            bool opening)
        {
            if (phase == Phase.MatchEnd)
            {
                return null;
            }

            if (!opening && previousSeat == seat)
            {
                return current;
            }

            return Begin(seat);
        }

        public TurnAnnounce Advance(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return this;
            }

            var remaining = RemainingSeconds - deltaSeconds;
            if (remaining < 0f)
            {
                remaining = 0f;
            }

            return new TurnAnnounce(Seat, remaining);
        }

        TurnAnnounce(int seat, float remainingSeconds)
        {
            Seat = seat;
            RemainingSeconds = remainingSeconds;
        }
    }
}
