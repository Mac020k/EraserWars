using System.Collections;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.PlayMode
{
    public sealed class CloseRangeKnockPlayModeTests
    {
        const float MinTargetTravelMeters = 0.12f;

        GameObject root;
        EraserBody striker;
        EraserBody target;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("CloseRangeKnockRig");
            CreateDesk();
            // Centers 8cm apart with 6cm-wide cubes => already nearly touching.
            striker = CreateEraser("A", new Vector3(0f, 0.84f, 0f), 0);
            target = CreateEraser("B", new Vector3(0.08f, 0.84f, 0f), 1);

            for (var i = 0; i < 20; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null)
            {
                Object.Destroy(root);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator CloseRangeHitLaunchesTargetAtLeastMinSpeed()
        {
            var n = Vector3.left;
            var v1n = -0.25f;
            var v2n = 0f;
            CollisionMath.EnsureMinimumClosing(ref v1n, ref v2n, 2f);
            CollisionMath.ResolveNormalSpeeds(0.02f, 0.02f, v1n, v2n, 1f, out var r1, out var r2);
            CollisionMath.EnsureMinimumLaunch(0.02f, 0.02f, v1n, v2n, ref r1, ref r2, 2.4f);

            Assert.LessOrEqual(r2, -2.4f + 0.05f, "Point-blank hits must still launch the target.");
            Assert.Greater(Mathf.Abs(r2), Mathf.Abs(0f) + 0.5f, "Target normal speed must jump after the hit.");
            yield break;
        }

        [UnityTest]
        public IEnumerator AlreadyTouchingFlickMovesHitteeOnDeskPlane()
        {
            var relay = striker.gameObject.AddComponent<EraserCollisionRelay>();
            var otherRelay = target.gameObject.AddComponent<EraserCollisionRelay>();
            relay.Bind(null, () => true);
            otherRelay.Bind(null, () => true);

            var targetStart = target.Body.position;
            // Soft shove into an already-near neighbor (the case that felt dead in playtests).
            striker.Body.linearVelocity = new Vector3(1.2f, 0f, 0f);
            target.Body.linearVelocity = Vector3.zero;

            var bestTravel = 0f;
            for (var i = 0; i < 45; i++)
            {
                yield return new WaitForFixedUpdate();
                var delta = target.Body.position - targetStart;
                delta.y = 0f;
                bestTravel = Mathf.Max(bestTravel, delta.magnitude);
                if (bestTravel >= MinTargetTravelMeters)
                {
                    break;
                }
            }

            Assert.GreaterOrEqual(
                bestTravel,
                MinTargetTravelMeters,
                $"Hittee only traveled {bestTravel:0.000}m on XZ after a close-range shove. "
                + "Planar Stay normals must keep the launch on the desk.");
        }

        void CreateDesk()
        {
            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(root.transform, false);
            desk.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            desk.transform.localScale = new Vector3(1.2f, 0.04f, 0.8f);
        }

        EraserBody CreateEraser(string name, Vector3 position, int player)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root.transform, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.06f, 0.03f, 0.12f);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.02f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var body = go.AddComponent<EraserBody>();
            body.PlayerIndex = player;
            return body;
        }
    }
}
