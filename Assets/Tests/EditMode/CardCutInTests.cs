using EraserWars.Gameplay;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class CardCutInTests
    {
        [Test]
        public void SignatureBannerHoldsLongerThanACommonCard()
        {
            var common = CardCutIn.Begin(0, 1, "ワックス", false);
            var signature = CardCutIn.Begin(0, 2, "全力", true);

            Assert.AreEqual(0.8f, common.DurationSeconds, 0.001f);
            Assert.AreEqual(1.2f, signature.DurationSeconds, 0.001f);
            Assert.Greater(signature.DurationSeconds, common.DurationSeconds);
            Assert.IsFalse(common.Signature);
            Assert.IsTrue(signature.Signature);
        }

        [Test]
        public void BannerSlashesInHoldsThenLeaves()
        {
            var cut = CardCutIn.Begin(1, 7, "席替え", false);
            var enter = cut.Pose();
            Assert.AreEqual(-720f, enter.SlidePixels, 0.01f);
            Assert.AreEqual(0f, enter.Opacity, 0.01f);

            var entering = cut.Advance(0.1f).Pose();
            Assert.AreEqual(-360f, entering.SlidePixels, 1f);
            Assert.AreEqual(0.5f, entering.Opacity, 0.02f);

            var held = cut.Advance(0.4f);
            var hold = held.Pose();
            Assert.AreEqual(0f, hold.SlidePixels, 0.01f);
            Assert.AreEqual(1f, hold.Opacity, 0.01f);
            Assert.AreEqual("席替え", held.DisplayName);
            Assert.AreEqual(1, held.Seat);
            Assert.IsFalse(held.IsFinished);

            var done = cut.Advance(cut.DurationSeconds);
            var exit = done.Pose();
            Assert.AreEqual(720f, exit.SlidePixels, 0.01f);
            Assert.AreEqual(0f, exit.Opacity, 0.01f);
            Assert.IsTrue(done.IsFinished);
        }

        [Test]
        public void BlankNameBecomesCardLabel()
        {
            var cut = CardCutIn.Begin(2, 3, "  ", true);
            Assert.AreEqual("カード", cut.DisplayName);
            Assert.AreEqual(3, cut.CardId);
        }

        [Test]
        public void ZeroAdvanceKeepsTheClock()
        {
            var cut = CardCutIn.Begin(0, 4, "カバー", false);
            var same = cut.Advance(0f);
            Assert.AreEqual(cut.RemainingSeconds, same.RemainingSeconds, 0.0001f);
            Assert.AreEqual(cut.DisplayName, same.DisplayName);
            Assert.IsFalse(same.IsFinished);
        }
    }
}
