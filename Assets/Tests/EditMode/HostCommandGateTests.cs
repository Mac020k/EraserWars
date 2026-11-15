using EraserWars.Core;
using EraserWars.Net;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class HostCommandGateTests
    {
        static MatchState AimState()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            return MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
        }

        static FlickCommand FlickFor(PlayerId player, float drag = 0.5f) =>
            new(player, 0f, 1f, drag, 0f, 0f, 0.1f);

        [Test]
        public void ValidateFlick_AcceptsCurrentPlayerInAim()
        {
            var state = AimState();
            Assert.IsTrue(HostCommandGate.ValidateFlick(state, FlickFor(state.CurrentPlayerId), out var reason));
            Assert.IsNull(reason);
        }

        [Test]
        public void ValidateFlick_RejectsWrongPlayer()
        {
            var state = AimState();
            var other = new PlayerId(1);
            Assert.AreNotEqual(state.CurrentPlayerId, other);
            Assert.IsFalse(HostCommandGate.ValidateFlick(state, FlickFor(other), out var reason));
            Assert.AreEqual("not-your-turn", reason);
        }

        [Test]
        public void ValidateFlick_RejectsDragOutOfRange()
        {
            var state = AimState();
            Assert.IsFalse(HostCommandGate.ValidateFlick(state, FlickFor(state.CurrentPlayerId, 1.5f), out var reason));
            Assert.AreEqual("drag-out-of-range", reason);
        }

        [Test]
        public void ValidateFlick_RejectsWhenSimulating()
        {
            var state = AimState();
            state = MatchReduce.Reduce(state, new Flicked(FlickFor(state.CurrentPlayerId)));
            Assert.AreEqual(Phase.Simulating, state.Phase);
            Assert.IsFalse(HostCommandGate.ValidateFlick(state, FlickFor(state.CurrentPlayerId), out var reason));
            Assert.AreEqual("not-aiming", reason);
        }

        [Test]
        public void ValidatePass_RejectsDuringSimulating()
        {
            var state = AimState();
            var player = state.CurrentPlayerId;
            state = MatchReduce.Reduce(state, new Flicked(FlickFor(player)));
            Assert.IsFalse(HostCommandGate.ValidatePass(state, player, out var reason));
            Assert.AreEqual("cannot-pass-now", reason);
        }
    }
}
