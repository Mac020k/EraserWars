using EraserWars.CameraRig;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class CameraViewTests
    {
        [Test]
        public void OverviewShowsTheDeskThenReturnsToTheChase()
        {
            var cameraObject = new GameObject("view-cam");
            var eraser = new GameObject("eraser");
            try
            {
                eraser.transform.position = new Vector3(1.6f, 0.8f, 1.2f);
                var desk = new Vector3(0f, 0.83f, 0f);
                var camera = cameraObject.AddComponent<OrbitCamera>();
                camera.SetChaseTarget(eraser.transform);
                camera.Snap();
                Assert.Less(AimError(cameraObject.transform, eraser.transform.position), 1f);
                var chaseBoom = Vector3.Distance(cameraObject.transform.position, eraser.transform.position);

                camera.ClearChaseTarget();
                camera.Snap();
                Assert.Less(AimError(cameraObject.transform, desk), 1f);
                var deskBoom = Vector3.Distance(cameraObject.transform.position, desk);
                Assert.Greater(deskBoom, chaseBoom);
                camera.SetChaseTarget(eraser.transform);
                camera.Snap();

                camera.ToggleOverview();
                camera.Snap();
                Assert.AreEqual(OrbitCamera.Mode.TopDown, camera.View);
                Assert.Greater(AimError(cameraObject.transform, eraser.transform.position), 20f);
                Assert.Less(Planar(cameraObject.transform.position).magnitude, 0.3f);
                Assert.Greater(cameraObject.transform.position.y, 2f);
                Assert.Greater(Vector3.Dot(cameraObject.transform.forward, Vector3.down), 0.95f);

                camera.ToggleOverview();
                camera.Snap();
                Assert.AreEqual(OrbitCamera.Mode.Orbit, camera.View);
                Assert.Less(AimError(cameraObject.transform, eraser.transform.position), 1f);
                Assert.AreEqual(chaseBoom, Vector3.Distance(cameraObject.transform.position, eraser.transform.position), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(eraser);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void CloseUpFramesTheActiveEraserAndRestoresTheChase()
        {
            var cameraObject = new GameObject("view-cam");
            var chased = new GameObject("chased");
            var active = new GameObject("active");
            try
            {
                chased.transform.position = new Vector3(-1.4f, 0.8f, 0.4f);
                active.transform.position = new Vector3(0.9f, 0.8f, -0.7f);
                var camera = cameraObject.AddComponent<OrbitCamera>();
                camera.SetChaseTarget(chased.transform);
                camera.Snap();
                var chaseBoom = Vector3.Distance(cameraObject.transform.position, chased.transform.position);

                camera.ToggleCloseUp();
                camera.Snap();
                Assert.Less(AimError(cameraObject.transform, chased.transform.position), 1f);
                var chaseClose = Vector3.Distance(cameraObject.transform.position, chased.transform.position);
                Assert.Less(chaseClose, chaseBoom * 0.7f);
                Assert.Less(Vector3.Dot((cameraObject.transform.position - chased.transform.position).normalized, Vector3.up), 0.7f);

                camera.ToggleCloseUp();
                camera.Snap();
                camera.SetActiveEraser(active.transform);
                camera.ToggleCloseUp();
                camera.Snap();
                Assert.AreEqual(OrbitCamera.Mode.CloseUp, camera.View);
                Assert.Less(AimError(cameraObject.transform, active.transform.position), 1f);
                Assert.Greater(AimError(cameraObject.transform, chased.transform.position), 15f);
                Assert.Less(
                    Vector3.Distance(cameraObject.transform.position, active.transform.position),
                    chaseBoom * 0.7f);

                camera.ToggleOverview();
                camera.Snap();
                Assert.AreEqual(OrbitCamera.Mode.TopDown, camera.View);
                Assert.Greater(AimError(cameraObject.transform, active.transform.position), 15f);
                Assert.Greater(Vector3.Dot(cameraObject.transform.forward, Vector3.down), 0.95f);

                camera.ToggleCloseUp();
                camera.Snap();
                Assert.AreEqual(OrbitCamera.Mode.CloseUp, camera.View);
                Assert.Less(AimError(cameraObject.transform, active.transform.position), 1f);

                camera.ToggleCloseUp();
                camera.Snap();
                Assert.AreEqual(OrbitCamera.Mode.Orbit, camera.View);
                Assert.Less(AimError(cameraObject.transform, chased.transform.position), 1f);
                Assert.AreEqual(chaseBoom, Vector3.Distance(cameraObject.transform.position, chased.transform.position), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(active);
                Object.DestroyImmediate(chased);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void OverviewKeepsTheFallLeaveForTheOrbit()
        {
            var cameraObject = new GameObject("view-cam");
            var eraser = new GameObject("eraser");
            try
            {
                eraser.transform.position = new Vector3(1.2f, 0.8f, 0.5f);
                var camera = cameraObject.AddComponent<OrbitCamera>();
                camera.SetChaseTarget(eraser.transform);
                camera.Snap();
                var chaseBoom = Vector3.Distance(cameraObject.transform.position, eraser.transform.position);

                var exit = new Vector3(-0.2f, 0.15f, 1.5f);
                var now = Time.unscaledTime;
                camera.PlayFallLeave(exit, now, 0.45f);
                camera.Snap();
                Assert.Less(AimError(cameraObject.transform, exit), 1f);
                var glanceBoom = Vector3.Distance(cameraObject.transform.position, exit);
                Assert.Less(glanceBoom, chaseBoom);
                Assert.IsTrue(camera.FallLeaveActiveAt(now));

                camera.ToggleOverview();
                camera.Snap();
                Assert.Greater(AimError(cameraObject.transform, exit), 15f);
                Assert.Less(Planar(cameraObject.transform.position).magnitude, 0.3f);
                Assert.IsTrue(camera.FallLeaveActiveAt(now));
                Assert.AreEqual(exit, camera.FallLeaveFocus);

                camera.ToggleOverview();
                camera.Snap();
                Assert.Less(AimError(cameraObject.transform, exit), 1f);
                Assert.AreEqual(glanceBoom, Vector3.Distance(cameraObject.transform.position, exit), 0.001f);

                camera.ToggleCloseUp();
                camera.FrameDeskOverview();
                var desk = new Vector3(0f, 0.83f, 0f);
                Assert.AreEqual(OrbitCamera.Mode.Orbit, camera.View);
                Assert.IsFalse(camera.FallLeaveActiveAt(now));
                Assert.Less(AimError(cameraObject.transform, desk), 1f);
                Assert.Greater(Vector3.Distance(cameraObject.transform.position, desk), 1.5f);
                Assert.Less(Vector3.Dot(cameraObject.transform.forward, Vector3.down), 0.9f);
            }
            finally
            {
                Object.DestroyImmediate(eraser);
                Object.DestroyImmediate(cameraObject);
            }
        }

        static float AimError(Transform camera, Vector3 point)
        {
            return Vector3.Angle(camera.forward, point - camera.position);
        }

        static Vector3 Planar(Vector3 position)
        {
            return new Vector3(position.x, 0f, position.z);
        }
    }
}
