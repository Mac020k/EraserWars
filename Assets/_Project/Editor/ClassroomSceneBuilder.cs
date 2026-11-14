using Keshipin.CameraRig;
using Keshipin.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Keshipin.Editor
{
    public static class ClassroomSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Classroom_Casual.unity";

        [MenuItem("Keshipin/Build Classroom Casual Scene")]
        public static void Build()
        {
            PrototypeLookBuilder.EnsureAssets();
            var deskMat = PrototypeLookBuilder.DeskMaterial;
            var legMat = PrototypeLookBuilder.DeskLegMaterial;
            var floorMat = PrototypeLookBuilder.FloorMaterial;
            var eraserMat = PrototypeLookBuilder.EraserBaseMaterial;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var root = new GameObject("ClassroomCasual");
            var desks = new GameObject("Desks");
            desks.transform.SetParent(root.transform);
            var deskSize = new Vector3(0.6f, 0.04f, 0.4f);
            var offsets = new[]
            {
                new Vector3(-0.3f, 0.75f, -0.2f),
                new Vector3(0.3f, 0.75f, -0.2f),
                new Vector3(-0.3f, 0.75f, 0.2f),
                new Vector3(0.3f, 0.75f, 0.2f),
            };

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var deskRoot = new GameObject($"Desk_{i}");
                deskRoot.transform.SetParent(desks.transform);
                deskRoot.transform.position = offset;

                var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                top.name = "Top";
                top.transform.SetParent(deskRoot.transform, false);
                top.transform.localPosition = Vector3.zero;
                top.transform.localScale = deskSize;
                PrototypeLookBuilder.ApplySharedMaterial(top, deskMat);

                var legOffsets = new[]
                {
                    new Vector3(-0.22f, -0.28f, -0.14f),
                    new Vector3(0.22f, -0.28f, -0.14f),
                    new Vector3(-0.22f, -0.28f, 0.14f),
                    new Vector3(0.22f, -0.28f, 0.14f),
                };
                foreach (var legOffset in legOffsets)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = "Leg";
                    leg.transform.SetParent(deskRoot.transform, false);
                    leg.transform.localPosition = legOffset;
                    leg.transform.localScale = new Vector3(0.035f, 0.52f, 0.035f);
                    PrototypeLookBuilder.ApplySharedMaterial(leg, legMat);
                }
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ClassroomFloor";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(4f, 0.08f, 4f);
            PrototypeLookBuilder.ApplySharedMaterial(floor, floorMat);

            var kill = new GameObject("KillZone");
            kill.tag = "KillZone";
            kill.transform.SetParent(root.transform);
            kill.transform.position = new Vector3(0f, 0.12f, 0f);
            var killCol = kill.AddComponent<BoxCollider>();
            killCol.isTrigger = true;
            killCol.size = new Vector3(4f, 0.2f, 4f);

            var spawns = new GameObject("Spawns");
            spawns.transform.SetParent(root.transform);
            var spawnTransforms = new Transform[4];
            for (var i = 0; i < 4; i++)
            {
                var s = new GameObject($"Spawn_{i}");
                s.transform.SetParent(spawns.transform);
                s.transform.position = offsets[i] + Vector3.up * 0.04f;
                spawnTransforms[i] = s.transform;
            }

            var eraser = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eraser.name = "EraserPrefab";
            eraser.transform.position = new Vector3(10f, 10f, 10f);
            // Slightly larger than real 10cm scale so the prototype is readable.
            eraser.transform.localScale = new Vector3(0.06f, 0.018f, 0.028f);
            var rb = eraser.AddComponent<Rigidbody>();
            rb.mass = 0.02f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var body = eraser.AddComponent<EraserBody>();
            eraser.AddComponent<FalloutDetector>();
            PrototypeLookBuilder.ApplySharedMaterial(eraser, eraserMat);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(eraser.transform, false);

            var directorGo = new GameObject("MatchDirector");
            directorGo.transform.SetParent(root.transform);
            var director = directorGo.AddComponent<MatchDirector>();
            var flick = directorGo.AddComponent<FlickApplier>();
            var settle = directorGo.AddComponent<SettleWatcher>();
            directorGo.AddComponent<HitstopController>();
            directorGo.AddComponent<EdgeSlowmoWatcher>();
            var input = directorGo.AddComponent<HotseatFlickInput>();
            directorGo.AddComponent<PhysicsScaleProbe>();
            var bootstrap = directorGo.AddComponent<ClassroomBootstrap>();
            var bootSo = new SerializedObject(bootstrap);
            bootSo.FindProperty("director").objectReferenceValue = director;
            bootSo.FindProperty("players").intValue = 2;
            bootSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(director);
            so.FindProperty("flickApplier").objectReferenceValue = flick;
            so.FindProperty("settleWatcher").objectReferenceValue = settle;
            so.FindProperty("eraserPrefab").objectReferenceValue = body;
            so.FindProperty("showDebug").boolValue = true;
            var spawnsProp = so.FindProperty("spawnPoints");
            spawnsProp.arraySize = 4;
            for (var i = 0; i < 4; i++)
            {
                spawnsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnTransforms[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            var inputSo = new SerializedObject(input);
            inputSo.FindProperty("director").objectReferenceValue = director;
            inputSo.FindProperty("worldCamera").objectReferenceValue = Camera.main;
            inputSo.ApplyModifiedPropertiesWithoutUndo();

            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 1.85f, -1.55f);
                cam.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
                var orbit = cam.gameObject.GetComponent<OrbitCamera>()
                             ?? cam.gameObject.AddComponent<OrbitCamera>();
                var orbitSo = new SerializedObject(orbit);
                orbitSo.FindProperty("focus").objectReferenceValue = root.transform;
                orbitSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Keep the prefab root active. Saving while inactive makes every
            // Instantiate start disabled, so erasers never render or receive hits.
            PrefabUtility.SaveAsPrefabAsset(eraser, "Assets/Prefabs/Eraser.prefab");
            Object.DestroyImmediate(eraser);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Eraser.prefab");
            so = new SerializedObject(director);
            so.FindProperty("eraserPrefab").objectReferenceValue = prefab.GetComponent<EraserBody>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {ScenePath}");
        }
    }
}
