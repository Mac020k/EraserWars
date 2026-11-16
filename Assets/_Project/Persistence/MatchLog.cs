using System;
using EraserWars.Core;

namespace EraserWars.Persistence
{
    public sealed record MatchSeatLog(int Seat, int Stocks, int Points, int? EliminatedRound);

    public sealed record MatchLog(
        string Id,
        DateTime StartedUtc,
        DateTime EndedUtc,
        WinMode WinMode,
        int StockCount,
        int PointRounds,
        int Round,
        int? WinnerSeat,
        MatchSeatLog[] Seats);
}
