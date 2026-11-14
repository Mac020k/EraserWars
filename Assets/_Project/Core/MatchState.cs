using System.Collections.Immutable;
using System.Linq;

namespace Keshipin.Core
{
    public sealed record MatchState(
        RuleSet Rules,
        ImmutableArray<PlayerState> Players,
        ImmutableArray<PlayerId> TurnOrder,
        int Round,
        int TurnIndex,
        Phase Phase,
        float TurnTimeLeft,
        ImmutableArray<ActiveEffect> Effects)
    {
        public PlayerId CurrentPlayerId => TurnOrder[TurnIndex];

        public PlayerState CurrentPlayer => Players.First(p => p.Id.Equals(CurrentPlayerId));

        public int AliveCount => Players.Count(p => p.IsAlive);

        public PlayerState GetPlayer(PlayerId id) => Players.First(p => p.Id.Equals(id));

        public MatchState WithPlayer(PlayerState updated)
        {
            var builder = Players.ToBuilder();
            for (var i = 0; i < builder.Count; i++)
            {
                if (builder[i].Id.Equals(updated.Id))
                {
                    builder[i] = updated;
                    break;
                }
            }

            return this with { Players = builder.ToImmutable() };
        }
    }
}
