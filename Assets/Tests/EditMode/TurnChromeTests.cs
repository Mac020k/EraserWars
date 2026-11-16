using EraserWars.Core;
using EraserWars.UI;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class TurnChromeTests
    {
        [TestCase(Phase.TurnStart, true)]
        [TestCase(Phase.CardSelect, true)]
        [TestCase(Phase.Aim, true)]
        [TestCase(Phase.Simulating, false)]
        [TestCase(Phase.Resolve, false)]
        [TestCase(Phase.RoundEnd, false)]
        public void PassHintShowsOnlyWhileThatSeatHoldsTheDevice(Phase phase, bool showPassHint)
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock) with
            {
                Phase = phase,
                TurnTimeLeft = 12.2f,
            };

            var chrome = TurnChrome.From(state);

            Assert.AreEqual("P1 のばん", chrome.Headline);
            Assert.AreEqual(0, chrome.Seat);
            Assert.IsTrue(chrome.ShowCallout);
            Assert.AreEqual(showPassHint, chrome.ShowPassHint);
            Assert.AreEqual("このきを P1 にわたして", chrome.PassHint);
            if (showPassHint)
            {
                Assert.AreEqual("13", chrome.TimerDigits);
                Assert.IsTrue(chrome.TimerTicking);
                Assert.IsFalse(chrome.TimerUrgent);
            }
            else
            {
                Assert.AreEqual("--", chrome.TimerDigits);
                Assert.IsFalse(chrome.TimerTicking);
                Assert.IsFalse(chrome.TimerUrgent);
            }
        }

        [Test]
        public void FourthSeatKeepsItsOwnNameAndColorIndex()
        {
            var state = MatchFactory.CreateHotseat(4, RuleSet.CasualStock) with
            {
                TurnIndex = 3,
                Phase = Phase.Aim,
                TurnTimeLeft = 4.1f,
            };

            var chrome = TurnChrome.From(state);

            Assert.AreEqual(3, chrome.Seat);
            Assert.AreEqual("P4 のばん", chrome.Headline);
            Assert.IsTrue(chrome.ShowPassHint);
            Assert.AreEqual("このきを P4 にわたして", chrome.PassHint);
            Assert.AreEqual("5", chrome.TimerDigits);
            Assert.IsTrue(chrome.TimerUrgent);
        }

        [Test]
        public void TimerTurnsUrgentAtFiveSeconds()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            var calm = TurnChrome.From(state with { Phase = Phase.CardSelect, TurnTimeLeft = 5.01f });
            var urgent = TurnChrome.From(state with { Phase = Phase.CardSelect, TurnTimeLeft = 5f });
            var done = TurnChrome.From(state with { Phase = Phase.CardSelect, TurnTimeLeft = 0f });

            Assert.IsFalse(calm.TimerUrgent);
            Assert.AreEqual("6", calm.TimerDigits);
            Assert.IsTrue(urgent.TimerUrgent);
            Assert.AreEqual("5", urgent.TimerDigits);
            Assert.IsTrue(done.TimerUrgent);
            Assert.AreEqual("0", done.TimerDigits);
        }

        [Test]
        public void ZeroTurnTimeHidesTheClockAndKeepsThePassHint()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock with { TurnTimeSeconds = 0f }) with
            {
                Phase = Phase.TurnStart,
                TurnTimeLeft = 0f,
            };

            var chrome = TurnChrome.From(state);

            Assert.IsTrue(chrome.ShowPassHint);
            Assert.AreEqual("P1 のばん", chrome.Headline);
            Assert.IsFalse(chrome.TimerTicking);
            Assert.IsFalse(chrome.TimerUrgent);
            Assert.AreEqual("--", chrome.TimerDigits);
        }

        [Test]
        public void MatchEndHidesTheCallout()
        {
            var state = MatchFactory.CreateHotseat(4, RuleSet.CasualStock) with
            {
                TurnIndex = 2,
                Phase = Phase.MatchEnd,
                TurnTimeLeft = 1f,
            };

            var chrome = TurnChrome.From(state);

            Assert.IsFalse(chrome.ShowCallout);
            Assert.AreEqual(-1, chrome.Seat);
            Assert.IsFalse(chrome.ShowPassHint);
            Assert.AreEqual(string.Empty, chrome.PassHint);
            Assert.IsFalse(chrome.TimerTicking);
        }
    }
}
