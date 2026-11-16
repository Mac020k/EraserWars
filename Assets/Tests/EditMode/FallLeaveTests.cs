using System.Collections;
using EraserWars.CameraRig;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.EditMode
{
    public sealed class FallLeaveTests
    {
        [Test]
        public void LeaveStaysInsideTheFallBudget()
        {
            var focus = new Vector3(1f, 2f, 3f);
            var stretched = FallLeave.Begin(focus, nowUnscaled: 10f, requestedSeconds: 5f);
            Assert.AreEqual(focus, stretched.Focus);
            Assert.LessOrEqual(stretched.UntilUnscaled - 10f, 0.8f);
            Assert.Greater(stretched.UntilUnscaled - 10f, 0f);
            Assert.IsTrue(stretched.Active(10f));
            Assert.IsFalse(stretched.Active(10.8f));

            var usual = FallLeave.Begin(Vector3.zero, nowUnscaled: 0f, FallLeave.DefaultSeconds);
            Assert.LessOrEqual(usual.UntilUnscaled, 0.8f);
            Assert.Greater(usual.UntilUnscaled, 0f);
        }

        [Test]
        public void CameraKeepsOneLeaveAndClearsItForTheDesk()
        {
            var go = new GameObject("fall-leave-cam");
            try
            {
                var camera = go.AddComponent<OrbitCamera>();
                camera.PlayFallLeave(Vector3.one, nowUnscaled: 3f, seconds: 0.45f);
                camera.PlayFallLeave(new Vector3(4f, 0.7f, -1f), nowUnscaled: 3.2f, seconds: 4f);
                Assert.AreEqual(new Vector3(4f, 0.7f, -1f), camera.FallLeaveFocus);
                Assert.LessOrEqual(camera.FallLeaveUntilUnscaled - 3.2f, 0.8f);
                Assert.IsTrue(camera.FallLeaveActiveAt(3.2f));
                Assert.IsFalse(camera.FallLeaveActiveAt(4f));
                camera.FrameDeskOverview();
                Assert.IsFalse(camera.FallLeaveActiveAt(3.2f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void FlashIsAColliderFreeMarkUnderTheBudget()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject ghost = null;
            try
            {
                var flashed = FallSilhouette.TryFlash(
                    cube.GetComponent<Renderer>(),
                    seconds: 5f,
                    nowUnscaled: 2f,
                    out ghost);
                Assert.IsTrue(flashed, "URP unlit or Unlit/Color should build the leave mark");
                Assert.AreNotSame(cube, ghost);
                Assert.IsNull(ghost.GetComponent<Collider>());
                Assert.IsNull(ghost.GetComponent<Rigidbody>());
                Assert.IsNotNull(ghost.GetComponent<MeshRenderer>());
                Assert.LessOrEqual(ghost.GetComponent<FallSilhouette>().EndsAtUnscaled - 2f, 0.8f);
                Assert.Greater(ghost.GetComponent<FallSilhouette>().EndsAtUnscaled, 2f);
            }
            finally
            {
                if (ghost != null)
                {
                    Object.DestroyImmediate(ghost);
                }

                Object.DestroyImmediate(cube);
            }
        }

        [UnityTest]
        public IEnumerator GlanceTracksTheExitOnWallClockWhileSlowed()
        {
            var go = new GameObject("fall-leave-cam");
            var camera = go.AddComponent<OrbitCamera>();
            yield return null;
            var previousScale = Time.timeScale;
            Time.timeScale = 0.2f;
            try
            {
                var exit = new Vector3(1.4f, 0.2f, 1.1f);
                var beforeAim = Vector3.Angle(go.transform.forward, exit - go.transform.position);
                var started = Time.realtimeSinceStartup;
                camera.PlayFallLeave(exit, Time.unscaledTime, FallLeave.DefaultSeconds);
                yield return new WaitForSecondsRealtime(0.3f);
                var afterAim = Vector3.Angle(go.transform.forward, exit - go.transform.position);
                Assert.Less(afterAim, beforeAim);
                Assert.Less(afterAim, 15f);
                while (camera.FallLeaveActiveAt(Time.unscaledTime))
                {
                    Assert.Less(Time.realtimeSinceStartup - started, 0.8f);
                    yield return null;
                }

                Assert.Less(Time.realtimeSinceStartup - started, 0.8f);
            }
            finally
            {
                Time.timeScale = previousScale;
                Object.DestroyImmediate(go);
            }
        }
    }
}
