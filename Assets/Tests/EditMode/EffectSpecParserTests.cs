using EraserWars.Cards;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class EffectSpecParserTests
    {
        [Test]
        public void Parses_ModifyStat_and_ExtraFlick()
        {
            Assert.IsTrue(EffectSpecParser.TryParse("ModifyStat:Mass:1.5:1", out var stat));
            Assert.IsInstanceOf<ModifyStat>(stat);
            var modify = (ModifyStat)stat;
            Assert.AreEqual("Mass", modify.Stat);
            Assert.AreEqual(1.5f, modify.Multiplier);
            Assert.AreEqual(1, modify.Turns);

            Assert.IsTrue(EffectSpecParser.TryParse("ExtraFlick", out var extra));
            Assert.IsInstanceOf<ExtraFlick>(extra);
        }

        [Test]
        public void SummarizeJa_uses_school_labels()
        {
            var text = EffectSpecParser.SummarizeJa("ModifyRule:OpponentTurnTime:0.5:1");
            StringAssert.Contains("相手ターン時間", text);
        }
    }
}
