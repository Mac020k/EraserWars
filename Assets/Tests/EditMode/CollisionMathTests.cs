using EraserWars.Core;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class CollisionMathTests
    {
        [Test]
        public void EqualMassElasticExchangesNormalSpeeds()
        {
            CollisionMath.ResolveNormalSpeeds(1f, 1f, 2f, 0f, 1f, out var a, out var b);
            Assert.AreEqual(0f, a, 1e-5f, "Striker should stop when hitting a resting equal mass elastically.");
            Assert.AreEqual(2f, b, 1e-5f, "Target should take the striker's normal speed.");
        }

        [Test]
        public void ClosingDetectsApproachAlongNormal()
        {
            Assert.IsTrue(CollisionMath.IsClosing(-1f, 0f));
            Assert.IsFalse(CollisionMath.IsClosing(1f, 0f));
        }

        [Test]
        public void SoftRestitutionLeavesStrikerSlowerThanTarget()
        {
            CollisionMath.ResolveNormalSpeeds(0.02f, 0.02f, 3f, 0f, 0.92f, out var a, out var b);
            Assert.Less(a, b, "After a soft hit the target should leave faster along the normal.");
            Assert.Less(a, 3f, "Striker must lose normal speed.");
            Assert.Greater(b, 0f, "Target must gain normal speed.");
        }
    }
}
