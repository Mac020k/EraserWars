using System.Reflection;
using EraserWars.CameraRig;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class HitPunchTests
    {
        bool hadShake;
        int prevShake;

        [SetUp]
        public void SetUp()
        {
            hadShake = PlayerPrefs.HasKey(HitstopController.CameraShakePref);
            prevShake = PlayerPrefs.GetInt(HitstopController.CameraShakePref, 1);
        }

        [TearDown]
        public void TearDown()
        {
            if (hadShake)
            {
                PlayerPrefs.SetInt(HitstopController.CameraShakePref, prevShake);
            }
            else
            {
                PlayerPrefs.DeleteKey(HitstopController.CameraShakePref);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void TapFloorHoldsTwoFramesAndAFullExchangeHoldsTen()
        {
            var tap = new NormalHit(0.048f, 1f, true);
            var mid = new NormalHit(0.084f, 1f, true);
            var full = new NormalHit(0.12f, 1f, true);

            Assert.AreEqual(2, tap.Frames);
            Assert.AreEqual(6, mid.Frames);
            Assert.AreEqual(10, full.Frames);
            Assert.AreEqual(0.012f, tap.KickMeters(true), 0.0001f);
            Assert.AreEqual(0.05f, full.KickMeters(true), 0.0001f);
            Assert.AreEqual(0f, full.KickMeters(false), 0.0001f);
            Assert.Greater(full.KickMeters(true), mid.KickMeters(true));
            Assert.Greater(mid.KickMeters(true), tap.KickMeters(true));
        }

        [Test]
        public void EqualMassExchangeReportsTheLaunchedBodysMomentum()
        {
            var hit = NormalHit.FromSpeeds(0.02f, 3.6f, 0f, 0.02f, 0f, -3.6f);
            Assert.AreEqual(0.072f, hit.Impulse, 0.0001f);
            Assert.AreEqual(-1f, hit.Sign, 0.0001f);
            Assert.IsTrue(hit.LaunchIsSecond);
            Assert.Greater(hit.Frames, 2);
            Assert.Less(hit.Frames, 10);
        }

        [Test]
        public void PunchStartsHardDecaysAndStaysOnTheDeskPlane()
        {
            var started = 4f;
            var duration = 0.2f;
            var punch = CameraPunch.Begin(new Vector3(0f, 4f, 1f), 0.05f, started, duration);
            var start = punch.OffsetAt(started);
            var mid = punch.OffsetAt(started + duration * 0.5f);
            Assert.AreEqual(0f, start.x, 0.0001f);
            Assert.AreEqual(0f, start.y, 0.0001f);
            Assert.AreEqual(0.05f, start.z, 0.0001f);
            Assert.Greater(start.magnitude, mid.magnitude);
            Assert.Greater(mid.magnitude, 0f);
            Assert.AreEqual(0f, punch.OffsetAt(started + duration + 0.05f).magnitude, 0.0001f);
        }

        [Test]
        public void PulledBackCameraKicksFartherUntilTheCap()
        {
            Assert.AreEqual(1f, CameraPunch.DistanceScale(1.85f, 1.85f), 0.0001f);
            Assert.AreEqual(0.85f, CameraPunch.DistanceScale(0.4f, 1.85f), 0.0001f);
            Assert.AreEqual(1.6f, CameraPunch.DistanceScale(7f, 1.85f), 0.0001f);

            var go = new GameObject("wide-cam");
            try
            {
                var camera = go.AddComponent<OrbitCamera>();
                var distance = typeof(OrbitCamera).GetField(
                    "distance",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(distance);
                distance.SetValue(camera, 7f);
                camera.Punch(Vector3.right, 0.05f, nowUnscaled: 0f, duration: 1f);
                Assert.AreEqual(0.08f, camera.PunchOffsetAt(0f).magnitude, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeskReframeDropsALeftoverPunch()
        {
            var go = new GameObject("reframe-cam");
            try
            {
                var camera = go.AddComponent<OrbitCamera>();
                camera.Punch(Vector3.right, 0.05f, Time.unscaledTime, 0.2f);
                Assert.Greater(camera.PunchOffsetAt(Time.unscaledTime).magnitude, 0.01f);
                camera.FrameDeskOverview();
                Assert.AreEqual(0f, camera.PunchOffsetAt(Time.unscaledTime).magnitude, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AcceptedHitScalesHoldAndKick()
        {
            using var rig = new HitRig();
            PlayerPrefs.SetInt(HitstopController.CameraShakePref, 1);
            var now = Time.unscaledTime;

            rig.Hitstop.BeginShot();
            rig.Hitstop.PresentAcceptedHit(new NormalHit(0.048f, 1f, true), Vector3.right, null);
            Assert.AreEqual(2f / 60f, rig.Camera.PunchDuration, 0.0001f);
            Assert.AreEqual(0.012f, rig.Camera.PunchOffsetAt(now).magnitude, 0.0001f);

            rig.Hitstop.BeginShot();
            rig.Hitstop.PresentAcceptedHit(new NormalHit(0.12f, 1f, true), Vector3.right, null);
            Assert.AreEqual(10f / 60f, rig.Camera.PunchDuration, 0.0001f);
            Assert.AreEqual(0.05f, rig.Camera.PunchOffsetAt(now).magnitude, 0.0001f);
        }

        [Test]
        public void CameraShakePrefSilencesTheKickAndKeepsTheHold()
        {
            using var rig = new HitRig();
            PlayerPrefs.SetInt(HitstopController.CameraShakePref, 0);
            rig.Hitstop.BeginShot();
            rig.Hitstop.PresentAcceptedHit(new NormalHit(0.12f, 1f, true), Vector3.forward, null);
            Assert.AreEqual(0f, rig.Camera.PunchOffsetAt(Time.unscaledTime).magnitude, 0.0001f);
            Assert.AreEqual(10f / 60f, rig.Camera.PunchDuration, 0.0001f);
        }

        [Test]
        public void MissingShakePrefStillPunches()
        {
            using var rig = new HitRig();
            PlayerPrefs.DeleteKey(HitstopController.CameraShakePref);
            rig.Hitstop.BeginShot();
            rig.Hitstop.PresentAcceptedHit(new NormalHit(0.12f, 1f, true), Vector3.right, null);
            Assert.AreEqual(0.05f, rig.Camera.PunchOffsetAt(Time.unscaledTime).magnitude, 0.0001f);
        }

        [Test]
        public void ShotBudgetDropsHitsAfterThirtyFrames()
        {
            using var rig = new HitRig();
            PlayerPrefs.SetInt(HitstopController.CameraShakePref, 1);
            rig.Hitstop.BeginShot();
            var now = Time.unscaledTime;
            var full = new NormalHit(0.12f, 1f, true);
            rig.Hitstop.PresentAcceptedHit(full, Vector3.right, null);
            rig.Hitstop.PresentAcceptedHit(full, Vector3.forward, null);
            rig.Hitstop.PresentAcceptedHit(full, Vector3.left, null);
            var held = rig.Camera.PunchOffsetAt(now);
            Assert.Less(Vector3.Dot(held.normalized, Vector3.right), -0.9f);
            Assert.AreEqual(10f / 60f, rig.Camera.PunchDuration, 0.0001f);

            rig.Hitstop.PresentAcceptedHit(full, Vector3.back, null);
            Assert.AreEqual(held, rig.Camera.PunchOffsetAt(now));
        }

        [Test]
        public void ZeroImpulseDoesNotReplaceTheCurrentPunch()
        {
            using var rig = new HitRig();
            PlayerPrefs.SetInt(HitstopController.CameraShakePref, 1);
            rig.Hitstop.BeginShot();
            var now = Time.unscaledTime;
            rig.Hitstop.PresentAcceptedHit(new NormalHit(0.12f, 1f, true), Vector3.right, null);
            var held = rig.Camera.PunchOffsetAt(now);
            rig.Hitstop.PresentAcceptedHit(new NormalHit(0f, 1f, false), Vector3.forward, null);
            Assert.AreEqual(held, rig.Camera.PunchOffsetAt(now));
        }

        sealed class HitRig : System.IDisposable
        {
            public readonly GameObject Root;
            public readonly OrbitCamera Camera;
            public readonly HitstopController Hitstop;

            public HitRig()
            {
                Root = new GameObject("hit-punch");
                Camera = Root.AddComponent<OrbitCamera>();
                var distance = typeof(OrbitCamera).GetField(
                    "distance",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                distance.SetValue(Camera, 1.85f);
                Hitstop = Root.AddComponent<HitstopController>();
                Hitstop.BindCamera(Camera);
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    Object.DestroyImmediate(Root);
                }
            }
        }
    }
}
