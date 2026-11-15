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
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("FishNetSpike");
            root.AddComponent<NetworkObject>();
            var session = root.AddComponent<FishNetSpikeSession>();
            var flickApplier = root.AddComponent<FlickApplier>();
            var settleWatcher = root.AddComponent<SettleWatcher>();
            var hud = root.AddComponent<FishNetSpikeHud>();

            var nmGo = new GameObject("NetworkManager");
            nmGo.transform.SetParent(root.transform);
            var nm = nmGo.AddComponent<NetworkManager>();
            nmGo.AddComponent<Tugboat>();

            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(root.transform);
            desk.transform.position = new Vector3(0f, 0.4f, 0f);
            desk.transform.localScale = new Vector3(1.2f, 0.08f, 0.7f);

            var spawn = new GameObject("Spawn");
            spawn.transform.SetParent(root.transform);
            spawn.transform.position = new Vector3(0f, 0.55f, -0.15f);

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

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {ScenePath}. Enter Play, Start Host, then Fire canned flick.");
        }
    }
}
