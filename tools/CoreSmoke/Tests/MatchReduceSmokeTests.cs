using EraserWars.Core;
using NUnit.Framework;

namespace EraserWars.Core.Smoke
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

        [Test]
        public void EqualMassElasticHitTransfersSpeed()
        {
            CollisionMath.ResolveNormalSpeeds(1f, 1f, 2f, 0f, 1f, out var striker, out var target);
            Assert.AreEqual(0f, striker, 1e-5f);
            Assert.AreEqual(2f, target, 1e-5f);
        }

        [Test]
        public void WeakCloseRangeHitStillLaunches()
        {
            var v1 = -0.15f;
            var v2 = 0f;
            CollisionMath.EnsureMinimumClosing(ref v1, ref v2, 1.25f);
            CollisionMath.ResolveNormalSpeeds(1f, 1f, v1, v2, 1f, out var r1, out var r2);
            CollisionMath.EnsureMinimumLaunch(1f, 1f, v1, v2, ref r1, ref r2, 1.4f);
            Assert.LessOrEqual(r2, -1.4f + 1e-4f);
        }
    }
}
