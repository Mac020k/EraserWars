using System.Collections;
using System.Reflection;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.PlayMode
{
    public sealed class ClassroomKnockoffPlayModeTests
    {
        const float DeskCenterY = 0.75f;
        const float DeskThickness = 0.04f;
        const float FellBelowY = 0.70f;
        const float TimeoutSeconds = 8f;
        const float ScriptedImpulseX = 0.2f;
        const float GateSpeedCap = 20f;

        GameObject root;
        EraserBody striker;
        EraserBody target;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("KnockoffRig");
            CreateDesk();
            CreateFloor();
            CreateKillZone();
            striker = CreateEraser("EraserA", new Vector3(0f, 0.84f, 0f), 0);
            target = CreateEraser("EraserB", new Vector3(0.35f, 0.84f, 0f), 1);

            for (var step = 0; step < 45; step++)
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
        public IEnumerator KnownImpulseFromAKnocksBOffTheDeskWithinEightSeconds()
        {
            Assert.Greater(striker.Body.position.y, FellBelowY, "A left the desk before the flick.");
            Assert.Greater(target.Body.position.y, FellBelowY, "B left the desk before the flick.");

            var fell = false;
            var detector = target.GetComponent<FalloutDetector>();
            detector.Fell += _ => fell = true;

            // ApplyFlick clamps linear speed to maxSpeed (trackpad default 3.6).
            // This gate raises only the striker cap so the scripted impulse is the speed that leaves A.
            // FlickApplier.jMax stays 0.24 for trackpad play.
            SetSpeedCap(striker, GateSpeedCap);
            var impulse = new Vector3(ScriptedImpulseX, 0f, 0f);
            striker.ApplyFlick(impulse, striker.Body.worldCenterOfMass);

            var elapsed = 0f;
            while (elapsed < TimeoutSeconds)
            {
                var offDesk = target.Body.position.y < FellBelowY;
                var gone = !target.gameObject.activeInHierarchy;
                if (fell || gone || offDesk)
                {
                    fell = true;
                    break;
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.IsTrue(
                fell,
                $"B stayed on the desk for {elapsed:0.00}s at y={target.Body.position.y:0.000}. "
                + $"Scripted impulse was ({ScriptedImpulseX}, 0, 0) with a test-only speed cap of {GateSpeedCap}. "
                + "Trackpad play still uses maxSpeed 3.6 and jMax 0.24.");
        }

        void CreateDesk()
        {
            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "DeskTop";
            desk.transform.SetParent(root.transform, false);
            desk.transform.localPosition = new Vector3(0f, DeskCenterY, 0f);
            desk.transform.localScale = new Vector3(1.2f, DeskThickness, 0.8f);
        }

        void CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ClassroomFloor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(4f, 0.08f, 4f);
        }

        void CreateKillZone()
        {
            var kill = new GameObject("KillZone");
            kill.tag = "KillZone";
            kill.transform.SetParent(root.transform, false);
            kill.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            var collider = kill.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(4f, 0.2f, 4f);
        }

        EraserBody CreateEraser(string name, Vector3 position, int playerIndex)
        {
            var eraser = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eraser.name = name;
            eraser.transform.SetParent(root.transform, false);
            eraser.transform.position = position;
            eraser.transform.localScale = new Vector3(0.06f, 0.018f, 0.028f);
            eraser.AddComponent<Rigidbody>();
            var body = eraser.AddComponent<EraserBody>();
            body.PlayerIndex = playerIndex;
            eraser.AddComponent<FalloutDetector>();
            return body;
        }

        static void SetSpeedCap(EraserBody eraser, float maxSpeed)
        {
            var field = typeof(EraserBody).GetField(
                "maxSpeed",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, "EraserBody.maxSpeed was renamed. The gate sets it so ApplyFlick keeps the scripted impulse.");
            field.SetValue(eraser, maxSpeed);
        }
    }
}
