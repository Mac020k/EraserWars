using System;
using EraserWars.Core;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class FlickMathTests
    {
        [Test]
        public void CenterContactHasNoYawAndTheEdgeSpinsHarder()
        {
            const float impulse = 0.24f;
            const float halfZ = 0.014f;
            var center = FlickMath.YawImpulse(0f, 0f, impulse, 0f, 1f);
            var middle = FlickMath.YawImpulse(0f, halfZ * 0.5f, impulse, 0f, 1f);
            var edge = FlickMath.YawImpulse(0f, halfZ, impulse, 0f, 1f);
            var alongPush = FlickMath.YawImpulse(0.03f, 0f, impulse, 0f, 1f);

            Assert.AreEqual(0f, center, 1e-8f, "A center contact has no yaw lever.");
            Assert.AreEqual(0f, alongPush, 1e-8f, "A contact on the push line has no yaw lever.");
            Assert.AreEqual(halfZ * 0.5f * impulse, middle, 1e-6f, "Half the lever is half the yaw.");
            Assert.AreEqual(halfZ * impulse, edge, 1e-6f, "Yaw is the vertical part of r cross J.");
            Assert.Greater(MathF.Abs(edge), MathF.Abs(middle), "The edge spins harder than the middle.");
            Assert.Greater(MathF.Abs(middle), MathF.Abs(center), "The middle spins harder than the center.");
        }

        [Test]
        public void SpinGainScalesYawAndContactStaysInsideTheFace()
        {
            var once = FlickMath.YawImpulse(0f, 0.014f, 0.24f, 0f, 1f);
            var twice = FlickMath.YawImpulse(0f, 0.014f, 0.24f, 0f, 2f);
            var local = FlickMath.LocalContact(4f, -3f, 0.5f, 0.5f);

            Assert.AreEqual(once * 2f, twice, 1e-6f, "Spin gain multiplies yaw and leaves the impulse.");
            Assert.AreEqual(0.5f, local.x, 1e-6f, "Offset past the face clamps to the local half extent.");
            Assert.AreEqual(0f, local.y, 1e-6f, "The contact stays at center height so a centered flick does not tip.");
            Assert.AreEqual(-0.5f, local.z, 1e-6f, "Offset past the face clamps to the local half extent.");
        }

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

        [Test]
        public void FullPullKeepsPowerOnThePullWhileBlurSitsAtThePeak()
        {
            const float meters = 0.8f;
            var time = PeakTime(1f, 1f);
            var sample = FlickMath.SampleAim(1f, 0f, 1f, time, 1f, meters, 0.35f, 0.8f, 2.5f);

            Assert.AreEqual(meters, sample.LengthMeters, 1e-5f, "Full drag reaches the full aim length.");
            Assert.AreEqual(1f, sample.CenterX, 1e-5f, "The power line stays on the pull.");
            Assert.AreEqual(0f, sample.CenterY, 1e-5f, "The power line stays on the pull.");
            Assert.AreEqual(0.35f, sample.HalfAngleRadians, 1e-4f, "A full pull on a normal eraser opens 0.35 rad of blur.");
            Assert.AreEqual(MathF.Cos(0.35f), sample.ReleaseX, 1e-4f, "The release arrow sits on the sway peak.");
            Assert.AreEqual(MathF.Sin(0.35f), sample.ReleaseY, 1e-4f, "The release arrow sits on the sway peak.");
            Assert.AreEqual(0.35f, SignedDelta(0f, MathF.Atan2(sample.LeftY, sample.LeftX)), 1e-4f, "The fan's left edge is +A from the pull.");
            Assert.AreEqual(-0.35f, SignedDelta(0f, MathF.Atan2(sample.RightY, sample.RightX)), 1e-4f, "The fan's right edge is -A from the pull.");
        }

        [Test]
        public void ZeroCrossingStillShowsTheBlurFan()
        {
            var sample = FlickMath.SampleAim(0f, 1f, 1f, 0f, 1f, 0.8f, 0.35f, 0.8f, 2.5f);
            var center = MathF.Atan2(sample.CenterY, sample.CenterX);

            Assert.AreEqual(0f, sample.CenterX, 1e-5f, "A pull along the plane +Y stays on +Y.");
            Assert.AreEqual(1f, sample.CenterY, 1e-5f, "A pull along the plane +Y stays on +Y.");
            Assert.AreEqual(sample.CenterX, sample.ReleaseX, 1e-4f, "At the press instant the arrow lies on the power line.");
            Assert.AreEqual(sample.CenterY, sample.ReleaseY, 1e-4f, "At the press instant the arrow lies on the power line.");
            Assert.AreEqual(0.35f, sample.HalfAngleRadians, 1e-4f, "The fan stays open when the arrow crosses the center.");
            Assert.AreEqual(0.35f, SignedDelta(center, MathF.Atan2(sample.LeftY, sample.LeftX)), 1e-4f);
            Assert.AreEqual(-0.35f, SignedDelta(center, MathF.Atan2(sample.RightY, sample.RightX)), 1e-4f);
        }

        [Test]
        public void ShortPullShrinksPowerAndBlurTogether()
        {
            var quarter = FlickMath.SampleAim(3f, 4f, 0.25f, 0f, 1f, 0.8f, 0.35f, 0.8f, 2.5f);
            var full = FlickMath.SampleAim(3f, 4f, 1f, 0f, 1f, 0.8f, 0.35f, 0.8f, 2.5f);

            Assert.AreEqual(0.2f, quarter.LengthMeters, 1e-5f, "A quarter drag is a quarter of the full length.");
            Assert.AreEqual(0.35f * 0.25f * 0.25f, quarter.HalfAngleRadians, 1e-4f, "Blur grows with L squared.");
            Assert.AreEqual(0.6f, quarter.CenterX, 1e-5f, "Power follows the pull, not a unit axis.");
            Assert.AreEqual(0.8f, quarter.CenterY, 1e-5f, "Power follows the pull, not a unit axis.");
            Assert.Greater(full.LengthMeters, quarter.LengthMeters, "A longer drag draws a longer power line.");
            Assert.Greater(full.HalfAngleRadians, quarter.HalfAngleRadians * 10f, "A full pull opens much more blur than a quarter pull.");
        }

        [Test]
        public void LargeEraserOpensTheFanToTheReleasePeak()
        {
            var time = PeakTime(1f, 2f);
            var sample = FlickMath.SampleAim(1f, 0f, 1f, time, 2f, 0.8f, 0.35f, 0.8f, 2.5f);

            Assert.AreEqual(0.70f, sample.HalfAngleRadians, 1e-4f, "Size 2 doubles the full-pull fan.");
            Assert.AreEqual(1f, sample.CenterX, 1e-5f, "Power stays on the pull for a large eraser.");
            Assert.AreEqual(0f, sample.CenterY, 1e-5f, "Power stays on the pull for a large eraser.");
            Assert.AreEqual(MathF.Cos(0.70f), sample.ReleaseX, 1e-4f, "The arrow peak stays on the fan edge.");
            Assert.AreEqual(MathF.Sin(0.70f), sample.ReleaseY, 1e-4f, "The arrow peak stays on the fan edge.");
        }

        static float PeakTime(float length, float sizeFactor)
        {
            const float frequency0 = 0.8f;
            const float frequencySlope = 2.5f;
            var frequency = frequency0 + frequencySlope * length * sizeFactor;
            return 0.25f / frequency;
        }

        static float SignedDelta(float from, float to)
        {
            var delta = to - from;
            if (delta > MathF.PI)
            {
                delta -= 2f * MathF.PI;
            }

            if (delta < -MathF.PI)
            {
                delta += 2f * MathF.PI;
            }

            return delta;
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
