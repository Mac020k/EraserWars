using System.Linq;
using System.Reflection;
using FishNet.Component.Transforming;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting.Tugboat;
using EraserWars.Gameplay;
using EraserWars.Net;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class FishNetSpikeSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/FishNet_Spike.unity";

        [MenuItem("EraserWars/Build Fish-Net Spike Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[EraserWars] Stop Play Mode before building FishNet_Spike.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("FishNetSpike");
            root.AddComponent<NetworkObject>();
            var session = root.AddComponent<FishNetSpikeSession>();
            var flickApplier = root.AddComponent<FlickApplier>();
            var settleWatcher = root.AddComponent<SettleWatcher>();
            var hud = root.AddComponent<FishNetSpikeHud>();

            // NetworkManager must be a root object for DontDestroyOnLoad.
            var nmGo = new GameObject("NetworkManager");
            var nm = nmGo.AddComponent<NetworkManager>();
            nmGo.AddComponent<Tugboat>();

            // Keep AutoSmoke off the NetworkObject so init failures cannot skip the smoke runner.
            var autoGo = new GameObject("FishNetSpikeAutoSmoke");
            var autoSmoke = autoGo.AddComponent<FishNetSpikeAutoSmoke>();

            // Ground plane so the spike desk does not read as floating in void.
            const float floorThickness = 0.08f;
            const float deskCenterY = 0.75f;
            const float deskThickness = 0.04f;
            // Push legs through the floor top so the contact never reads as a gap under hard shadows.
            const float legSink = 0.05f;
            var floorTop = floorThickness * 0.5f;
            var deskBottom = deskCenterY - deskThickness * 0.5f;
            var legBottom = floorTop - legSink;
            var legHeight = deskBottom - legBottom;
            var legCenterY = (legBottom + deskBottom) * 0.5f;

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, 0f, 0f);
            floor.transform.localScale = new Vector3(4f, floorThickness, 4f);

            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(root.transform);
            desk.transform.position = new Vector3(0f, deskCenterY, 0f);
            desk.transform.localScale = new Vector3(1.2f, deskThickness, 0.8f);

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
                leg.transform.SetParent(root.transform);
                leg.transform.position = new Vector3(xz.x, legCenterY, xz.y);
                leg.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
            }

            var spawn = new GameObject("Spawn");
            spawn.transform.SetParent(root.transform);
            spawn.transform.position = new Vector3(0f, 0.782f, -0.15f);

            var eraserGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eraserGo.name = "Eraser";
            eraserGo.transform.SetParent(root.transform);
            eraserGo.transform.position = spawn.transform.position;
            eraserGo.transform.localScale = new Vector3(0.06f, 0.03f, 0.12f);
            var rb = eraserGo.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = eraserGo.AddComponent<Rigidbody>();
            }

            rb.mass = 0.02f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var eraserBody = eraserGo.AddComponent<EraserBody>();
            eraserGo.AddComponent<NetworkObject>();
            eraserGo.AddComponent<NetworkTransform>();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("eraser").objectReferenceValue = eraserBody;
            sessionSo.FindProperty("flickApplier").objectReferenceValue = flickApplier;
            sessionSo.FindProperty("settleWatcher").objectReferenceValue = settleWatcher;
            sessionSo.FindProperty("spawnPoint").objectReferenceValue = spawn.transform;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("networkManager").objectReferenceValue = nm;
            hudSo.FindProperty("session").objectReferenceValue = session;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            var autoSo = new SerializedObject(autoSmoke);
            autoSo.FindProperty("networkManager").objectReferenceValue = nm;
            autoSo.FindProperty("session").objectReferenceValue = session;
            autoSo.FindProperty("autoHostInEditor").boolValue = true;
            autoSo.FindProperty("autoClientInPlayer").boolValue = true;
            autoSo.ApplyModifiedPropertiesWithoutUndo();

            var cam = Camera.main;
            if (cam != null)
            {
                cam.fieldOfView = 48f;
                cam.transform.position = new Vector3(-1.6f, 2.4f, -2.6f);
                cam.transform.LookAt(new Vector3(0f, 0.4f, 0f));
            }

            ReserializeSceneNetworkObjects();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {ScenePath}. Play Mode auto-starts Host + canned flick; player build auto-starts Client.");
        }

        static void ReserializeSceneNetworkObjects()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var createSceneId = typeof(NetworkObject).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m =>
                    m.Name == "CreateSceneId"
                    && m.GetParameters().Length == 3
                    && m.GetParameters()[0].ParameterType == typeof(UnityEngine.SceneManagement.Scene));
            var reserialize = typeof(NetworkObject).GetMethod(
                "ReserializeEditorSetValues",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var nobs = Object.FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var changed = 0;
            if (createSceneId != null)
            {
                var args = new object[] { scene, true, 0 };
                createSceneId.Invoke(null, args);
                changed = (int)args[2];
            }

            // SceneId is internal; read via reflection, write via public SetSceneId.
            var sceneIdField = typeof(NetworkObject).GetField(
                "SceneId",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var used = new System.Collections.Generic.HashSet<ulong>();
            foreach (var nob in nobs)
            {
                var current = sceneIdField != null ? (ulong)sceneIdField.GetValue(nob) : 0UL;
                if (current != 0UL && used.Add(current))
                {
                    continue;
                }

                ulong next;
                do
                {
                    next = (ulong)(uint)Random.Range(1, int.MaxValue) | ((ulong)(uint)scene.path.GetHashCode() << 32);
                }
                while (next == 0UL || !used.Add(next));

                nob.SetSceneId(next);
                changed++;
                EditorUtility.SetDirty(nob);
            }

            foreach (var nob in nobs)
            {
                reserialize?.Invoke(nob, new object[] { true, false });
                EditorUtility.SetDirty(nob);
            }

            Debug.Log($"[EraserWars] SceneId refresh on FishNet_Spike: nobs={nobs.Length} changed={changed}");
        }
    }
}
