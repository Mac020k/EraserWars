using System.Collections;
using Keshipin.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Keshipin.Tests.PlayMode
{
    public sealed class FlickMathPlayModeTests
    {
        [UnityTest]
        public IEnumerator SwayCenterAndEdgeDiffer()
        {
            const float aim = 0f;
            const float l = 1f;
            var center = FlickMath.AimAngleWithSway(aim, l, 0f, 1f, 0.35f, 0.8f, 2.5f);
            var edge = FlickMath.AimAngleWithSway(aim, l, 0.25f / 0.8f, 1f, 0.35f, 0.8f, 2.5f);
            Assert.AreNotEqual(center, edge);
            yield return null;
        }
    }
}
