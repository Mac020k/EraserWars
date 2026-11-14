using System;
using System.Collections.Immutable;

namespace Keshipin.Core
{
    public static class MatchFactory
    {
        public static MatchState CreateHotseat(int playerCount, RuleSet rules)
        {
            if (playerCount is < 2 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(playerCount), "Hotseat supports 2 to 4 players.");
            }

            var players = ImmutableArray.CreateBuilder<PlayerState>(playerCount);
            var order = ImmutableArray.CreateBuilder<PlayerId>(playerCount);
            for (var i = 0; i < playerCount; i++)
            {
                var id = new PlayerId(i);
                order.Add(id);
                players.Add(new PlayerState(
                    id,
                    new CharacterId(i),
                    rules.StockCount,
                    Points: 0,
                    Hand: ImmutableArray<CardId>.Empty,
                    Deck: ImmutableArray<CardId>.Empty,
                    Used: ImmutableHashSet<CardId>.Empty,
                    Cooldowns: ImmutableDictionary<CardId, int>.Empty,
                    EliminatedRound: null,
                    Disconnected: false));
            }

            return new MatchState(
                rules,
                players.ToImmutable(),
                order.ToImmutable(),
                Round: 1,
                TurnIndex: 0,
                Phase.TurnStart,
                rules.TurnTimeSeconds,
                ImmutableArray<ActiveEffect>.Empty);
        }
    }
}
