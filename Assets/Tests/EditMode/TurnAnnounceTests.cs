using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class TurnAnnounceTests
    {
        [Test]
        public void AnnounceShowsForOneSecond()
        {
            var announce = TurnAnnounce.Begin(3);

            Assert.AreEqual(1f, TurnAnnounce.ShowSeconds);
            Assert.AreEqual(3, announce.Seat);
            Assert.AreEqual(TurnAnnounce.ShowSeconds, announce.RemainingSeconds, 0.001f);
            Assert.IsTrue(announce.IsOnScreen(covered: false));
            Assert.IsFalse(announce.IsOnScreen(covered: true));

            var mid = announce.Advance(0.4f);
            Assert.IsTrue(mid.IsShowing);
            Assert.AreEqual(0.6f, mid.RemainingSeconds, 0.001f);
            Assert.AreEqual(announce.RemainingSeconds, announce.Advance(0f).RemainingSeconds, 0.001f);

            var done = announce.Advance(TurnAnnounce.ShowSeconds);
            Assert.IsFalse(done.IsShowing);
            Assert.IsFalse(done.IsOnScreen(covered: false));
        }

        [Test]
        public void SameSeatKeepsTheClockAndANewSeatStartsOver()
        {
            var mid = TurnAnnounce.Begin(0).Advance(0.25f);

            var kept = TurnAnnounce.Next(mid, previousSeat: 0, seat: 0, Phase.Aim, opening: false);
            Assert.IsTrue(kept.HasValue);
            Assert.AreEqual(0, kept.Value.Seat);
            Assert.AreEqual(mid.RemainingSeconds, kept.Value.RemainingSeconds, 0.001f);

            var handed = TurnAnnounce.Next(mid, previousSeat: 0, seat: 1, Phase.CardSelect, opening: false);
            Assert.AreEqual(1, handed.Value.Seat);
            Assert.AreEqual(TurnAnnounce.ShowSeconds, handed.Value.RemainingSeconds, 0.001f);

            var opener = TurnAnnounce.Next(null, previousSeat: 0, seat: 0, Phase.CardSelect, opening: true);
            Assert.AreEqual(0, opener.Value.Seat);
            Assert.IsTrue(opener.Value.IsShowing);
        }

        [Test]
        public void MatchEndClearsTheAnnounce()
        {
            var live = TurnAnnounce.Begin(2);
            Assert.IsFalse(TurnAnnounce.Next(live, 2, 2, Phase.MatchEnd, opening: false).HasValue);
            Assert.IsFalse(TurnAnnounce.Next(live, 2, 0, Phase.MatchEnd, opening: true).HasValue);
        }
    }
}
