using System.Collections.Generic;
using NUnit.Framework;

namespace EraserWars.Core.Smoke
{
    public sealed class DeckRulesSmokeTests
    {
        [Test]
        public void CopyLimitNamesTheCard()
        {
            var heavy = new DeckCardFact(1, 1, 1, "重い消しゴム", "card.weight");
            var verdict = DeckRules.Evaluate(true, 8, new List<DeckCardFact> { heavy, heavy });

            Assert.AreEqual(DeckFault.SameName, verdict.Fault);
            Assert.AreEqual("重い消しゴム は1枚までです。", verdict.Message);
            Assert.AreEqual("card.weight", verdict.NameKey);
        }

        [Test]
        public void CostOverNamesTheCapFault()
        {
            var cards = new List<DeckCardFact>
            {
                new(1, 3, 2, "重い", "a"),
                new(2, 2, 2, "中", "b"),
            };

            var verdict = DeckRules.Evaluate(true, 2, cards);

            Assert.AreEqual(DeckFault.OverCost, verdict.Fault);
            Assert.AreEqual("上限を超えています。カードを外してください。", verdict.Message);
        }

        [Test]
        public void CopyLimitWinsWhenTheCostIsAlsoOver()
        {
            var heavy = new DeckCardFact(1, 5, 1, "重い", "a");
            var verdict = DeckRules.Evaluate(true, 2, new List<DeckCardFact> { heavy, heavy });

            Assert.AreEqual(DeckFault.SameName, verdict.Fault);
            Assert.AreEqual("重い は1枚までです。", verdict.Message);
        }

        [Test]
        public void SevenCommonsReportTheSlotFault()
        {
            var cards = new List<DeckCardFact>();
            for (var i = 0; i < 7; i++)
            {
                cards.Add(new DeckCardFact(i + 1, 1, 2, "カード", "c"));
            }

            var verdict = DeckRules.Evaluate(true, 80, cards);

            Assert.AreEqual(DeckFault.Slots, verdict.Fault);
            Assert.AreEqual("コモンは6枚までです。", verdict.Message);
        }

        [Test]
        public void AFittingDeckIsLegal()
        {
            var cards = new List<DeckCardFact>
            {
                new(1, 1, 2, "軽い", "a"),
                new(1, 1, 2, "軽い", "a"),
            };

            var verdict = DeckRules.Evaluate(true, 8, cards);

            Assert.AreEqual(DeckFault.None, verdict.Fault);
            Assert.AreEqual("", verdict.Message);
        }

        [Test]
        public void MissingEraserReportsNoCharacter()
        {
            var verdict = DeckRules.Evaluate(false, 8, new List<DeckCardFact>());

            Assert.AreEqual(DeckFault.NoCharacter, verdict.Fault);
            Assert.AreEqual("キャラを選んでください。", verdict.Message);
        }
    }
}
