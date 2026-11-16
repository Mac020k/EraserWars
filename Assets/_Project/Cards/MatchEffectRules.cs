using System;
using System.Globalization;
using EraserWars.Core;

namespace EraserWars.Cards
{
    public static class MatchEffectRules
    {
        public const string OpponentTurnTimeKey = "OpponentTurnTime";
        public const string SelfTurnTimeKey = "SelfTurnTime";

        const string ShieldPrefix = "shield:";

        public static MatchState OnTurnHandover(MatchState state)
        {
            if (state == null || state.Phase == Phase.MatchEnd || state.Effects.IsDefaultOrEmpty)
            {
                return state;
            }

            var current = state.CurrentPlayerId;
            var timeLeft = state.TurnTimeLeft;
            var builder = state.Effects.ToBuilder();
            var consumed = false;
            for (var i = builder.Count - 1; i >= 0; i--)
            {
                var effect = builder[i];
                if (effect.Owner.Equals(current)
                    || !TryReadRule(effect.SpecKind, OpponentTurnTimeKey, out var scale))
                {
                    continue;
                }

                timeLeft *= scale;
                builder.RemoveAt(i);
                consumed = true;
            }

            return consumed
                ? state with { TurnTimeLeft = timeLeft, Effects = builder.ToImmutable() }
                : state;
        }

        public static bool TryConsumeShield(MatchState state, PlayerId victim, out MatchState next)
        {
            next = state;
            if (state == null)
            {
                return false;
            }

            var builder = state.Effects.ToBuilder();
            for (var i = 0; i < builder.Count; i++)
            {
                var effect = builder[i];
                if (!effect.Owner.Equals(victim) || !TryReadShieldHits(effect.SpecKind, out var absorb, out var hits))
                {
                    continue;
                }

                if (hits > 1)
                {
                    builder[i] = effect with { SpecKind = ShieldKind(absorb, hits - 1) };
                }
                else
                {
                    builder.RemoveAt(i);
                }

                next = state with { Effects = builder.ToImmutable() };
                return true;
            }

            return false;
        }

        public static PlayerId? SwapPartner(MatchState state, PlayerId owner)
        {
            if (state == null)
            {
                return null;
            }

            var order = state.TurnOrder;
            var start = order.IndexOf(owner);
            if (start < 0)
            {
                return null;
            }

            for (var step = 1; step < order.Length; step++)
            {
                var candidate = order[(start + step) % order.Length];
                var player = state.GetPlayer(candidate);
                if (player.IsAlive && !player.Disconnected)
                {
                    return candidate;
                }
            }

            return null;
        }

        public static MatchState RestoreStock(MatchState state, PlayerId owner, int amount)
        {
            var player = state.GetPlayer(owner);
            if (!player.IsAlive || amount <= 0)
            {
                return state;
            }

            var restored = Math.Min(player.Stocks + amount, Math.Max(state.Rules.StockCount, player.Stocks));
            return restored == player.Stocks ? state : state.WithPlayer(player with { Stocks = restored });
        }

        internal static string ShieldKind(float absorb, int hits) =>
            string.Format(CultureInfo.InvariantCulture, "{0}{1}*{2}", ShieldPrefix, absorb, hits);

        internal static string RuleKind(string key, float value, int turns) =>
            string.Format(CultureInfo.InvariantCulture, "rule:{0}*{1}/{2}", key, value, turns);

        static bool TryReadRule(string kind, string key, out float value)
        {
            value = 1f;
            var prefix = $"rule:{key}*";
            if (kind == null || !kind.StartsWith(prefix))
            {
                return false;
            }

            var slash = kind.IndexOf('/');
            var raw = slash > prefix.Length ? kind.Substring(prefix.Length, slash - prefix.Length) : kind.Substring(prefix.Length);
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        static bool TryReadShieldHits(string kind, out float absorb, out int hits)
        {
            absorb = 0f;
            hits = 0;
            if (kind == null || !kind.StartsWith(ShieldPrefix))
            {
                return false;
            }

            var star = kind.IndexOf('*');
            if (star < 0)
            {
                return false;
            }

            return float.TryParse(
                       kind.Substring(ShieldPrefix.Length, star - ShieldPrefix.Length),
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out absorb)
                   && int.TryParse(kind.Substring(star + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out hits)
                   && hits > 0;
        }
    }
}
