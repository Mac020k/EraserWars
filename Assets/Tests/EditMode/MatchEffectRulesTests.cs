using System.Collections.Immutable;
using EraserWars.Cards;
using EraserWars.Core;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class MatchEffectRulesTests
    {
        static readonly CardId Card = new(99);

        static MatchState Start(int players = 2)
        {
            var state = MatchFactory.CreateHotseat(players, RuleSet.CasualStock);
            return MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
        }

        static MatchState Play(MatchState state, PlayerId owner, string raw)
        {
            Assert.IsTrue(EffectSpecParser.TryParse(raw, out var spec), raw);
            return CardResolver.Apply(state, owner, Card, new[] { spec }, target: null);
        }

        [Test]
        public void OpponentTurnTime_scales_next_opponent_turn_once()
        {
            var state = Start();
            var owner = state.CurrentPlayerId;
            state = Play(state, owner, "ModifyRule:OpponentTurnTime:0.5:1");
            Assert.AreEqual(20f, state.TurnTimeLeft);

            state = MatchReduce.Reduce(state, new Passed(owner));
            state = MatchEffectRules.OnTurnHandover(state);

            Assert.AreEqual(10f, state.TurnTimeLeft, 0.001f);
            Assert.AreEqual(0, state.Effects.Length);

            var again = MatchEffectRules.OnTurnHandover(state);
            Assert.AreEqual(10f, again.TurnTimeLeft, 0.001f);
        }

        [Test]
        public void OpponentTurnTime_ignores_owner_turn()
        {
            var state = Start();
            var owner = state.CurrentPlayerId;
            state = Play(state, owner, "ModifyRule:OpponentTurnTime:0.5:1");

            var after = MatchEffectRules.OnTurnHandover(state);

            Assert.AreEqual(20f, after.TurnTimeLeft);
            Assert.AreEqual(1, after.Effects.Length);
        }

        [Test]
        public void SelfTurnTime_extends_current_clock_immediately()
        {
            var state = Start();
            state = Play(state, state.CurrentPlayerId, "ModifyRule:SelfTurnTime:1.5:1");
            Assert.AreEqual(30f, state.TurnTimeLeft, 0.001f);
        }

        [Test]
        public void Shield_consumes_one_hit_per_fall()
        {
            var state = Start();
            var owner = state.CurrentPlayerId;
            state = Play(state, owner, "GrantShield:1:2");
            Assert.AreEqual(2, state.Effects[0].TurnsRemaining);

            Assert.IsTrue(MatchEffectRules.TryConsumeShield(state, owner, out state));
            Assert.AreEqual(1, state.Effects.Length);
            Assert.IsTrue(MatchEffectRules.TryConsumeShield(state, owner, out state));
            Assert.AreEqual(0, state.Effects.Length);
            Assert.IsFalse(MatchEffectRules.TryConsumeShield(state, owner, out _));
        }

        [Test]
        public void Shield_only_protects_its_owner()
        {
            var state = Start();
            var owner = state.CurrentPlayerId;
            state = Play(state, owner, "GrantShield:0.5:1");
            Assert.IsFalse(MatchEffectRules.TryConsumeShield(state, new PlayerId(1), out _));
        }

        [Test]
        public void SwapPartner_skips_eliminated_players()
        {
            var state = Start(3);
            var middle = state.GetPlayer(new PlayerId(1));
            state = state.WithPlayer(middle with { Stocks = 0 });
            Assert.AreEqual(new PlayerId(2), MatchEffectRules.SwapPartner(state, new PlayerId(0)));
            Assert.AreEqual(new PlayerId(0), MatchEffectRules.SwapPartner(state, new PlayerId(2)));
        }

        [Test]
        public void RestoreStock_caps_at_rule_stock_count()
        {
            var state = Start();
            var owner = state.CurrentPlayerId;
            var hurt = state.WithPlayer(state.GetPlayer(owner) with { Stocks = 1 });

            hurt = Play(hurt, owner, "RestoreStock:1");
            Assert.AreEqual(2, hurt.GetPlayer(owner).Stocks);
            Assert.AreEqual(0, hurt.Effects.Length);

            var full = Play(state, owner, "RestoreStock:1");
            Assert.AreEqual(RuleSet.CasualStock.StockCount, full.GetPlayer(owner).Stocks);
        }

        [Test]
        public void RestoreStock_parses_and_summarizes()
        {
            Assert.IsTrue(EffectSpecParser.TryParse("RestoreStock:2", out var spec));
            Assert.AreEqual(new RestoreStock(2), spec);
            StringAssert.Contains("ストック回復", EffectSpecParser.SummarizeJa("RestoreStock:2"));
        }
    }
}
