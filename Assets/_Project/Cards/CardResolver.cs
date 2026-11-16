using System.Collections.Generic;
using System.Collections.Immutable;
using EraserWars.Core;

namespace EraserWars.Cards
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
                next = effect is PlaceDeskObject { PrefabKey: "ClearDust" }
                    ? ClearDust(next)
                    : ApplyOne(next, owner, card, effect, target);
            }

            return next;
        }

        static MatchState ClearDust(MatchState state) =>
            state with { Effects = state.Effects.RemoveAll(e => e.SpecKind.StartsWith("object:Dust")) };

        static MatchState ApplyOne(
            MatchState state,
            PlayerId owner,
            CardId card,
            EffectSpec effect,
            PlayerId? target)
        {
            switch (effect)
            {
                case RestoreStock restore:
                    return MatchEffectRules.RestoreStock(state, owner, restore.Amount);
                case ModifyRule { RuleKey: MatchEffectRules.SelfTurnTimeKey } self:
                    return state with { TurnTimeLeft = state.TurnTimeLeft * self.Value };
            }

            // A shield must outlive the owner's own settle tick to cover every opponent turn.
            var turns = effect switch
            {
                ModifyStat s => s.Turns,
                ModifyRule r => r.Turns,
                PlaceDeskObject o => o.Turns,
                GrantShield => state.Players.Length,
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
