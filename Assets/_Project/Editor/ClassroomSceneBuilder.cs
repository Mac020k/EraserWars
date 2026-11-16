using System.Collections.Generic;
using EraserWars.CameraRig;
using EraserWars.Cards;
using EraserWars.Gameplay;
using EraserWars.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
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
        const float OverviewDistance = 1.85f;
        const float OverviewYaw = -22f;
        const float OverviewPitch = 54f;
        const float OverviewFocusBiasY = 0.06f;

        // Interior is 7 (X) by 9 (Z) by 3 (Y): chalkboard wall is the short face, room runs deeper.
        const float RoomHalfX = 3.5f;
        const float RoomHalfZ = 4.5f;
        const float RoomHeight = 3f;
        const float ShellT = 0.12f;
        const float OuterX = RoomHalfX + ShellT;
        const float OuterZ = RoomHalfZ + ShellT;
        const float FloorTop = FloorCenterY + FloorThickness * 0.5f;
        const float CeilingBottom = FloorTop + RoomHeight;
        const float ShellTop = CeilingBottom + 0.08f;
        const float CorridorWidth = 2.2f;
        const float CorridorHalfLength = 4.5f;

        const int ArenaDeskCount = 4;
        const float ArenaDeskWidth = 0.6f;
        const float ArenaDeskDepth = 0.4f;

        static readonly Dictionary<Color, Material> ColorMaterials = new();

        [MenuItem("EraserWars/Build Classroom Casual Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[EraserWars] Stop Play Mode before building Classroom_Casual.");
                return;
            }

            ColorMaterials.Clear();
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

            // Four school desks in a 2×2 cluster. Shared collider so erasers never catch on a joint.
            var playSurface = new GameObject("PlaySurface");
            playSurface.transform.SetParent(deskRoot.transform, false);
            var playCol = playSurface.AddComponent<BoxCollider>();
            playCol.size = new Vector3(ArenaDeskWidth * 2f, DeskThickness, ArenaDeskDepth * 2f);

            var arenaOffsets = new[]
            {
                new Vector3(-ArenaDeskWidth * 0.5f, 0f, -ArenaDeskDepth * 0.5f),
                new Vector3(ArenaDeskWidth * 0.5f, 0f, -ArenaDeskDepth * 0.5f),
                new Vector3(-ArenaDeskWidth * 0.5f, 0f, ArenaDeskDepth * 0.5f),
                new Vector3(ArenaDeskWidth * 0.5f, 0f, ArenaDeskDepth * 0.5f),
            };
            for (var i = 0; i < ArenaDeskCount; i++)
            {
                var desk = new GameObject($"Desk_{i}");
                desk.transform.SetParent(deskRoot.transform, false);
                desk.transform.localPosition = arenaOffsets[i];
                BuildArenaDesk(desk.transform, deskMat, legMat, trimMat, legHeight, legCenterLocalY, floorTop - legSink * 0.3f - DeskCenterY);
            }

            // Continuous top hides the 2×2 joint cross so the play surface reads as one table.
            CreateDecorLocal(
                deskRoot.transform,
                "PlayTop",
                new Vector3(0f, DeskThickness * 0.52f, 0f),
                new Vector3(ArenaDeskWidth * 2f + 0.01f, 0.008f, ArenaDeskDepth * 2f + 0.01f),
                deskMat);

            BuildPropDeskGrid(desks.transform, deskMat, legMat);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ClassroomFloor";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, FloorCenterY, 0f);
            floor.transform.localScale = new Vector3(OuterX * 2f, FloorThickness, OuterZ * 2f);
            PrototypeLookBuilder.ApplySharedMaterial(floor, floorMat);

            BuildRoomShell(root.transform, wallMat, chalkMat, trimMat, corkMat, yardMat);

            // Wide enough to catch erasers that tumble off either long edge or either end of the row.
            var kill = new GameObject("KillZone");
            kill.tag = "KillZone";
            kill.transform.SetParent(root.transform);
            kill.transform.position = new Vector3(0f, 0.12f, 0f);
            var killCol = kill.AddComponent<BoxCollider>();
            killCol.isTrigger = true;
            killCol.size = new Vector3(ArenaDeskWidth * 2f + 2.4f, 0.2f, ArenaDeskDepth * 2f + 2.4f);

            // 2P matches use spawns 0 and 1 on opposite corners of the 2×2 cluster.
            var spawnOffsets = new[]
            {
                new Vector3(-ArenaDeskWidth * 0.5f, DeskCenterY, -ArenaDeskDepth * 0.5f),
                new Vector3(ArenaDeskWidth * 0.5f, DeskCenterY, ArenaDeskDepth * 0.5f),
                new Vector3(-ArenaDeskWidth * 0.5f, DeskCenterY, ArenaDeskDepth * 0.5f),
                new Vector3(ArenaDeskWidth * 0.5f, DeskCenterY, -ArenaDeskDepth * 0.5f),
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
            var edgeSlowmo = directorGo.AddComponent<EdgeSlowmoWatcher>();
            var edgeSo = new SerializedObject(edgeSlowmo);
            edgeSo.FindProperty("deskHalfExtents").vector2Value = new Vector2(
                ArenaDeskWidth,
                ArenaDeskDepth);
            edgeSo.ApplyModifiedPropertiesWithoutUndo();
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

            WireUi(root.transform, director);

            var light = Object.FindAnyObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.97f, 0.90f);
                light.intensity = 1.55f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.78f, 0.74f, 0.66f);

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(root.transform, false);
            fillGo.transform.position = new Vector3(0.6f, 2.5f, -0.8f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(0.95f, 0.95f, 0.98f);
            fill.intensity = 0.85f;
            fill.range = 12f;

            // Sun through the yard windows: light sits outside and shines into the room.
            var windowGo = new GameObject("WindowLight");
            windowGo.transform.SetParent(root.transform, false);
            windowGo.transform.position = new Vector3(-RoomHalfX - 1.6f, 2.15f, 0.4f);
            var windowLight = windowGo.AddComponent<Light>();
            windowLight.type = LightType.Spot;
            windowLight.color = new Color(1f, 0.92f, 0.75f);
            windowLight.intensity = 3.2f;
            windowLight.range = 14f;
            windowLight.spotAngle = 78f;
            windowLight.innerSpotAngle = 42f;
            windowLight.shadows = LightShadows.Soft;
            windowGo.transform.rotation = Quaternion.LookRotation(new Vector3(1f, -0.18f, -0.05f), Vector3.up);

            // Fluorescent fixtures + real lights parent under CeilingGroup (unscaled) for occlusion.
            var ceilingGroup = root.transform.Find("RoomShell/CeilingGroup");
            var fluoParent = ceilingGroup != null ? ceilingGroup : root.transform;
            foreach (var fz in new[] { -2.4f, 0f, 2.4f })
            {
                foreach (var fx in new[] { -1.6f, 1.6f })
                {
                    CreateDecor(fluoParent, "Fluorescent", new Vector3(fx, CeilingBottom - 0.04f, fz), new Vector3(1.35f, 0.06f, 0.22f), wallMat);
                    var lampGo = new GameObject("FluorescentLight");
                    lampGo.transform.SetParent(fluoParent, false);
                    lampGo.transform.position = new Vector3(fx, CeilingBottom - 0.12f, fz);
                    var lamp = lampGo.AddComponent<Light>();
                    lamp.type = LightType.Point;
                    lamp.color = new Color(1f, 0.98f, 0.92f);
                    lamp.intensity = 1.15f;
                    lamp.range = 6.5f;
                    lamp.shadows = LightShadows.None;
                }
            }

            // Corridor fluorescent strip lights.
            var corridorParent = root.transform.Find("RoomShell");
            if (corridorParent != null)
            {
                foreach (var zc in new[] { -2.8f, 0f, 2.8f })
                {
                    var cx = OuterX + CorridorWidth * 0.5f;
                    CreateDecor(corridorParent, "CorrFluorescent", new Vector3(cx, CeilingBottom - 0.05f, zc), new Vector3(0.22f, 0.05f, 1.1f), wallMat);
                    var corrLampGo = new GameObject("CorridorLight");
                    corrLampGo.transform.SetParent(corridorParent, false);
                    corrLampGo.transform.position = new Vector3(cx, CeilingBottom - 0.15f, zc);
                    var corrLamp = corrLampGo.AddComponent<Light>();
                    corrLamp.type = LightType.Point;
                    corrLamp.color = new Color(0.95f, 0.95f, 0.90f);
                    corrLamp.intensity = 1.35f;
                    corrLamp.range = 5.5f;
                    corrLamp.shadows = LightShadows.None;
                }
            }

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
                orbitSo.FindProperty("maxDistance").floatValue = 7f;
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
            GameViewAspectHelper.TrySetFreeAspect();
            Debug.Log($"Built {ScenePath} (Heisei classroom + StartMenu 6B + occlusion fade). Game view → Free Aspect.");
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
            startDoc.sortingOrder = 100;
            var start = startGo.AddComponent<StartMenuController>();
            var startSo = new SerializedObject(start);
            startSo.FindProperty("director").objectReferenceValue = director;
            startSo.FindProperty("document").objectReferenceValue = startDoc;
            startSo.FindProperty("contentCatalog").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<ContentCatalog>(ContentCatalogBuilder.CatalogPath);
            startSo.FindProperty("brandIcon").objectReferenceValue = UiIconBuilder.BrandIcon;
            startSo.FindProperty("icon2P").objectReferenceValue = UiIconBuilder.Icon2P;
            startSo.FindProperty("icon4P").objectReferenceValue = UiIconBuilder.Icon4P;
            startSo.FindProperty("iconDeck").objectReferenceValue = UiIconBuilder.IconDeck;
            startSo.FindProperty("iconCollection").objectReferenceValue = UiIconBuilder.IconCollection;
            startSo.FindProperty("iconOptions").objectReferenceValue = UiIconBuilder.IconOptions;
            startSo.ApplyModifiedPropertiesWithoutUndo();

            var hudGo = new GameObject("MatchHud");
            hudGo.transform.SetParent(root, false);
            var hudDoc = hudGo.AddComponent<UIDocument>();
            hudDoc.visualTreeAsset = matchUxml;
            hudDoc.panelSettings = panelSettings;
            hudDoc.sortingOrder = 50;
            var hud = hudGo.AddComponent<MatchHudController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("director").objectReferenceValue = director;
            hudSo.FindProperty("document").objectReferenceValue = hudDoc;
            hudSo.FindProperty("startMenu").objectReferenceValue = start;
            hudSo.FindProperty("iconStock").objectReferenceValue = UiIconBuilder.IconStock;
            hudSo.FindProperty("iconPass").objectReferenceValue = UiIconBuilder.IconPass;
            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static PanelSettings EnsurePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null)
            {
                existing.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                // Expand keeps the whole layout inside the panel for Free Aspect / Full HD / 4K.
                existing.screenMatchMode = PanelScreenMatchMode.Expand;
                existing.match = 0.5f;
                existing.referenceResolution = new Vector2Int(1920, 1080);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.screenMatchMode = PanelScreenMatchMode.Expand;
            settings.match = 0.5f;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            return settings;
        }

        static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        static void BuildArenaDesk(
            Transform desk,
            Material deskMat,
            Material legMat,
            Material trimMat,
            float legHeight,
            float legCenterLocalY,
            float footLocalY)
        {
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Top";
            top.transform.SetParent(desk, false);
            top.transform.localScale = new Vector3(ArenaDeskWidth, DeskThickness, ArenaDeskDepth);
            PrototypeLookBuilder.ApplySharedMaterial(top, deskMat);
            Object.DestroyImmediate(top.GetComponent<Collider>());

            CreateDecorLocal(desk, "Apron", new Vector3(0f, -DeskThickness * 0.5f - 0.018f, 0f), new Vector3(ArenaDeskWidth - 0.04f, 0.03f, ArenaDeskDepth - 0.04f), legMat);
            CreateDecorLocal(desk, "Tray", new Vector3(0f, -0.11f, 0.01f), new Vector3(ArenaDeskWidth - 0.1f, 0.1f, ArenaDeskDepth - 0.08f), legMat);

            var legX = ArenaDeskWidth * 0.5f - 0.04f;
            var legZ = ArenaDeskDepth * 0.5f - 0.04f;
            foreach (var sx in new[] { -1f, 1f })
            {
                foreach (var sz in new[] { -1f, 1f })
                {
                    CreateDecorLocal(desk, "Leg", new Vector3(sx * legX, legCenterLocalY, sz * legZ), new Vector3(0.035f, legHeight, 0.035f), legMat);
                    CreateDecorLocal(desk, "Foot", new Vector3(sx * legX, footLocalY, sz * legZ), new Vector3(0.06f, 0.02f, 0.06f), trimMat);
                }
            }
        }

        static GameObject ShellBox(
            Transform parent,
            string name,
            Vector3 min,
            Vector3 max,
            Material mat,
            bool castShadow = true,
            bool occludable = true)
        {
            var wall = CreateWall(parent, name, (min + max) * 0.5f, max - min, mat);
            if (occludable)
            {
                wall.AddComponent<OccludableFade>();
            }

            if (!castShadow)
            {
                wall.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            return wall;
        }

        static GameObject Box(Transform parent, string name, Vector3 min, Vector3 max, Material mat)
        {
            return CreateDecor(parent, name, (min + max) * 0.5f, max - min, mat);
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

            // Front = chalkboard wall (+Z), windows on -X, corridor on +X, cleaning locker on -Z.
            // Occlusion groups stay unscaled; scaled wall meshes are children so props keep correct size.
            var frontGroup = CreateOcclusionGroup(
                room.transform,
                "Wall_FrontGroup",
                new Vector3(0f, 1.5f, RoomHalfZ),
                Vector3.back);
            ShellBox(frontGroup.transform, "Wall_Front", V(-OuterX, 0f, RoomHalfZ), V(OuterX, ShellTop, OuterZ), wallMat, occludable: false);

            var backGroup = CreateOcclusionGroup(
                room.transform,
                "Wall_BackGroup",
                new Vector3(0f, 1.5f, -RoomHalfZ),
                Vector3.forward);
            ShellBox(backGroup.transform, "Wall_Back", V(-OuterX, 0f, -OuterZ), V(OuterX, ShellTop, -RoomHalfZ), wallMat, false, occludable: false);

            var ceilingGroup = CreateOcclusionGroup(
                room.transform,
                "CeilingGroup",
                new Vector3(0f, CeilingBottom, 0f),
                Vector3.down);
            ShellBox(ceilingGroup.transform, "Ceiling", V(-OuterX, CeilingBottom, -OuterZ), V(OuterX, ShellTop, OuterZ), wallMat, false, occludable: false);

            var pillarMat = ColorMaterial(new Color(0.90f, 0.84f, 0.70f));
            var frameMat = ColorMaterial(new Color(0.80f, 0.82f, 0.84f));

            BuildWindowWall(room.transform, wallMat, pillarMat, frameMat, yardMat);
            BuildCorridorWall(room.transform, wallMat, frameMat);
            BuildCorridorExterior(room.transform, wallMat);
            BuildChalkboardWall(frontGroup.transform, chalkMat, trimMat);
            BuildBackWall(backGroup.transform, corkMat);

            CreateWall(room.transform, "Base_Front", V(0f, 0.09f, RoomHalfZ - 0.03f), V(RoomHalfX * 2f, 0.12f, 0.06f), trimMat);
            CreateWall(room.transform, "Base_Back", V(0f, 0.09f, -RoomHalfZ + 0.03f), V(RoomHalfX * 2f, 0.12f, 0.06f), trimMat);
            CreateWall(room.transform, "Base_Window", V(-RoomHalfX + 0.03f, 0.09f, 0f), V(0.06f, 0.12f, RoomHalfZ * 2f), trimMat);

            CreateWall(room.transform, "TeacherDesk", V(1.4f, FloorTop + 0.38f, RoomHalfZ - 1.5f), V(1.0f, 0.76f, 0.5f), PrototypeLookBuilder.DeskMaterial);
            CreateWall(room.transform, "Heater", V(RoomHalfX - 0.15f, FloorTop + 0.35f, -1.2f), V(0.25f, 0.7f, 0.9f), trimMat);

            var clockMat = ColorMaterial(new Color(0.96f, 0.96f, 0.94f));
            CreateDecor(frontGroup.transform, "Clock_Rim", V(2.2f, 2.65f, RoomHalfZ - 0.015f), V(0.36f, 0.36f, 0.03f), trimMat);
            CreateDecor(frontGroup.transform, "Clock", V(2.2f, 2.65f, RoomHalfZ - 0.04f), V(0.32f, 0.32f, 0.03f), clockMat);
        }

        static GameObject CreateOcclusionGroup(Transform parent, string name, Vector3 planePoint, Vector3 inwardNormal)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            var fade = group.AddComponent<OccludableFade>();
            fade.ConfigureSeparationPlane(planePoint, inwardNormal);
            return group;
        }

        // Four bays with five pillars (both ends included). Curtains/frames parent under the sill so they hide with it.
        static void BuildWindowWall(Transform room, Material wallMat, Material pillarMat, Material frameMat, Material yardMat)
        {
            const int bayCount = 4;
            const float pillarW = 0.35f;
            const float curtainW = 0.32f;
            var bayW = (RoomHalfZ * 2f - (bayCount + 1) * pillarW) / bayCount;
            var step = bayW + pillarW;
            var sillTop = FloorTop + 0.9f;
            var winTop = FloorTop + 2.5f;
            var xOut = -OuterX;
            var xIn = -RoomHalfX;
            var curtainMat = PrototypeLookBuilder.CurtainMaterial;

            var windowGroup = CreateOcclusionGroup(
                room,
                "WindowWall",
                new Vector3(-RoomHalfX, 1.5f, 0f),
                Vector3.right);
            var windowAttach = windowGroup.transform;

            ShellBox(windowAttach, "Wall_Window_Sill", V(xOut, 0f, -RoomHalfZ), V(xIn, sillTop, RoomHalfZ), wallMat, occludable: false);
            ShellBox(windowAttach, "Wall_Window_Header", V(xOut, winTop, -RoomHalfZ), V(xIn, ShellTop, RoomHalfZ), wallMat, occludable: false);

            for (var i = 0; i <= bayCount; i++)
            {
                var z0 = -RoomHalfZ + i * step;
                ShellBox(windowAttach, $"Wall_Window_Pillar{i}", V(xOut, sillTop, z0), V(xIn, winTop, z0 + pillarW), wallMat, occludable: false);
                Box(windowAttach, $"Pillar_Face{i}", V(xIn, FloorTop, z0), V(xIn + 0.08f, CeilingBottom, z0 + pillarW), pillarMat);
            }

            for (var j = 0; j < bayCount; j++)
            {
                var zMin = -RoomHalfZ + pillarW + j * step;
                var zMax = zMin + bayW;
                var zc = (zMin + zMax) * 0.5f;
                var fx0 = xIn - 0.03f;
                var fx1 = xIn + 0.02f;

                Box(windowAttach, "WinFrame_Sill", V(fx0, sillTop, zMin), V(xIn + 0.1f, sillTop + 0.04f, zMax), frameMat);
                Box(windowAttach, "WinFrame_Head", V(fx0, winTop - 0.05f, zMin), V(fx1, winTop, zMax), frameMat);
                Box(windowAttach, "WinFrame_Rail", V(fx0, sillTop + 1.0f, zMin), V(fx1, sillTop + 1.04f, zMax), frameMat);
                Box(windowAttach, "WinFrame_Stile_A", V(fx0, sillTop, zc - 0.035f), V(fx1, winTop, zc - 0.005f), frameMat);
                Box(windowAttach, "WinFrame_Stile_B", V(fx0, sillTop, zc + 0.005f), V(fx1, winTop, zc + 0.035f), frameMat);

                Box(windowAttach, "Curtain_L", V(xIn + 0.075f, 0.45f, zMin), V(xIn + 0.125f, 2.78f, zMin + curtainW), curtainMat);
                Box(windowAttach, "Curtain_L_Fold", V(xIn + 0.10f, 0.5f, zMin + curtainW - 0.06f), V(xIn + 0.15f, 2.74f, zMin + curtainW + 0.06f), curtainMat);
                Box(windowAttach, "Curtain_R", V(xIn + 0.075f, 0.45f, zMax - curtainW), V(xIn + 0.125f, 2.78f, zMax), curtainMat);
                Box(windowAttach, "Curtain_R_Fold", V(xIn + 0.10f, 0.5f, zMax - curtainW - 0.06f), V(xIn + 0.15f, 2.74f, zMax - curtainW + 0.06f), curtainMat);
            }

            Box(windowAttach, "CurtainRod", V(xIn + 0.14f, 2.76f, -RoomHalfZ + 0.05f), V(xIn + 0.18f, 2.80f, RoomHalfZ - 0.05f), PrototypeLookBuilder.TrimMaterial);

            // Quad normal is -Z, so Euler(0,-90,0) turns it to +X and faces the room.
            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "SchoolyardView";
            backdrop.transform.SetParent(room, false);
            backdrop.transform.position = new Vector3(-10f, 3.4f, 0f);
            backdrop.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            backdrop.transform.localScale = new Vector3(26f, 8f, 1f);
            PrototypeLookBuilder.ApplySharedMaterial(backdrop, yardMat);
            Object.DestroyImmediate(backdrop.GetComponent<Collider>());

            var groundWidth = 10f - OuterX;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ground.name = "YardGround";
            ground.transform.SetParent(room, false);
            ground.transform.position = new Vector3(-OuterX - groundWidth * 0.5f, -0.1f, 0f);
            ground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ground.transform.localScale = new Vector3(groundWidth, 26f, 1f);
            PrototypeLookBuilder.ApplySharedMaterial(ground, PrototypeLookBuilder.YardGroundMaterial);
            Object.DestroyImmediate(ground.GetComponent<Collider>());
        }

        // JP classroom corridor wall: 引き違い戸 ~0.9m leaves (≈1.8m bay) front/rear, high 連窓 between.
        static void BuildCorridorWall(Transform room, Material wallMat, Material frameMat)
        {
            const float doorHalfW = 0.9f;
            // Push front/rear doors toward the chalkboard and back walls (JP classroom ends).
            const float doorFrontZ = 3.45f;
            const float pierW = 0.28f;
            const int paneCount = 3;
            var xIn = RoomHalfX;
            var xOut = OuterX;
            var doorTop = FloorTop + 2.0f;
            var hiSill = FloorTop + 1.2f;
            var hiTop = FloorTop + 2.4f;
            var frontMin = doorFrontZ - doorHalfW;
            var frontMax = doorFrontZ + doorHalfW;
            var rearMin = -doorFrontZ - doorHalfW;
            var rearMax = -doorFrontZ + doorHalfW;

            var corridorGroup = CreateOcclusionGroup(
                room,
                "CorridorWall",
                new Vector3(RoomHalfX, 1.5f, 0f),
                Vector3.left);
            room = corridorGroup.transform;

            ShellBox(room, "Wall_Corridor_StubFront", V(xIn, 0f, frontMax), V(xOut, ShellTop, RoomHalfZ), wallMat, false, occludable: false);
            ShellBox(room, "Wall_Corridor_StubRear", V(xIn, 0f, -RoomHalfZ), V(xOut, ShellTop, rearMin), wallMat, false, occludable: false);
            ShellBox(room, "Wall_Corridor_HeadFront", V(xIn, doorTop, frontMin), V(xOut, ShellTop, frontMax), wallMat, false, occludable: false);
            ShellBox(room, "Wall_Corridor_HeadRear", V(xIn, doorTop, rearMin), V(xOut, ShellTop, rearMax), wallMat, false, occludable: false);
            ShellBox(room, "Wall_Corridor_Lower", V(xIn, 0f, rearMax), V(xOut, hiSill, frontMin), wallMat, false, occludable: false);
            ShellBox(room, "Wall_Corridor_Upper", V(xIn, hiTop, rearMax), V(xOut, ShellTop, frontMin), wallMat, false, occludable: false);

            var span = frontMin - rearMax;
            var paneW = (span - (paneCount + 1) * pierW) / paneCount;
            var step = paneW + pierW;
            for (var i = 0; i <= paneCount; i++)
            {
                var z0 = rearMax + i * step;
                ShellBox(room, $"Wall_Corridor_Pier{i}", V(xIn, hiSill, z0), V(xOut, hiTop, z0 + pierW), wallMat, false, occludable: false);
            }

            for (var j = 0; j < paneCount; j++)
            {
                var zMin = rearMax + pierW + j * step;
                var zMax = zMin + paneW;
                var zc = (zMin + zMax) * 0.5f;
                var fx0 = xIn - 0.02f;
                var fx1 = xIn + 0.03f;
                Box(room, "CorrFrame_Sill", V(xIn - 0.08f, hiSill, zMin), V(fx1, hiSill + 0.04f, zMax), frameMat);
                Box(room, "CorrFrame_Head", V(fx0, hiTop - 0.05f, zMin), V(fx1, hiTop, zMax), frameMat);
                Box(room, "CorrFrame_Stile", V(fx0, hiSill, zc - 0.02f), V(fx1, hiTop, zc + 0.02f), frameMat);
            }

            var doorMat = PrototypeLookBuilder.DoorMaterial;
            var paneMat = ColorMaterial(new Color(0.78f, 0.88f, 0.92f));
            var handleMat = ColorMaterial(new Color(0.25f, 0.27f, 0.30f));

            foreach (var (zMin, zMax) in new[] { (frontMin, frontMax), (rearMin, rearMax) })
            {
                Box(room, "DoorFrame_JambA", V(xIn - 0.03f, FloorTop, zMin), V(xOut, doorTop, zMin + 0.05f), frameMat);
                Box(room, "DoorFrame_JambB", V(xIn - 0.03f, FloorTop, zMax - 0.05f), V(xOut, doorTop, zMax), frameMat);
                Box(room, "DoorFrame_Head", V(xIn - 0.03f, doorTop - 0.05f, zMin), V(xOut, doorTop, zMax), frameMat);
            }

            const float outerLeafX = RoomHalfX + 0.06f;
            const float innerLeafX = RoomHalfX + 0.01f;
            var mid = doorFrontZ;
            BuildDoorLeaf(room, "DoorFront_Outer", outerLeafX, frontMin, mid + 0.01f, mid + 0.01f, doorTop, doorMat, paneMat, handleMat);
            BuildDoorLeaf(room, "DoorFront_Inner", innerLeafX, frontMin, mid + 0.01f, frontMin + 0.04f, doorTop, doorMat, paneMat, handleMat);
            BuildDoorLeaf(room, "DoorRear_Outer", outerLeafX, rearMin, -mid + 0.01f, -mid + 0.01f, doorTop, doorMat, paneMat, handleMat);
            BuildDoorLeaf(room, "DoorRear_Inner", innerLeafX, -mid - 0.01f, rearMax, -mid - 0.01f, doorTop, doorMat, paneMat, handleMat);
        }

        static void BuildDoorLeaf(
            Transform room,
            string name,
            float xCenter,
            float z0,
            float z1,
            float handleZ,
            float doorTop,
            Material doorMat,
            Material paneMat,
            Material handleMat)
        {
            var leaf = new GameObject(name);
            leaf.transform.SetParent(room, false);
            Box(leaf.transform, "Leaf", V(xCenter - 0.02f, FloorTop, z0), V(xCenter + 0.02f, doorTop - 0.03f, z1), doorMat);
            Box(leaf.transform, "Pane", V(xCenter - 0.025f, FloorTop + 1.05f, z0 + 0.07f), V(xCenter + 0.025f, doorTop - 0.12f, z1 - 0.07f), paneMat);
            Box(leaf.transform, "Handle", V(xCenter - 0.04f, FloorTop + 0.95f, handleZ - 0.03f), V(xCenter + 0.04f, FloorTop + 1.12f, handleZ + 0.03f), handleMat);
        }

        // Floor strip, far wall and ceiling so the doors and high windows read as a corridor, not a void.
        static void BuildCorridorExterior(Transform room, Material wallMat)
        {
            var far = OuterX + CorridorWidth;
            var farOut = far + ShellT;
            var corridorFloorMat = ColorMaterial(new Color(0.74f, 0.72f, 0.66f));
            Box(room, "Corridor_Floor", V(OuterX, FloorCenterY - FloorThickness * 0.5f, -CorridorHalfLength), V(far, FloorTop, CorridorHalfLength), corridorFloorMat);
            ShellBox(room, "Wall_CorridorFar", V(far, 0f, -CorridorHalfLength), V(farOut, ShellTop, CorridorHalfLength), wallMat, false);
            ShellBox(room, "Ceiling_Corridor", V(OuterX, CeilingBottom, -CorridorHalfLength), V(farOut, ShellTop, CorridorHalfLength), wallMat, false);
            ShellBox(room, "Wall_CorridorEndFront", V(OuterX, 0f, CorridorHalfLength), V(farOut, ShellTop, CorridorHalfLength + ShellT), wallMat, false);
            ShellBox(room, "Wall_CorridorEndRear", V(OuterX, 0f, -CorridorHalfLength - ShellT), V(farOut, ShellTop, -CorridorHalfLength), wallMat, false);

            var glassMat = ColorMaterial(new Color(0.55f, 0.72f, 0.82f));
            var doorMat = PrototypeLookBuilder.DoorMaterial;
            foreach (var zc in new[] { -3.0f, -0.2f, 2.6f })
            {
                Box(room, "CorrFar_Window", V(far - 0.02f, FloorTop + 1.2f, zc - 0.8f), V(far, FloorTop + 2.3f, zc + 0.8f), glassMat);
            }

            Box(room, "CorrFar_Door", V(far - 0.03f, FloorTop, 1.0f), V(far, FloorTop + 2.0f, 2.0f), doorMat);
        }

        // Board front must sit nearer the room than the frame front, otherwise the frame hides the green.
        // Parent is Wall_Front so chalk/side boards hide with the front wall.
        static void BuildChalkboardWall(Transform room, Material chalkMat, Material trimMat)
        {
            const float front = RoomHalfZ;
            const float boardW = 3.6f;
            const float boardH = 1.3f;
            var boardBottom = FloorTop + 0.9f;
            var boardY = boardBottom + boardH * 0.5f;
            var ledgeTop = boardBottom - 0.015f;
            CreateDecor(room, "ChalkFrame", new Vector3(0f, boardY, front - 0.02f), new Vector3(boardW + 0.2f, boardH + 0.1f, 0.04f), trimMat);
            CreateDecor(room, "Chalkboard", new Vector3(0f, boardY, front - 0.04f), new Vector3(boardW, boardH, 0.04f), chalkMat);
            CreateDecor(room, "ChalkLedge", new Vector3(0f, ledgeTop - 0.025f, front - 0.11f), new Vector3(boardW + 0.1f, 0.05f, 0.14f), trimMat);
            var chalkY = ledgeTop + 0.0125f;
            CreateDecor(room, "Chalk_White", new Vector3(-0.8f, chalkY, front - 0.14f), new Vector3(0.09f, 0.025f, 0.025f), ColorMaterial(Color.white));
            CreateDecor(room, "Chalk_Yellow", new Vector3(-0.6f, chalkY, front - 0.12f), new Vector3(0.08f, 0.025f, 0.025f), ColorMaterial(new Color(0.98f, 0.88f, 0.30f)));
            CreateDecor(room, "Chalk_Pink", new Vector3(-0.4f, chalkY, front - 0.14f), new Vector3(0.08f, 0.025f, 0.025f), ColorMaterial(new Color(0.98f, 0.60f, 0.70f)));
            CreateDecor(room, "ChalkEraser_A", new Vector3(0.9f, chalkY + 0.005f, front - 0.12f), new Vector3(0.14f, 0.035f, 0.06f), ColorMaterial(new Color(0.30f, 0.30f, 0.34f)));
            CreateDecor(room, "ChalkEraser_B", new Vector3(1.15f, chalkY + 0.005f, front - 0.14f), new Vector3(0.14f, 0.035f, 0.06f), ColorMaterial(new Color(0.45f, 0.40f, 0.34f)));
            CreateDecor(room, "Speaker", new Vector3(0f, 2.78f, front - 0.09f), new Vector3(0.45f, 0.26f, 0.12f), ColorMaterial(new Color(0.38f, 0.38f, 0.40f)));

            var boardGreen = PrototypeLookBuilder.BoardGreenMaterial;
            const float sideX = 2.55f;
            foreach (var side in new[] { -1f, 1f })
            {
                var x = side * sideX;
                CreateDecor(room, "Bulletin_Side", new Vector3(x, 1.65f, front - 0.02f), new Vector3(1.1f, 1.4f, 0.04f), boardGreen);
                PlacePaper(room, "Paper_Side_A", new Vector3(x - 0.2f, 1.9f, front - 0.07f), new Color(0.98f, 0.96f, 0.88f), facePositiveZ: false);
                PlacePaper(room, "Paper_Side_B", new Vector3(x + 0.2f, 1.45f, front - 0.07f), new Color(0.98f, 0.90f, 0.55f), facePositiveZ: false);

                var shelf = new Vector3(x - side * 0.05f, FloorTop + 0.35f, front - 0.2f);
                CreateDecor(room, "Shelf", shelf, new Vector3(0.9f, 0.7f, 0.35f), PrototypeLookBuilder.DeskMaterial);
                CreateDecor(room, "ShelfTop", shelf + new Vector3(0f, 0.36f, 0f), new Vector3(0.95f, 0.03f, 0.4f), PrototypeLookBuilder.TrimMaterial);
            }

            var shelfTopY = FloorTop + 0.35f + 0.375f;
            var potTop = new Vector3(-sideX - 0.05f, shelfTopY + 0.1f, front - 0.2f);
            CreateDecor(room, "PlantPot", potTop, new Vector3(0.2f, 0.2f, 0.2f), ColorMaterial(new Color(0.75f, 0.42f, 0.30f)));
            var leafMat = ColorMaterial(new Color(0.22f, 0.55f, 0.25f));
            CreateDecor(room, "PlantLeaves_A", potTop + new Vector3(0f, 0.22f, 0f), new Vector3(0.3f, 0.28f, 0.3f), leafMat);
            var leavesB = CreateDecor(room, "PlantLeaves_B", potTop + new Vector3(0f, 0.28f, 0f), new Vector3(0.26f, 0.26f, 0.26f), leafMat);
            leavesB.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            var bookColors = new[]
            {
                new Color(0.85f, 0.30f, 0.30f), new Color(0.30f, 0.45f, 0.85f), new Color(0.95f, 0.80f, 0.30f), new Color(0.35f, 0.65f, 0.40f),
            };
            for (var i = 0; i < bookColors.Length; i++)
            {
                CreateDecor(room, "Book", new Vector3(sideX - 0.15f + i * 0.09f, shelfTopY + 0.13f + (i % 2) * 0.015f, front - 0.2f), new Vector3(0.07f, 0.24f + (i % 2) * 0.03f, 0.2f), ColorMaterial(bookColors[i]));
            }
        }

        // One cleaning locker on the left; long low storage with vertical bays + corkboard above.
        static void BuildBackWall(Transform room, Material corkMat)
        {
            const float unitW = 0.7f;
            const float lockerD = 0.45f;
            const float lockerH = 1.85f;
            const float x0 = -RoomHalfX + 0.25f;
            var x1 = x0 + unitW;
            var backZ = -RoomHalfZ;
            var frontZ = backZ + lockerD;
            var bodyMat = ColorMaterial(new Color(0.40f, 0.46f, 0.50f));
            var doorMat = PrototypeLookBuilder.LockerMaterial;
            var darkMat = ColorMaterial(new Color(0.25f, 0.28f, 0.30f));

            Box(room, "LockerBody", V(x0, FloorTop, backZ), V(x1, FloorTop + lockerH, frontZ), bodyMat);
            Box(room, "LockerCap", V(x0 - 0.01f, FloorTop + lockerH, backZ), V(x1 + 0.01f, FloorTop + lockerH + 0.03f, frontZ + 0.01f), doorMat);
            Box(room, "LockerDoor", V(x0 + 0.02f, FloorTop + 0.05f, frontZ), V(x1 - 0.02f, FloorTop + lockerH - 0.04f, frontZ + 0.02f), doorMat);
            for (var k = 0; k < 4; k++)
            {
                var y = FloorTop + lockerH - 0.2f - k * 0.05f;
                Box(room, "LockerVent", V(x0 + 0.1f, y, frontZ + 0.02f), V(x1 - 0.1f, y + 0.015f, frontZ + 0.026f), darkMat);
            }

            Box(room, "LockerHandle", V(x1 - 0.12f, FloorTop + 0.85f, frontZ + 0.02f), V(x1 - 0.08f, FloorTop + 1.1f, frontZ + 0.05f), darkMat);

            var bucketMat = ColorMaterial(new Color(0.30f, 0.55f, 0.80f));
            Box(room, "Bucket", V(x1 + 0.12f, FloorTop, backZ + 0.15f), V(x1 + 0.42f, FloorTop + 0.28f, backZ + 0.45f), bucketMat);

            var shelfX0 = x1 + 0.55f;
            var shelfX1 = RoomHalfX - 0.35f;
            const float shelfH = 0.95f;
            const float shelfD = 0.38f;
            const int bayCount = 6;
            var shelfMat = PrototypeLookBuilder.DeskMaterial;
            var shelfW = shelfX1 - shelfX0;
            var bayW = shelfW / bayCount;
            Box(room, "StorageShelf_Body", V(shelfX0, FloorTop, backZ), V(shelfX1, FloorTop + shelfH, backZ + shelfD), shelfMat);
            foreach (var level in new[] { 0.02f, 0.34f, 0.64f })
            {
                Box(room, "StorageShelf_Board", V(shelfX0 + 0.03f, FloorTop + level + 0.03f, backZ + 0.04f), V(shelfX1 - 0.03f, FloorTop + level + 0.31f, backZ + shelfD + 0.005f), darkMat);
            }

            for (var i = 1; i < bayCount; i++)
            {
                var dx = shelfX0 + i * bayW;
                Box(room, "StorageShelf_Divider", V(dx - 0.015f, FloorTop + 0.04f, backZ + 0.03f), V(dx + 0.015f, FloorTop + shelfH - 0.04f, backZ + shelfD - 0.02f), darkMat);
            }

            var boxColors = new[]
            {
                new Color(0.80f, 0.70f, 0.50f), new Color(0.55f, 0.65f, 0.80f), new Color(0.85f, 0.55f, 0.45f),
                new Color(0.60f, 0.75f, 0.55f), new Color(0.90f, 0.75f, 0.45f), new Color(0.70f, 0.55f, 0.75f),
            };
            for (var i = 0; i < bayCount; i++)
            {
                var bx0 = shelfX0 + i * bayW + 0.08f;
                var bx1 = shelfX0 + (i + 1) * bayW - 0.08f;
                Box(room, "StorageBox", V(bx0, FloorTop + 0.37f, backZ + 0.07f), V(bx1, FloorTop + 0.6f, backZ + shelfD - 0.03f), ColorMaterial(boxColors[i % boxColors.Length]));
            }

            var boardCx = (shelfX0 + shelfX1) * 0.5f;
            var boardW = Mathf.Min(shelfW - 0.2f, 3.4f);
            CreateDecor(room, "Bulletin_Back", new Vector3(boardCx, 1.9f, backZ + 0.02f), new Vector3(boardW, 1.2f, 0.04f), corkMat);
            PlacePaper(room, "Paper_Red", new Vector3(boardCx - boardW * 0.28f, 2.05f, backZ + 0.05f), new Color(0.95f, 0.35f, 0.35f));
            PlacePaper(room, "Paper_Blue", new Vector3(boardCx, 1.8f, backZ + 0.05f), new Color(0.35f, 0.55f, 0.95f));
            PlacePaper(room, "Paper_Yellow", new Vector3(boardCx + boardW * 0.28f, 2.05f, backZ + 0.05f), new Color(0.95f, 0.85f, 0.25f));
        }

        // Side rows are 2-desk joined pairs stacked along Z. Most chairs tuck under; a few stay pulled out.
        static void BuildPropDeskGrid(Transform parent, Material deskMat, Material legMat)
        {
            var group = new GameObject("PropDesks");
            group.transform.SetParent(parent, false);
            var pairCentersX = new[] { -2.15f, 2.15f };
            var pairZs = new[] { -3.4f, -2.2f, -1.0f, 0.2f, 1.4f, 2.6f };
            var index = 0;
            foreach (var cx in pairCentersX)
            {
                foreach (var z in pairZs)
                {
                    PlacePropDesk(group.transform, new Vector3(cx - 0.3f, DeskCenterY, z), deskMat, legMat, index, tuckChair: index % 3 != 0);
                    index++;
                    PlacePropDesk(group.transform, new Vector3(cx + 0.3f, DeskCenterY, z), deskMat, legMat, index, tuckChair: index % 3 != 0);
                    index++;
                }
            }
        }

        static void PlacePropDesk(Transform parent, Vector3 position, Material deskMat, Material legMat, int index, bool tuckChair)
        {
            var desk = new GameObject("PropDesk");
            desk.transform.SetParent(parent, false);
            desk.transform.position = position;
            CreateDecorLocal(desk.transform, "Top", Vector3.zero, new Vector3(0.58f, 0.04f, 0.42f), deskMat);
            CreateDecorLocal(desk.transform, "Tray", new Vector3(0f, -0.1f, 0.02f), new Vector3(0.48f, 0.1f, 0.34f), legMat);
            foreach (var xz in new[] { new Vector2(-0.24f, -0.16f), new Vector2(0.24f, -0.16f), new Vector2(-0.24f, 0.16f), new Vector2(0.24f, 0.16f) })
            {
                CreateDecorLocal(desk.transform, "Leg", new Vector3(xz.x, -0.38f, xz.y), new Vector3(0.04f, 0.72f, 0.04f), legMat);
            }

            // Tucked chairs sit under the top (z≈-0.12). Pulled chairs stay clear of the next row (spacing ≥1.2).
            var pull = tuckChair ? 0.12f : 0.42f;
            var chair = new GameObject("Chair");
            chair.transform.SetParent(desk.transform, false);
            chair.transform.localPosition = new Vector3(0f, 0f, -pull);
            chair.transform.localRotation = Quaternion.Euler(0f, tuckChair ? 0f : (((index * 37) % 5) - 2) * 3f, 0f);
            CreateDecorLocal(chair.transform, "Seat", new Vector3(0f, -0.31f, 0f), new Vector3(0.36f, 0.03f, 0.34f), deskMat);
            CreateDecorLocal(chair.transform, "Back", new Vector3(0f, -0.155f, -0.155f), new Vector3(0.36f, 0.28f, 0.03f), deskMat);
            foreach (var xz in new[] { new Vector2(-0.14f, -0.14f), new Vector2(0.14f, -0.14f), new Vector2(-0.14f, 0.14f), new Vector2(0.14f, 0.14f) })
            {
                CreateDecorLocal(chair.transform, "ChairLeg", new Vector3(xz.x, -0.5375f, xz.y), new Vector3(0.025f, 0.425f, 0.025f), legMat);
            }
        }

        static Material ColorMaterial(Color color)
        {
            if (ColorMaterials.TryGetValue(color, out var cached) && cached != null)
            {
                return cached;
            }

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            ColorMaterials[color] = mat;
            return mat;
        }

        static GameObject CreateDecor(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            PrototypeLookBuilder.ApplySharedMaterial(go, mat);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static void CreateDecorLocal(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            PrototypeLookBuilder.ApplySharedMaterial(go, mat);
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        static void PlacePaper(Transform parent, string name, Vector3 position, Color color, bool facePositiveZ = true)
        {
            var paper = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paper.name = name;
            paper.transform.SetParent(parent, false);
            paper.transform.position = position;
            paper.transform.rotation = Quaternion.Euler(0f, facePositiveZ ? 180f : 0f, 0f);
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

    }
}
