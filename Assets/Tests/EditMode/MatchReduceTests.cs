using EraserWars.Core;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class MatchReduceTests
    {
        static MatchState TwoPlayer() => MatchFactory.CreateHotseat(2, RuleSet.CasualStock);

        [Test]
        public void StockDecrementsOnFall()
        {
            var state = TwoPlayer();
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(DummyFlick(state.CurrentPlayerId)));
            var victim = new PlayerId(1);
            state = MatchReduce.Reduce(state, new EraserFell(victim, state.CurrentPlayerId));
            Assert.AreEqual(2, state.GetPlayer(victim).Stocks);
        }

        [Test]
        public void SelfFallDecrements()
        {
            var state = TwoPlayer();
            var self = state.CurrentPlayerId;
            state = MatchReduce.Reduce(state, new TurnStarted(self));
            state = MatchReduce.Reduce(state, new Flicked(DummyFlick(self)));
            state = MatchReduce.Reduce(state, new EraserFell(self, self));
            Assert.AreEqual(2, state.GetPlayer(self).Stocks);
        }

        [Test]
        public void PassAdvancesTurn()
        {
            var state = TwoPlayer();
            var first = state.CurrentPlayerId;
            state = MatchReduce.Reduce(state, new TurnStarted(first));
            state = MatchReduce.Reduce(state, new Passed(first));
            Assert.AreEqual(new PlayerId(1), state.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, state.Phase);
        }

        [Test]
        public void TimerExpiredActsAsPass()
        {
            var state = TwoPlayer();
            var first = state.CurrentPlayerId;
            state = MatchReduce.Reduce(state, new TurnStarted(first));
            state = MatchReduce.Reduce(state, new TimerExpired(first));
            Assert.AreEqual(new PlayerId(1), state.CurrentPlayerId);
        }

        [Test]
        public void SameRoundFallsShareEliminatedRound()
        {
            var rules = RuleSet.CasualStock with { StockCount = 1 };
            var state = MatchFactory.CreateHotseat(3, rules);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(DummyFlick(state.CurrentPlayerId)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(2), new PlayerId(0)));
            Assert.AreEqual(1, state.GetPlayer(new PlayerId(1)).EliminatedRound);
            Assert.AreEqual(1, state.GetPlayer(new PlayerId(2)).EliminatedRound);
        }

        [Test]
        public void MatchEndsWhenOneSurvivorAfterRound()
        {
            var rules = RuleSet.CasualStock with { StockCount = 1 };
            var state = MatchFactory.CreateHotseat(2, rules);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(DummyFlick(state.CurrentPlayerId)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new SimulationSettled());
            Assert.AreEqual(Phase.MatchEnd, state.Phase);
        }

        [Test]
        public void RoundContinuesForLaterSeatAfterOneSurvivor()
        {
            var rules = RuleSet.CasualStock with { StockCount = 1 };
            var state = MatchFactory.CreateHotseat(3, rules);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(DummyFlick(state.CurrentPlayerId)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(0), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new SimulationSettled());
            Assert.AreEqual(new PlayerId(2), state.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, state.Phase);
            state = MatchReduce.Reduce(state, new Passed(state.CurrentPlayerId));
            Assert.AreEqual(Phase.MatchEnd, state.Phase);
        }

        [Test]
        public void DisconnectedPlayerIsSkipped()
        {
            var state = TwoPlayer();
            state = MatchReduce.Reduce(state, new PlayerDisconnected(new PlayerId(1)));
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Passed(state.CurrentPlayerId));
            Assert.AreEqual(2, state.Round);
            Assert.AreEqual(new PlayerId(0), state.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, state.Phase);
        }

        [Test]
        public void TimerOffDoesNotAdvance()
        {
            var rules = RuleSet.CasualStock with { TurnTimeSeconds = 0f };
            var state = MatchFactory.CreateHotseat(2, rules);
            var first = state.CurrentPlayerId;
            state = MatchReduce.Reduce(state, new TurnStarted(first));
            state = MatchReduce.Reduce(state, new TimerExpired(first));
            Assert.AreEqual(first, state.CurrentPlayerId);
            Assert.AreEqual(Phase.CardSelect, state.Phase);
        }

        static FlickCommand DummyFlick(PlayerId player) =>
            new(player, 1f, 0f, 0.5f, 0f, 0f, 0.1f);
    }
}
