using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using Keshipin.Net;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Keshipin.Editor
{
    public static class FishNetSpikeSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/FishNet_Spike.unity";

        [MenuItem("Keshipin/Build Fish-Net Spike Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("FishNetSpike");
            var nmGo = new GameObject("NetworkManager");
            nmGo.transform.SetParent(root.transform);
            var nm = nmGo.AddComponent<NetworkManager>();
            nmGo.AddComponent<Tugboat>();

            var hud = root.AddComponent<FishNetSpikeHud>();
            var so = new SerializedObject(hud);
            so.FindProperty("networkManager").objectReferenceValue = nm;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {ScenePath}. Enter Play and press Start Host.");
        }
    }
}
