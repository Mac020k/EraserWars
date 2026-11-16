using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EraserWars.Core
{
    public static class MatchFactory
    {
        public static MatchState CreateHotseat(int playerCount, RuleSet rules) =>
            CreateHotseat(playerCount, rules, null, null);

        public static MatchState CreateHotseat(
            int playerCount,
            RuleSet rules,
            IReadOnlyList<ImmutableArray<CardId>> hands,
            IReadOnlyList<ImmutableArray<CardId>> decks)
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
                var hand = hands != null && i < hands.Count ? hands[i] : ImmutableArray<CardId>.Empty;
                var deck = decks != null && i < decks.Count ? decks[i] : ImmutableArray<CardId>.Empty;
                players.Add(new PlayerState(
                    id,
                    new CharacterId(i),
                    rules.StockCount,
                    Points: 0,
                    Hand: hand,
                    Deck: deck,
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
