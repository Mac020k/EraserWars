using EraserWars.CameraRig;
using EraserWars.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class ClassroomSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Classroom_Casual.unity";

        // Keep in sync with EdgeSlowmoWatcher / Fallout deskTopY = 0.75.
        const float DeskCenterY = 0.75f;
        const float DeskThickness = 0.04f;
        const float FloorCenterY = 0f;
        const float FloorThickness = 0.08f;
        const float OverviewDistance = 4.55f;
        const float OverviewYaw = 28f;
        const float OverviewPitch = 36f;
        const float OverviewFocusBiasY = -0.34f;

        [MenuItem("EraserWars/Build Classroom Casual Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[EraserWars] Stop Play Mode before building Classroom_Casual.");
                return;
            }

            PrototypeLookBuilder.EnsureAssets(forceRefreshTextures: true);
            var deskMat = PrototypeLookBuilder.DeskMaterial;
            var legMat = PrototypeLookBuilder.DeskLegMaterial;
            var floorMat = PrototypeLookBuilder.FloorMaterial;
            var wallMat = PrototypeLookBuilder.WallMaterial;
            var eraserMat = PrototypeLookBuilder.EraserBaseMaterial;
            var chalkMat = PrototypeLookBuilder.ChalkboardMaterial;
            var trimMat = PrototypeLookBuilder.TrimMaterial;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var root = new GameObject("ClassroomCasual");
            var desks = new GameObject("Desks");
            desks.transform.SetParent(root.transform);

            // One joined arena top. Four corner legs with feet sunk into the floor.
            var floorTop = FloorCenterY + FloorThickness * 0.5f;
            var deskBottom = DeskCenterY - DeskThickness * 0.5f;
            const float legSink = 0.035f;
            var legHeight = deskBottom - (floorTop - legSink);
            var legCenterLocalY = ((floorTop - legSink + deskBottom) * 0.5f) - DeskCenterY;

            var deskRoot = new GameObject("DeskArena");
            deskRoot.transform.SetParent(desks.transform);
            deskRoot.transform.position = new Vector3(0f, DeskCenterY, 0f);

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Top";
            top.transform.SetParent(deskRoot.transform, false);
            top.transform.localPosition = Vector3.zero;
            top.transform.localScale = new Vector3(1.2f, DeskThickness, 0.8f);
            PrototypeLookBuilder.ApplySharedMaterial(top, deskMat);

            // Thin apron under the top so the silhouette reads as furniture, not a floating slab.
            var apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            apron.name = "Apron";
            apron.transform.SetParent(deskRoot.transform, false);
            apron.transform.localPosition = new Vector3(0f, -DeskThickness * 0.5f - 0.018f, 0f);
            apron.transform.localScale = new Vector3(1.16f, 0.03f, 0.76f);
            PrototypeLookBuilder.ApplySharedMaterial(apron, legMat);

            CreateSeam(deskRoot.transform, "Seam_X", new Vector3(0f, DeskThickness * 0.51f, 0f), new Vector3(0.01f, 0.002f, 0.8f), trimMat);
            CreateSeam(deskRoot.transform, "Seam_Z", new Vector3(0f, DeskThickness * 0.51f, 0f), new Vector3(1.2f, 0.002f, 0.01f), trimMat);

            var legOffsets = new[]
            {
                new Vector3(-0.52f, legCenterLocalY, -0.32f),
                new Vector3(0.52f, legCenterLocalY, -0.32f),
                new Vector3(-0.52f, legCenterLocalY, 0.32f),
                new Vector3(0.52f, legCenterLocalY, 0.32f),
            };
            foreach (var legOffset in legOffsets)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg";
                leg.transform.SetParent(deskRoot.transform, false);
                leg.transform.localPosition = legOffset;
                leg.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
                PrototypeLookBuilder.ApplySharedMaterial(leg, legMat);

                // Foot pad sits on / into the floor so contact never reads as a gap.
                var foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                foot.name = "Foot";
                foot.transform.SetParent(deskRoot.transform, false);
                var footLocalY = (floorTop - legSink * 0.35f) - DeskCenterY;
                foot.transform.localPosition = new Vector3(legOffset.x, footLocalY, legOffset.z);
                foot.transform.localScale = new Vector3(0.08f, 0.025f, 0.08f);
                PrototypeLookBuilder.ApplySharedMaterial(foot, trimMat);
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ClassroomFloor";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, FloorCenterY, 0f);
            floor.transform.localScale = new Vector3(7.2f, FloorThickness, 7.2f);
            PrototypeLookBuilder.ApplySharedMaterial(floor, floorMat);

            BuildRoomShell(root.transform, wallMat, chalkMat, trimMat);

            var kill = new GameObject("KillZone");
            kill.tag = "KillZone";
            kill.transform.SetParent(root.transform);
            kill.transform.position = new Vector3(0f, 0.12f, 0f);
            var killCol = kill.AddComponent<BoxCollider>();
            killCol.isTrigger = true;
            killCol.size = new Vector3(4f, 0.2f, 4f);

            var spawnOffsets = new[]
            {
                new Vector3(-0.45f, DeskCenterY, -0.28f),
                new Vector3(0.45f, DeskCenterY, -0.28f),
                new Vector3(-0.45f, DeskCenterY, 0.28f),
                new Vector3(0.45f, DeskCenterY, 0.28f),
            };
            var spawns = new GameObject("Spawns");
            spawns.transform.SetParent(root.transform);
            var spawnTransforms = new Transform[4];
            for (var i = 0; i < 4; i++)
            {
                var s = new GameObject($"Spawn_{i}");
                s.transform.SetParent(spawns.transform);
                s.transform.position = spawnOffsets[i] + new Vector3(0f, DeskThickness * 0.5f + 0.012f, 0f);
                spawnTransforms[i] = s.transform;
            }

            var deskFocus = new GameObject("DeskFocus");
            deskFocus.transform.SetParent(root.transform);
            deskFocus.transform.position = new Vector3(0f, DeskCenterY + DeskThickness * 0.5f, 0f);

            var eraser = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eraser.name = "EraserPrefab";
            eraser.transform.position = new Vector3(10f, 10f, 10f);
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

            var light = Object.FindAnyObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.96f, 0.88f);
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            }

            // Soft fill so the underside of the desk and leg/floor contact stay readable.
            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(root.transform, false);
            fillGo.transform.position = new Vector3(-1.2f, 1.6f, -1.4f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(0.75f, 0.82f, 0.95f);
            fill.intensity = 0.55f;
            fill.range = 6f;
            fill.shadows = LightShadows.None;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.fieldOfView = 48f;
                cam.nearClipPlane = 0.05f;
                cam.backgroundColor = new Color(0.42f, 0.48f, 0.54f);
                var orbit = cam.gameObject.GetComponent<OrbitCamera>()
                             ?? cam.gameObject.AddComponent<OrbitCamera>();
                var orbitSo = new SerializedObject(orbit);
                orbitSo.FindProperty("focus").objectReferenceValue = deskFocus.transform;
                orbitSo.FindProperty("distance").floatValue = OverviewDistance;
                orbitSo.FindProperty("yaw").floatValue = OverviewYaw;
                orbitSo.FindProperty("pitch").floatValue = OverviewPitch;
                orbitSo.ApplyModifiedPropertiesWithoutUndo();
                var lookAt = deskFocus.transform.position + new Vector3(0f, OverviewFocusBiasY, 0f);
                var rotation = Quaternion.Euler(OverviewPitch, OverviewYaw, 0f);
                cam.transform.position = lookAt + rotation * (Vector3.back * OverviewDistance);
                cam.transform.LookAt(lookAt);
            }

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

        static void BuildRoomShell(Transform parent, Material wallMat, Material chalkMat, Material trimMat)
        {
            var room = new GameObject("RoomShell");
            room.transform.SetParent(parent, false);

            CreateWall(room.transform, "Wall_Back", new Vector3(0f, 1.55f, 3.45f), new Vector3(7.4f, 3.1f, 0.1f), wallMat);
            CreateWall(room.transform, "Wall_Left", new Vector3(-3.55f, 1.55f, 0f), new Vector3(0.1f, 3.1f, 7.0f), wallMat);
            CreateWall(room.transform, "Wall_Right", new Vector3(3.55f, 1.55f, 0f), new Vector3(0.1f, 3.1f, 7.0f), wallMat);
            CreateWall(room.transform, "Ceiling", new Vector3(0f, 3.05f, 0f), new Vector3(7.4f, 0.08f, 7.0f), wallMat);

            // Chalkboard on the back wall — classroom silhouette without cluttering the desk.
            CreateWall(room.transform, "Chalkboard", new Vector3(0f, 1.55f, 3.38f), new Vector3(2.4f, 1.2f, 0.04f), chalkMat);
            CreateWall(room.transform, "ChalkFrame", new Vector3(0f, 1.55f, 3.36f), new Vector3(2.55f, 1.35f, 0.03f), trimMat);

            // Baseboards so wall/floor seams read as architecture, not floating geometry.
            CreateWall(room.transform, "Base_Back", new Vector3(0f, 0.09f, 3.39f), new Vector3(7.2f, 0.12f, 0.06f), trimMat);
            CreateWall(room.transform, "Base_Left", new Vector3(-3.49f, 0.09f, 0f), new Vector3(0.06f, 0.12f, 6.9f), trimMat);
            CreateWall(room.transform, "Base_Right", new Vector3(3.49f, 0.09f, 0f), new Vector3(0.06f, 0.12f, 6.9f), trimMat);
        }

        static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            PrototypeLookBuilder.ApplySharedMaterial(wall, mat);
            var col = wall.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }
        }

        static void CreateSeam(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material mat)
        {
            var seam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seam.name = name;
            seam.transform.SetParent(parent, false);
            seam.transform.localPosition = localPosition;
            seam.transform.localScale = localScale;
            PrototypeLookBuilder.ApplySharedMaterial(seam, mat);
            var col = seam.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }
        }
    }
}
