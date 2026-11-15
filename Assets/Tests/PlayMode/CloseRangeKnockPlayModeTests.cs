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
        GameObject root;
        EraserBody striker;
        EraserBody target;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("CloseRangeKnockRig");
            CreateDesk();
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
            var relay = striker.gameObject.AddComponent<EraserCollisionRelay>();
            var otherRelay = target.gameObject.AddComponent<EraserCollisionRelay>();
            relay.Bind(null, () => true);
            otherRelay.Bind(null, () => true);

            // Weak approach that used to feel like a soft nudge at point-blank.
            striker.Body.linearVelocity = new Vector3(0.25f, 0f, 0f);
            target.Body.linearVelocity = Vector3.zero;

            // Drive one FixedUpdate so VelocityBeforePhysics snapshots, then force resolve math.
            yield return new WaitForFixedUpdate();

            // Normal on the striker points from target toward striker (-X when striking from the left).
            var n = Vector3.left;
            var v1n = Vector3.Dot(striker.VelocityBeforePhysics, n);
            var v2n = Vector3.Dot(target.VelocityBeforePhysics, n);
            CollisionMath.EnsureMinimumClosing(ref v1n, ref v2n, 2f);
            CollisionMath.ResolveNormalSpeeds(
                striker.Body.mass,
                target.Body.mass,
                v1n,
                v2n,
                1f,
                out var r1,
                out var r2);
            CollisionMath.EnsureMinimumLaunch(
                striker.Body.mass,
                target.Body.mass,
                v1n,
                v2n,
                ref r1,
                ref r2,
                2.4f);

            Assert.LessOrEqual(r2, -2.4f + 0.05f, "Point-blank hits must still launch the target.");
            Assert.Greater(Mathf.Abs(r2), Mathf.Abs(v2n) + 0.5f, "Target normal speed must jump after the hit.");
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
