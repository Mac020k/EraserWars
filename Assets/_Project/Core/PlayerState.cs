using System.Collections.Immutable;

namespace EraserWars.Core
{
    public sealed record PlayerState(
        PlayerId Id,
        CharacterId Character,
        int Stocks,
        int Points,
        ImmutableArray<CardId> Hand,
        ImmutableArray<CardId> Deck,
        ImmutableHashSet<CardId> Used,
        ImmutableDictionary<CardId, int> Cooldowns,
        int? EliminatedRound,
        bool Disconnected)
    {
        public bool IsAlive => Stocks > 0 && EliminatedRound is null;
    }
}
