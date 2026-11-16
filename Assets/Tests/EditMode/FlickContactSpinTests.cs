using System.Collections.Generic;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class FlickContactSpinTests
    {
        const float Mass = 0.02f;
        const float JMax = 0.24f;
        const float Step = 0.01f;

        readonly List<Object> owned = new();
        SimulationMode simulationMode;

        [SetUp]
        public void SetUp()
        {
            simulationMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            Physics.simulationMode = simulationMode;
        }

        [Test]
        public void EdgeReleaseSpinsMoreThanACenterRelease()
        {
            var center = Release(0f, 0f, null);
            var edge = Release(0f, 1f, null);
            var expected = ExpectedYawRate(1f);

            Assert.Less(center.angularVelocity.magnitude, Mathf.Abs(expected) * 0.05f, "A center release does not spin or tip.");
            Assert.Greater(Mathf.Abs(edge.angularVelocity.y), Mathf.Abs(center.angularVelocity.y), "An edge release spins harder than the center.");
            Assert.Less(Mathf.Abs(edge.angularVelocity.x), Mathf.Abs(expected) * 0.05f, "The edge lever is yaw, not pitch.");
            Assert.Less(Mathf.Abs(edge.angularVelocity.z), Mathf.Abs(expected) * 0.05f, "The edge lever is yaw, not pitch.");
            Assert.AreEqual(expected, edge.angularVelocity.y, Mathf.Abs(expected) * 0.25f, "Yaw matches r cross J on the scaled box.");
        }

        [Test]
        public void SpinGainDoublesEdgeYaw()
        {
            var once = Release(0f, 1f, Tuning(1f));
            var twice = Release(0f, 1f, Tuning(2f));
            var expected = ExpectedYawRate(2f);

            Assert.AreEqual(expected, twice.angularVelocity.y, Mathf.Abs(expected) * 0.25f, "SpinGain 2 doubles the edge yaw.");
            Assert.Greater(Mathf.Abs(twice.angularVelocity.y), Mathf.Abs(once.angularVelocity.y) * 1.5f, "SpinGain 2 beats SpinGain 1.");
        }

        Rigidbody Release(float offsetX, float offsetZ, FlickTuning tuning)
        {
            var eraserGo = Track(new GameObject("spin-eraser"));
            eraserGo.transform.position = new Vector3(80f, 40f + owned.Count, 80f);
            eraserGo.transform.localScale = new Vector3(0.06f, 0.018f, 0.028f);
            var body = eraserGo.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.mass = Mass;
            var box = eraserGo.AddComponent<BoxCollider>();
            box.size = Vector3.one;
            eraserGo.AddComponent<EraserBody>();
            body.maxAngularVelocity = 10000f;
            var applier = Track(new GameObject("spin-applier")).AddComponent<FlickApplier>();
            if (tuning != null)
            {
                applier.Tuning = tuning;
            }

            applier.Apply(
                eraserGo.GetComponent<EraserBody>(),
                new FlickCommand(new PlayerId(0), 1f, 0f, 1f, offsetX, offsetZ, 0f));
            Physics.Simulate(Step);
            return body;
        }

        FlickTuning Tuning(float spinGain)
        {
            var tuning = Track(ScriptableObject.CreateInstance<FlickTuning>());
            tuning.SpinGain = spinGain;
            return tuning;
        }

        static float ExpectedYawRate(float spinGain)
        {
            var scale = new Vector3(0.06f, 0.018f, 0.028f);
            var leverZ = 0.5f * scale.z;
            var impulse = 1f * JMax;
            var yaw = spinGain * leverZ * impulse;
            var inertia = Mass / 12f * (scale.x * scale.x + scale.z * scale.z);
            var damping = Mathf.Exp(-1.2f * Step);
            return yaw / inertia * damping;
        }

        T Track<T>(T obj) where T : Object
        {
            owned.Add(obj);
            return obj;
        }
    }
}
