using EraserWars.Core;

namespace EraserWars.Gameplay
{
    public readonly struct CpuSeatMask
    {
        public const int SeatLimit = 4;

        public int Bits { get; }

        public CpuSeatMask(int bits)
        {
            Bits = bits & ((1 << SeatLimit) - 1);
        }

        public bool Contains(int seat)
        {
            if ((uint)seat >= SeatLimit)
            {
                return false;
            }

            return (Bits & (1 << seat)) != 0;
        }
    }

    public enum CpuActKind
    {
        Wait,
        Pass,
        Flick,
    }

    public readonly struct CpuAct
    {
        public CpuAct(CpuActKind kind, FlickCommand command)
        {
            Kind = kind;
            Command = command;
        }

        public CpuActKind Kind { get; }
        public FlickCommand Command { get; }

        public static CpuAct Wait => new(CpuActKind.Wait, default);

        public static CpuAct Pass => new(CpuActKind.Pass, default);

        public static CpuAct Flick(FlickCommand command) => new(CpuActKind.Flick, command);
    }

    public static class CpuTurn
    {
        public const float ThinkSeconds = 0.4f;
        public const float WeakDrag01 = 0.18f;
        public const float WeakDirectionX = 0.15f;
        public const float WeakDirectionY = 1f;
        public const float WeakContactY = 0.85f;

        public static CpuAct Choose(CpuSeatMask mask, int seat, Phase phase, int round, float thinkElapsed)
        {
            if (!mask.Contains(seat) || thinkElapsed < ThinkSeconds)
            {
                return CpuAct.Wait;
            }

            if (phase is not (Phase.CardSelect or Phase.Aim))
            {
                return CpuAct.Wait;
            }

            if (FlickThisVisit(round, seat))
            {
                return CpuAct.Flick(WeakFlick(new PlayerId(seat)));
            }

            return CpuAct.Pass;
        }

        public static bool FlickThisVisit(int round, int seat) => ((round + seat) & 1) == 0;

        public static FlickCommand WeakFlick(PlayerId player)
        {
            return new FlickCommand(
                player,
                WeakDirectionX,
                WeakDirectionY,
                WeakDrag01,
                0f,
                WeakContactY,
                0.2f);
        }
    }
}
