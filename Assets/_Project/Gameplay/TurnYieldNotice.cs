using System;

namespace EraserWars.Gameplay
{
    public enum TurnYieldKind
    {
        Pass,
        Time,
    }

    public readonly struct TurnYieldNotice
    {
        public const float Seconds = 0.7f;

        public TurnYieldNotice(TurnYieldKind kind, int seat)
        {
            Kind = kind;
            Seat = seat;
        }

        public TurnYieldKind Kind { get; }
        public int Seat { get; }

        public string Label => Text(Kind);

        static string Text(TurnYieldKind kind)
        {
            switch (kind)
            {
                case TurnYieldKind.Pass:
                    return "パスした";
                case TurnYieldKind.Time:
                    return "じかんぎれ";
                default:
                    return Never(kind);
            }
        }

        static string Never(TurnYieldKind kind) => kind switch
        {
            TurnYieldKind.Pass => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            TurnYieldKind.Time => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }
}
