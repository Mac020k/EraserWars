using System.Collections.Generic;
using System.Collections.Immutable;
using Keshipin.Core;

namespace Keshipin.Cards
{
    public static class CardResolver
    {
        public static MatchState Apply(
            MatchState state,
            PlayerId owner,
            CardId card,
            IReadOnlyList<EffectSpec> effects,
            PlayerId? target)
        {
            var next = state;
            foreach (var effect in effects)
            {
                next = ApplyOne(next, owner, card, effect, target);
            }

            return next;
        }

        static MatchState ApplyOne(
            MatchState state,
            PlayerId owner,
            CardId card,
            EffectSpec effect,
            PlayerId? target)
        {
            var turns = effect switch
            {
                ModifyStat s => s.Turns,
                ModifyRule r => r.Turns,
                PlaceDeskObject o => o.Turns,
                _ => 1,
            };

            var kind = EffectSpecExhaustive.Describe(effect);
            var active = new ActiveEffect(card, owner, target, kind, turns);
            return state with { Effects = state.Effects.Add(active) };
        }

        public static MatchState TickEffects(MatchState state)
        {
            var builder = state.Effects.ToBuilder();
            for (var i = builder.Count - 1; i >= 0; i--)
            {
                var e = builder[i];
                var left = e.TurnsRemaining - 1;
                if (left <= 0)
                {
                    builder.RemoveAt(i);
                }
                else
                {
                    builder[i] = e with { TurnsRemaining = left };
                }
            }

            return state with { Effects = builder.ToImmutable() };
        }
    }
}
