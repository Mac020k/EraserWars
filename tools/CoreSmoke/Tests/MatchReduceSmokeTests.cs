using Keshipin.Core;
using NUnit.Framework;

namespace Keshipin.Core.Smoke
{
    public sealed class MatchReduceSmokeTests
    {
        [Test]
        public void PassAdvancesTurn()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            var first = state.CurrentPlayerId;
            state = MatchReduce.Reduce(state, new TurnStarted(first));
            state = MatchReduce.Reduce(state, new Passed(first));
            Assert.AreEqual(new PlayerId(1), state.CurrentPlayerId);
        }

        [Test]
        public void FallDropsStock()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(new FlickCommand(state.CurrentPlayerId, 1, 0, 0.5f, 0, 0, 0.1f)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            Assert.AreEqual(2, state.GetPlayer(new PlayerId(1)).Stocks);
        }

        [Test]
        public void MatchEndsAfterLastStock()
        {
            var rules = RuleSet.CasualStock with { StockCount = 1 };
            var state = MatchFactory.CreateHotseat(2, rules);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(new FlickCommand(state.CurrentPlayerId, 1, 0, 0.5f, 0, 0, 0.1f)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new SimulationSettled());
            Assert.AreEqual(Phase.MatchEnd, state.Phase);
        }
    }
}
