using EraserWars.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class CareerXpTests
    {
        bool had;
        int previous;

        [SetUp]
        public void ClearTotal()
        {
            had = PlayerPrefs.HasKey(CareerXp.TotalKey);
            previous = PlayerPrefs.GetInt(CareerXp.TotalKey, 0);
            PlayerPrefs.DeleteKey(CareerXp.TotalKey);
        }

        [TearDown]
        public void RestoreTotal()
        {
            if (had)
            {
                PlayerPrefs.SetInt(CareerXp.TotalKey, previous);
            }
            else
            {
                PlayerPrefs.DeleteKey(CareerXp.TotalKey);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void MissingTotalReadsAsZero()
        {
            Assert.AreEqual(0, CareerXp.ReadTotal());
            Assert.IsFalse(PlayerPrefs.HasKey(CareerXp.TotalKey));
        }

        [Test]
        public void EachFinishedMatchAddsTenToTheStoredTotal()
        {
            var first = CareerXp.GrantFinishedMatch();
            var second = CareerXp.GrantFinishedMatch();

            Assert.AreEqual(10, first.Gained);
            Assert.AreEqual(10, first.Total);
            Assert.AreEqual(10, second.Gained);
            Assert.AreEqual(20, second.Total);
            Assert.AreEqual(20, CareerXp.ReadTotal());
            Assert.AreEqual(20, PlayerPrefs.GetInt(CareerXp.TotalKey));
        }

        [Test]
        public void NegativeStoredTotalCountsAsZero()
        {
            PlayerPrefs.SetInt(CareerXp.TotalKey, -4);

            Assert.AreEqual(0, CareerXp.ReadTotal());
            var receipt = CareerXp.GrantFinishedMatch();

            Assert.AreEqual(10, receipt.Gained);
            Assert.AreEqual(10, receipt.Total);
            Assert.AreEqual(10, PlayerPrefs.GetInt(CareerXp.TotalKey));
        }
    }
}
