using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class EdgeSlowmoCueTests
    {
        bool hadReduce;
        int reduceBefore;
        float scaleBefore;

        [SetUp]
        public void SetUp()
        {
            scaleBefore = Time.timeScale;
            hadReduce = PlayerPrefs.HasKey(EdgeSlowmoCue.ReduceMotionPref);
            reduceBefore = PlayerPrefs.GetInt(EdgeSlowmoCue.ReduceMotionPref, 0);
            PlayerPrefs.DeleteKey(EdgeSlowmoCue.ReduceMotionPref);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = scaleBefore;
            if (hadReduce)
            {
                PlayerPrefs.SetInt(EdgeSlowmoCue.ReduceMotionPref, reduceBefore);
            }
            else
            {
                PlayerPrefs.DeleteKey(EdgeSlowmoCue.ReduceMotionPref);
            }
        }

        [Test]
        public void RimRisesToAHoldInsideTheSlowmoBeat()
        {
            Assert.AreEqual(0f, EdgeSlowmoCue.At(false, 0.2f, false).EdgeAlpha, 0.0001f);
            Assert.AreEqual(0f, EdgeSlowmoCue.At(false, 0.2f, true).EdgeAlpha, 0.0001f);

            var open = EdgeSlowmoCue.At(true, 0f, false).EdgeAlpha;
            var held = EdgeSlowmoCue.At(true, 0.12f, false).EdgeAlpha;
            var still = EdgeSlowmoCue.At(true, 0.2f, false).EdgeAlpha;
            Assert.Greater(open, 0.25f);
            Assert.Less(open, held);
            Assert.Greater(held, 0.7f);
            Assert.AreEqual(held, still, 0.0001f);

            var quiet = EdgeSlowmoCue.At(true, 0f, true).EdgeAlpha;
            Assert.AreEqual(quiet, EdgeSlowmoCue.At(true, 0.2f, true).EdgeAlpha, 0.0001f);
            Assert.Greater(quiet, 0.1f);
            Assert.Less(quiet, open);
        }

        [Test]
        public void ReduceMotionPrefStaysOffUntilItIsStoredOn()
        {
            Assert.IsFalse(EdgeSlowmoCue.ReduceMotionPreferred());

            PlayerPrefs.SetInt(EdgeSlowmoCue.ReduceMotionPref, 0);
            Assert.IsFalse(EdgeSlowmoCue.ReduceMotionPreferred());

            PlayerPrefs.SetInt(EdgeSlowmoCue.ReduceMotionPref, 1);
            Assert.IsTrue(EdgeSlowmoCue.ReduceMotionPreferred());
        }

        [Test]
        public void WatcherWithoutAMatchDoesNotScaleTime()
        {
            var host = new GameObject("edge-slowmo");
            try
            {
                var watcher = host.AddComponent<EdgeSlowmoWatcher>();
                var now = Time.unscaledTime;
                watcher.Poll(now);

                Assert.IsFalse(watcher.IsSlowing);
                Assert.AreEqual(0f, watcher.CurrentCue(now, false).EdgeAlpha, 0.0001f);
                Assert.AreEqual(scaleBefore, Time.timeScale, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
