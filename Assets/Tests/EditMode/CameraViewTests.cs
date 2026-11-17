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

        [Test]
        public void HomeIdleYawsEightDegreesPerSecondAndThenHolds()
        {
            var cameraObject = new GameObject("view-cam");
            try
            {
                var camera = cameraObject.AddComponent<OrbitCamera>();
                camera.FrameDeskOverview();
                camera.SetHomeIdle(true);
                var before = PlanarAngle(cameraObject.transform.position);
                camera.Tick(0f, 1f, 1f);
                Assert.AreEqual(8f, OrbitCamera.HomeIdleYawDegreesPerSecond, 0.001f);
                Assert.AreEqual(8f, Mathf.DeltaAngle(before, PlanarAngle(cameraObject.transform.position)), 0.2f);

                camera.SetHomeIdle(false);
                var held = PlanarAngle(cameraObject.transform.position);
                camera.Tick(1f, 1f, 1f);
                Assert.AreEqual(0f, Mathf.DeltaAngle(held, PlanarAngle(cameraObject.transform.position)), 0.05f);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void HomeIdleStaysPutInMatchViewsAndWhileChasing()
        {
            var cameraObject = new GameObject("view-cam");
            var eraser = new GameObject("eraser");
            try
            {
                eraser.transform.position = new Vector3(0.4f, 0.8f, 0.2f);
                var camera = cameraObject.AddComponent<OrbitCamera>();
                camera.FrameDeskOverview();
                camera.ToggleOverview();
                camera.Snap();
                camera.SetHomeIdle(true);
                var topDown = cameraObject.transform.position;
                camera.Tick(0f, 1f, 1f);
                Assert.Less(Vector3.Distance(topDown, cameraObject.transform.position), 0.001f);

                camera.ToggleOverview();
                camera.ToggleCloseUp();
                camera.Snap();
                var closeUp = cameraObject.transform.position;
                camera.Tick(1f, 1f, 1f);
                Assert.Less(Vector3.Distance(closeUp, cameraObject.transform.position), 0.001f);

                camera.ToggleCloseUp();
                camera.SetChaseTarget(eraser.transform);
                camera.Snap();
                var chase = cameraObject.transform.position;
                camera.Tick(2f, 1f, 0f);
                Assert.Less(Vector3.Distance(chase, cameraObject.transform.position), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(eraser);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void DestinationPointerPolicyIgnoresDragAndWheel()
        {
            var cameraObject = new GameObject("view-cam");
            try
            {
                var camera = cameraObject.AddComponent<OrbitCamera>();
                camera.FrameDeskOverview();
                var parked = cameraObject.transform.position;
                camera.SetMenuPointerPolicy(blockWheel: true, blockDrag: true);
                camera.ApplyPointerOrbit(true, new Vector2(90f, 24f), 240f);
                camera.Snap();
                Assert.Less(Vector3.Distance(parked, cameraObject.transform.position), 0.001f);

                camera.SetMenuPointerPolicy(blockWheel: true, blockDrag: false);
                camera.ApplyPointerOrbit(true, new Vector2(40f, 0f), 240f);
                camera.Snap();
                Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(PlanarAngle(parked), PlanarAngle(cameraObject.transform.position))), 1f);
                var dragged = Vector3.Distance(cameraObject.transform.position, Vector3.zero);
                camera.ApplyPointerOrbit(false, Vector2.zero, 240f);
                camera.Snap();
                Assert.AreEqual(dragged, Vector3.Distance(cameraObject.transform.position, Vector3.zero), 0.001f);

                camera.SetMenuPointerPolicy(blockWheel: false, blockDrag: false);
                camera.ApplyPointerOrbit(false, Vector2.zero, 240f);
                camera.Snap();
                Assert.Greater(
                    Mathf.Abs(dragged - Vector3.Distance(cameraObject.transform.position, Vector3.zero)),
                    0.02f);
            }
            finally
            {
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

        static float PlanarAngle(Vector3 position)
        {
            var flat = Planar(position);
            return Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
        }
    }
}
