using EraserWars.Gameplay;
using EraserWars.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class KnockPraiseBurstTests
    {
        [Test]
        public void StockDropNotifiesTheVictimAndTheKnocker()
        {
            var noticed = EraserStockLoss.TryNotice(3, 2, victimSeat: 1, knockerSeat: 0, out var loss);

            Assert.IsTrue(noticed);
            Assert.AreEqual(1, loss.VictimSeat);
            Assert.AreEqual(0, loss.KnockerSeat);
        }

        [Test]
        public void UnchangedStocksDoNotNotify()
        {
            Assert.IsFalse(EraserStockLoss.TryNotice(2, 2, 1, 0, out _));
            Assert.IsFalse(EraserStockLoss.TryNotice(0, 0, 1, 0, out _));
            Assert.IsFalse(EraserStockLoss.TryNotice(1, 2, 1, 0, out _));
        }

        [Test]
        public void PraiseGoesToTheOtherSeat()
        {
            Assert.IsTrue(KnockPraiseBurst.TryPraiseSeat(0, 1, out var seat));
            Assert.AreEqual(1, seat);
            Assert.IsTrue(KnockPraiseBurst.TryPraiseSeat(1, 3, out seat));
            Assert.AreEqual(3, seat);
        }

        [Test]
        public void SelfFallAndUnknownKnockerGetNoPraise()
        {
            Assert.IsFalse(KnockPraiseBurst.TryPraiseSeat(2, 2, out _));
            Assert.IsFalse(KnockPraiseBurst.TryPraiseSeat(0, -1, out _));
        }

        [Test]
        public void ScrapsLeaveTheSeatUpwardAndAreGoneBeforeTheFallCap()
        {
            var pieces = new PraisePiece[KnockPraiseBurst.PieceCount];
            KnockPraiseBurst.WritePieces(0f, 7, pieces);
            for (var i = 0; i < pieces.Length; i++)
            {
                Assert.AreEqual(0f, pieces[i].X, 0.001f);
                Assert.AreEqual(0f, pieces[i].Y, 0.001f);
                Assert.Greater(pieces[i].Alpha, 0.9f);
            }

            KnockPraiseBurst.WritePieces(0.2f, 7, pieces);
            var meanY = 0f;
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            for (var i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                meanY += piece.Y;
                minX = Mathf.Min(minX, piece.X);
                maxX = Mathf.Max(maxX, piece.X);
                var distance = Mathf.Sqrt(piece.X * piece.X + piece.Y * piece.Y);
                Assert.Greater(distance, 12f);
                Assert.Greater(piece.Alpha, 0.9f);
            }

            meanY /= pieces.Length;
            Assert.Less(meanY, 0f);
            Assert.Greater(maxX - minX, 20f);

            KnockPraiseBurst.WritePieces(0.8f, 7, pieces);
            for (var i = 0; i < pieces.Length; i++)
            {
                Assert.AreEqual(0f, pieces[i].Alpha, 0.001f);
            }

            KnockPraiseBurst.WritePieces(-0.1f, 7, pieces);
            Assert.AreEqual(0f, pieces[0].Alpha, 0.001f);
        }

        [Test]
        public void SameSeedRepeatsAndAnotherSeedSpreadsDifferently()
        {
            var first = new PraisePiece[KnockPraiseBurst.PieceCount];
            var second = new PraisePiece[KnockPraiseBurst.PieceCount];
            KnockPraiseBurst.WritePieces(0.2f, 11, first);
            KnockPraiseBurst.WritePieces(0.2f, 11, second);
            for (var i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].X, second[i].X);
                Assert.AreEqual(first[i].Y, second[i].Y);
            }

            KnockPraiseBurst.WritePieces(0.2f, 12, second);
            var differed = false;
            for (var i = 0; i < first.Length; i++)
            {
                if (Mathf.Abs(first[i].X - second[i].X) > 1f)
                {
                    differed = true;
                }
            }

            Assert.IsTrue(differed);
        }

        [Test]
        public void MarkPopsUpAndFadesBeforeTheFallCap()
        {
            var early = KnockPraiseBurst.WriteMark(0.02f);
            var popped = KnockPraiseBurst.WriteMark(0.12f);

            Assert.Greater(early.Alpha, 0.9f);
            Assert.Less(popped.Y, early.Y);
            Assert.Greater(popped.Scale, early.Scale);
            Assert.AreEqual(0f, KnockPraiseBurst.WriteMark(0.8f).Alpha, 0.001f);
            Assert.Less(KnockPraiseBurst.DurationSeconds, 0.8f);
        }

        [Test]
        public void ViewShowsScrapsOnTheSeatThenHidesThem()
        {
            var layer = new VisualElement { name = "knock-praise" };
            var view = new KnockPraiseView(layer, _ => null);
            var mark = layer.Q<Label>("praise-mark");
            var scraps = layer.Query(className: "praise-scrap").ToList();

            Assert.AreEqual(DisplayStyle.None, layer.style.display.value);
            Assert.AreEqual(KnockPraiseBurst.PieceCount, scraps.Count);
            Assert.AreEqual("ナイス!", mark.text);

            view.Play(1, 4);
            Assert.AreEqual(DisplayStyle.Flex, layer.style.display.value);
            Assert.Greater(mark.style.opacity.value, 0.9f);
            var markTop = Pixels(mark.style.top);

            view.Tick(0.12f);
            Assert.Less(Pixels(mark.style.top), markTop);

            view.Tick(0.08f);
            Assert.Greater(scraps[0].style.opacity.value, 0.5f);
            var minLeft = float.MaxValue;
            var maxLeft = float.MinValue;
            for (var i = 0; i < scraps.Count; i++)
            {
                var left = Pixels(scraps[i].style.left);
                minLeft = Mathf.Min(minLeft, left);
                maxLeft = Mathf.Max(maxLeft, left);
            }

            Assert.Greater(maxLeft - minLeft, 20f);

            view.Tick(0.7f);
            Assert.AreEqual(DisplayStyle.None, layer.style.display.value);
        }

        [Test]
        public void NegativeSeatDoesNotStartTheBurst()
        {
            var layer = new VisualElement();
            var view = new KnockPraiseView(layer, _ => null);

            view.Play(-1, 1);

            Assert.AreEqual(DisplayStyle.None, layer.style.display.value);
        }

        static float Pixels(StyleLength length)
        {
            Assert.AreEqual(LengthUnit.Pixel, length.value.unit);
            return length.value.value;
        }
    }
}
