using EraserWars.CameraRig;
using EraserWars.Gameplay;
using EraserWars.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Editor
{
    public static class ClassroomSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Classroom_Casual.unity";
        const string PanelSettingsPath = "Assets/UI/PanelSettings.asset";
        const string StartMenuUxmlPath = "Assets/UI/StartMenu.uxml";
        const string MatchHudUxmlPath = "Assets/UI/MatchHud.uxml";

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
            UiIconBuilder.BuildAll();
            var deskMat = PrototypeLookBuilder.DeskMaterial;
            var legMat = PrototypeLookBuilder.DeskLegMaterial;
            var floorMat = PrototypeLookBuilder.FloorMaterial;
            var wallMat = PrototypeLookBuilder.WallMaterial;
            var eraserMat = PrototypeLookBuilder.EraserBaseMaterial;
            var chalkMat = PrototypeLookBuilder.ChalkboardMaterial;
            var trimMat = PrototypeLookBuilder.TrimMaterial;
            var corkMat = PrototypeLookBuilder.CorkMaterial;
            var yardMat = PrototypeLookBuilder.SchoolyardMaterial;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("ClassroomCasual");
            var desks = new GameObject("Desks");
            desks.transform.SetParent(root.transform);

            var floorTop = FloorCenterY + FloorThickness * 0.5f;
            var deskBottom = DeskCenterY - DeskThickness * 0.5f;
            const float legSink = 0.05f;
            var legBottom = floorTop - legSink;
            var legHeight = deskBottom - legBottom;
            var legCenterLocalY = ((legBottom + deskBottom) * 0.5f) - DeskCenterY;

            var deskRoot = new GameObject("DeskArena");
            deskRoot.transform.SetParent(desks.transform);
            deskRoot.transform.position = new Vector3(0f, DeskCenterY, 0f);

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Top";
            top.transform.SetParent(deskRoot.transform, false);
            top.transform.localScale = new Vector3(1.2f, DeskThickness, 0.8f);
            PrototypeLookBuilder.ApplySharedMaterial(top, deskMat);

            var apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            apron.name = "Apron";
            apron.transform.SetParent(deskRoot.transform, false);
            apron.transform.localPosition = new Vector3(0f, -DeskThickness * 0.5f - 0.018f, 0f);
            apron.transform.localScale = new Vector3(1.16f, 0.03f, 0.76f);
            PrototypeLookBuilder.ApplySharedMaterial(apron, legMat);

            CreateSeam(deskRoot.transform, "Seam_X", new Vector3(0f, DeskThickness * 0.51f, 0f), new Vector3(0.01f, 0.002f, 0.8f), trimMat);
            CreateSeam(deskRoot.transform, "Seam_Z", new Vector3(0f, DeskThickness * 0.51f, 0f), new Vector3(1.2f, 0.002f, 0.01f), trimMat);

            foreach (var xz in new[]
                     {
                         new Vector2(-0.52f, -0.32f),
                         new Vector2(0.52f, -0.32f),
                         new Vector2(-0.52f, 0.32f),
                         new Vector2(0.52f, 0.32f),
                     })
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg";
                leg.transform.SetParent(deskRoot.transform, false);
                leg.transform.localPosition = new Vector3(xz.x, legCenterLocalY, xz.y);
                leg.transform.localScale = new Vector3(0.045f, legHeight, 0.045f);
                PrototypeLookBuilder.ApplySharedMaterial(leg, legMat);

                var foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                foot.name = "Foot";
                foot.transform.SetParent(deskRoot.transform, false);
                foot.transform.localPosition = new Vector3(xz.x, (floorTop - legSink * 0.3f) - DeskCenterY, xz.y);
                foot.transform.localScale = new Vector3(0.08f, 0.02f, 0.08f);
                PrototypeLookBuilder.ApplySharedMaterial(foot, trimMat);
            }

            // Atmosphere desks (non-play) so the arena reads as a classroom, not a void arena.
            PlacePropDesk(desks.transform, new Vector3(-2.1f, DeskCenterY, 1.4f), deskMat, legMat, trimMat);
            PlacePropDesk(desks.transform, new Vector3(2.1f, DeskCenterY, 1.4f), deskMat, legMat, trimMat);
            PlacePropDesk(desks.transform, new Vector3(-2.1f, DeskCenterY, -1.5f), deskMat, legMat, trimMat);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ClassroomFloor";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, FloorCenterY, 0f);
            floor.transform.localScale = new Vector3(9f, FloorThickness, 8f);
            PrototypeLookBuilder.ApplySharedMaterial(floor, floorMat);

            BuildRoomShell(root.transform, wallMat, chalkMat, trimMat, corkMat, yardMat);

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
            new GameObject("Visual").transform.SetParent(eraser.transform, false);

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
            bootSo.FindProperty("autoStartHotseat").boolValue = false;
            bootSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(director);
            so.FindProperty("flickApplier").objectReferenceValue = flick;
            so.FindProperty("settleWatcher").objectReferenceValue = settle;
            so.FindProperty("eraserPrefab").objectReferenceValue = body;
            so.FindProperty("showDebug").boolValue = false;
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

            WireUi(root.transform, director);

            var light = Object.FindAnyObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.96f, 0.88f);
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(42f, -40f, 0f);
            }

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(root.transform, false);
            fillGo.transform.position = new Vector3(-1.4f, 1.7f, -1.6f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(0.75f, 0.84f, 0.95f);
            fill.intensity = 0.6f;
            fill.range = 7f;

            // Fluorescent strip over the arena (Heisei classroom cue).
            var fluo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fluo.name = "Fluorescent";
            fluo.transform.SetParent(root.transform, false);
            fluo.transform.position = new Vector3(0f, 2.7f, 0f);
            fluo.transform.localScale = new Vector3(2.4f, 0.05f, 0.2f);
            PrototypeLookBuilder.ApplySharedMaterial(fluo, wallMat);
            Object.DestroyImmediate(fluo.GetComponent<Collider>());

            var cam = Camera.main;
            if (cam != null)
            {
                cam.fieldOfView = 48f;
                cam.nearClipPlane = 0.05f;
                cam.backgroundColor = new Color(0.55f, 0.70f, 0.88f);
                var orbit = cam.gameObject.GetComponent<OrbitCamera>()
                             ?? cam.gameObject.AddComponent<OrbitCamera>();
                var occlusion = cam.gameObject.GetComponent<CameraOcclusionFader>()
                                ?? cam.gameObject.AddComponent<CameraOcclusionFader>();
                var orbitSo = new SerializedObject(orbit);
                orbitSo.FindProperty("focus").objectReferenceValue = deskFocus.transform;
                orbitSo.FindProperty("distance").floatValue = OverviewDistance;
                orbitSo.FindProperty("yaw").floatValue = OverviewYaw;
                orbitSo.FindProperty("pitch").floatValue = OverviewPitch;
                orbitSo.ApplyModifiedPropertiesWithoutUndo();
                var occSo = new SerializedObject(occlusion);
                occSo.FindProperty("focus").objectReferenceValue = deskFocus.transform;
                occSo.ApplyModifiedPropertiesWithoutUndo();

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
            Debug.Log($"Built {ScenePath} (Heisei classroom + StartMenu 6B + occlusion fade)");
        }

        static void WireUi(Transform root, MatchDirector director)
        {
            var panelSettings = EnsurePanelSettings();
            var startUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(StartMenuUxmlPath);
            var matchUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MatchHudUxmlPath);

            var startGo = new GameObject("StartMenu");
            startGo.transform.SetParent(root, false);
            var startDoc = startGo.AddComponent<UIDocument>();
            startDoc.visualTreeAsset = startUxml;
            startDoc.panelSettings = panelSettings;
            var start = startGo.AddComponent<StartMenuController>();
            var startSo = new SerializedObject(start);
            startSo.FindProperty("director").objectReferenceValue = director;
            startSo.FindProperty("document").objectReferenceValue = startDoc;
            startSo.FindProperty("brandIcon").objectReferenceValue = UiIconBuilder.BrandIcon;
            startSo.FindProperty("icon2P").objectReferenceValue = UiIconBuilder.Icon2P;
            startSo.FindProperty("icon4P").objectReferenceValue = UiIconBuilder.Icon4P;
            startSo.FindProperty("iconCollection").objectReferenceValue = UiIconBuilder.IconCollection;
            startSo.FindProperty("iconOptions").objectReferenceValue = UiIconBuilder.IconOptions;
            startSo.ApplyModifiedPropertiesWithoutUndo();

            var hudGo = new GameObject("MatchHud");
            hudGo.transform.SetParent(root, false);
            var hudDoc = hudGo.AddComponent<UIDocument>();
            hudDoc.visualTreeAsset = matchUxml;
            hudDoc.panelSettings = panelSettings;
            var hud = hudGo.AddComponent<MatchHudController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("director").objectReferenceValue = director;
            hudSo.FindProperty("document").objectReferenceValue = hudDoc;
            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static PanelSettings EnsurePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null)
            {
                return existing;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            return settings;
        }

        static void BuildRoomShell(
            Transform parent,
            Material wallMat,
            Material chalkMat,
            Material trimMat,
            Material corkMat,
            Material yardMat)
        {
            var room = new GameObject("RoomShell");
            room.transform.SetParent(parent, false);

            // Front = blackboard wall (+Z), windows on -X (schoolyard), corridor on +X.
            CreateOccludableWall(room.transform, "Wall_Front", new Vector3(0f, 1.55f, 3.6f), new Vector3(8.2f, 3.1f, 0.12f), wallMat);
            CreateOccludableWall(room.transform, "Wall_Back", new Vector3(0f, 1.55f, -3.6f), new Vector3(8.2f, 3.1f, 0.12f), wallMat);
            CreateOccludableWall(room.transform, "Wall_Corridor", new Vector3(3.9f, 1.55f, 0f), new Vector3(0.12f, 3.1f, 7.2f), wallMat);
            CreateOccludableWall(room.transform, "Ceiling", new Vector3(0f, 3.05f, 0f), new Vector3(8.2f, 0.08f, 7.2f), wallMat);

            // Window wall: three openings framed by wall segments + schoolyard backdrop.
            CreateOccludableWall(room.transform, "Wall_Window_PillarA", new Vector3(-3.9f, 1.55f, -2.4f), new Vector3(0.12f, 3.1f, 1.6f), wallMat);
            CreateOccludableWall(room.transform, "Wall_Window_PillarB", new Vector3(-3.9f, 1.55f, 0f), new Vector3(0.12f, 3.1f, 1.0f), wallMat);
            CreateOccludableWall(room.transform, "Wall_Window_PillarC", new Vector3(-3.9f, 1.55f, 2.4f), new Vector3(0.12f, 3.1f, 1.6f), wallMat);
            CreateOccludableWall(room.transform, "Wall_Window_Sill", new Vector3(-3.9f, 0.35f, 0f), new Vector3(0.14f, 0.7f, 7.2f), wallMat);
            CreateOccludableWall(room.transform, "Wall_Window_Header", new Vector3(-3.9f, 2.75f, 0f), new Vector3(0.14f, 0.6f, 7.2f), wallMat);

            var yard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            yard.name = "SchoolyardView";
            yard.transform.SetParent(room.transform, false);
            yard.transform.position = new Vector3(-4.3f, 1.55f, 0f);
            yard.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            yard.transform.localScale = new Vector3(7.2f, 2.8f, 1f);
            PrototypeLookBuilder.ApplySharedMaterial(yard, yardMat);
            Object.DestroyImmediate(yard.GetComponent<Collider>());

            CreateWall(room.transform, "Chalkboard", new Vector3(0f, 1.55f, 3.52f), new Vector3(3.2f, 1.35f, 0.04f), chalkMat);
            CreateWall(room.transform, "ChalkFrame", new Vector3(0f, 1.55f, 3.5f), new Vector3(3.4f, 1.5f, 0.03f), trimMat);
            CreateWall(room.transform, "ChalkLedge", new Vector3(0f, 0.82f, 3.48f), new Vector3(3.3f, 0.06f, 0.1f), trimMat);
            CreateWall(room.transform, "TimetableBoard", new Vector3(2.2f, 1.9f, 3.52f), new Vector3(0.9f, 0.7f, 0.03f), corkMat);

            CreateWall(room.transform, "Bulletin_Back", new Vector3(0f, 1.6f, -3.52f), new Vector3(2.8f, 1.2f, 0.04f), corkMat);
            PlacePaper(room.transform, "Paper_Red", new Vector3(-0.7f, 1.75f, -3.48f), new Color(0.95f, 0.35f, 0.35f));
            PlacePaper(room.transform, "Paper_Blue", new Vector3(0.1f, 1.55f, -3.48f), new Color(0.35f, 0.55f, 0.95f));
            PlacePaper(room.transform, "Paper_Yellow", new Vector3(0.8f, 1.8f, -3.48f), new Color(0.95f, 0.85f, 0.25f));

            CreateWall(room.transform, "Base_Front", new Vector3(0f, 0.09f, 3.52f), new Vector3(8f, 0.12f, 0.06f), trimMat);
            CreateWall(room.transform, "Base_Back", new Vector3(0f, 0.09f, -3.52f), new Vector3(8f, 0.12f, 0.06f), trimMat);
            CreateWall(room.transform, "Base_Corridor", new Vector3(3.82f, 0.09f, 0f), new Vector3(0.06f, 0.12f, 7f), trimMat);

            // Teacher desk + heater (classroom symbols, non-play).
            CreateWall(room.transform, "TeacherDesk", new Vector3(0f, 0.55f, 2.55f), new Vector3(1.0f, 0.7f, 0.45f), PrototypeLookBuilder.DeskMaterial);
            CreateWall(room.transform, "Heater", new Vector3(3.55f, 0.45f, -2.2f), new Vector3(0.25f, 0.7f, 0.9f), trimMat);
        }

        static void PlacePropDesk(Transform parent, Vector3 position, Material deskMat, Material legMat, Material trimMat)
        {
            var desk = new GameObject("PropDesk");
            desk.transform.SetParent(parent, false);
            desk.transform.position = position;
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Top";
            top.transform.SetParent(desk.transform, false);
            top.transform.localScale = new Vector3(0.6f, 0.04f, 0.45f);
            PrototypeLookBuilder.ApplySharedMaterial(top, deskMat);
            Object.DestroyImmediate(top.GetComponent<Collider>());
            foreach (var xz in new[] { new Vector2(-0.25f, -0.18f), new Vector2(0.25f, -0.18f), new Vector2(-0.25f, 0.18f), new Vector2(0.25f, 0.18f) })
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(desk.transform, false);
                leg.transform.localPosition = new Vector3(xz.x, -0.38f, xz.y);
                leg.transform.localScale = new Vector3(0.04f, 0.72f, 0.04f);
                PrototypeLookBuilder.ApplySharedMaterial(leg, legMat);
                Object.DestroyImmediate(leg.GetComponent<Collider>());
            }
        }

        static void PlacePaper(Transform parent, string name, Vector3 position, Color color)
        {
            var paper = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paper.name = name;
            paper.transform.SetParent(parent, false);
            paper.transform.position = position;
            paper.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            paper.transform.localScale = new Vector3(0.35f, 0.45f, 1f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            paper.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(paper.GetComponent<Collider>());
        }

        static void CreateOccludableWall(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var wall = CreateWall(parent, name, position, scale, mat);
            wall.AddComponent<OccludableFade>();
        }

        static GameObject CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            PrototypeLookBuilder.ApplySharedMaterial(wall, mat);
            var col = wall.GetComponent<Collider>();
            if (col != null && (name.StartsWith("Chalk") || name.StartsWith("Bulletin") || name.StartsWith("Paper") || name.StartsWith("Timetable") || name.StartsWith("Base") || name.StartsWith("Teacher") || name.StartsWith("Heater") || name.StartsWith("Fluorescent")))
            {
                Object.DestroyImmediate(col);
            }

            return wall;
        }

        static void CreateSeam(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material mat)
        {
            var seam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seam.name = name;
            seam.transform.SetParent(parent, false);
            seam.transform.localPosition = localPosition;
            seam.transform.localScale = localScale;
            PrototypeLookBuilder.ApplySharedMaterial(seam, mat);
            Object.DestroyImmediate(seam.GetComponent<Collider>());
        }
    }
}
