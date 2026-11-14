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
    }
}
