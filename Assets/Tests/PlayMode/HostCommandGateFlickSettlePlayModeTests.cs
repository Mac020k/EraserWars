using System.Collections;
using Keshipin.Core;
using Keshipin.Gameplay;
using Keshipin.Net;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Keshipin.Tests.PlayMode
{
    /// <summary>
    /// Host-authoritative canned flick path without Fish-Net scene objects
    /// (runtime NetworkObjects need Fish-Net reserialize / prefab collection).
    /// </summary>
    public sealed class HostCommandGateFlickSettlePlayModeTests
    {
        const float TimeoutSeconds = 10f;

        GameObject root;
        EraserBody eraser;
        FlickApplier flickApplier;
        SettleWatcher settleWatcher;
        MatchState state;
        bool settled;
        Vector3 spawnPose;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("HostCommandGateFlickRig");
            flickApplier = root.AddComponent<FlickApplier>();
            settleWatcher = root.AddComponent<SettleWatcher>();
            settleWatcher.Settled += () => settled = true;

            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(root.transform);
            desk.transform.position = new Vector3(0f, 0.4f, 0f);
            desk.transform.localScale = new Vector3(1.2f, 0.08f, 0.7f);

            var spawn = new GameObject("Spawn");
            spawn.transform.SetParent(root.transform);
            spawn.transform.position = new Vector3(0f, 0.55f, -0.15f);
            spawnPose = spawn.transform.position;

            var eraserGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eraserGo.name = "Eraser";
            eraserGo.transform.SetParent(root.transform);
            eraserGo.transform.position = spawnPose;
            eraserGo.transform.localScale = new Vector3(0.06f, 0.03f, 0.12f);
            var rb = eraserGo.AddComponent<Rigidbody>();
            rb.mass = 0.02f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            eraser = eraserGo.AddComponent<EraserBody>();

            state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));

            for (var step = 0; step < 30; step++)
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
        public IEnumerator GatedCannedFlickSettlesAwayFromSpawn()
        {
            var command = new FlickCommand(
                state.CurrentPlayerId,
                0f,
                1f,
                0.7f,
                0f,
                0f,
                0.25f);

            Assert.IsTrue(HostCommandGate.ValidateFlick(state, command, out var reason), reason);
            flickApplier.Apply(eraser, command);
            state = MatchReduce.Reduce(state, new Flicked(command));
            Assert.AreEqual(Phase.Simulating, state.Phase);

            settled = false;
            settleWatcher.Begin(new[] { eraser });

            var elapsed = 0f;
            while (elapsed < TimeoutSeconds && !settled)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.IsTrue(settled, "Eraser did not settle in time.");
            Assert.Greater(
                Vector3.Distance(eraser.transform.position, spawnPose),
                0.01f,
                "Eraser should move after a gated canned flick.");
        }
    }
}
