using System;
using Keshipin.Core;
using NUnit.Framework;

namespace Keshipin.Tests.EditMode
{
    public sealed class FlickMathTests
    {
        [Test]
        public void ImpulseIncreasesWithDragLength()
        {
            const float power = 1f;
            const float jMax = 0.12f;
            var idle = FlickMath.Impulse(power, jMax, 0f);
            var quarter = FlickMath.Impulse(power, jMax, 0.25f);
            var half = FlickMath.Impulse(power, jMax, 0.5f);
            var full = FlickMath.Impulse(power, jMax, 1f);
            var pastFull = FlickMath.Impulse(power, jMax, 1.5f);

            Assert.AreEqual(0f, idle, 1e-6f, "A zero drag is zero impulse.");
            Assert.Greater(quarter, idle, "A short drag must beat a zero drag.");
            Assert.Greater(half, quarter, "A half drag must beat a quarter drag.");
            Assert.Greater(full, half, "A full drag must beat a half drag.");
            Assert.AreEqual(power * jMax, full, 1e-6f, "Full drag is power * jMax.");
            Assert.AreEqual(full, pastFull, 1e-6f, "Drag past 1 stays at the full impulse.");
        }

        [Test]
        public void HeldAimStaysOnThePullUntilTimePasses()
        {
            var (x, y) = FlickMath.SwayedDirection(3f, 4f, 1f, 0f, 1f, 0.35f, 0.8f, 2.5f);

            Assert.AreEqual(0.6f, x, 1e-5f, "At the press instant the aim is the pull, not a unit X.");
            Assert.AreEqual(0.8f, y, 1e-5f, "At the press instant the aim is the pull, not a unit X.");
        }

        [Test]
        public void LongPullAndLargeEraserSwingFarther()
        {
            var quarter = PeakOffset(0.25f, 1f);
            var full = PeakOffset(1f, 1f);
            var large = PeakOffset(1f, 2f);

            Assert.AreEqual(0.35f * 0.25f * 0.25f, quarter, 1e-4f, "A quarter pull peaks at A0 * L^2.");
            Assert.AreEqual(0.35f, full, 1e-4f, "A full pull on a normal eraser peaks at 0.35 rad.");
            Assert.AreEqual(0.70f, large, 1e-4f, "Size 2 doubles the full-pull peak.");
            Assert.Greater(full, quarter * 10f, "A full pull swings more than ten times a quarter pull.");
        }

        [Test]
        public void SwayIsAddedToThePullAngle()
        {
            var angle = PeakAngle(-1f, 0f, 1f, 1f);
            var expected = MathF.PI + 0.35f;
            if (expected > MathF.PI)
            {
                expected -= 2f * MathF.PI;
            }

            Assert.AreEqual(expected, angle, 1e-4f, "A backward pull sways around that pull, not around +X.");
        }

        static float PeakOffset(float length, float sizeFactor)
        {
            return PeakAngle(1f, 0f, length, sizeFactor);
        }

        static float PeakAngle(float directionX, float directionY, float length, float sizeFactor)
        {
            const float amplitude0 = 0.35f;
            const float frequency0 = 0.8f;
            const float frequencySlope = 2.5f;
            var frequency = frequency0 + frequencySlope * length * sizeFactor;
            var time = 0.25f / frequency;
            var (x, y) = FlickMath.SwayedDirection(
                directionX,
                directionY,
                length,
                time,
                sizeFactor,
                amplitude0,
                frequency0,
                frequencySlope);
            return MathF.Atan2(y, x);
        }
    }
}
