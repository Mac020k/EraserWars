using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace Keshipin.Net
{
    /// <summary>
    /// Minimal Host/Client HUD for the Fish-Net compile and Tugboat smoke check.
    /// </summary>
    public sealed class FishNetSpikeHud : MonoBehaviour
    {
        [SerializeField] NetworkManager networkManager;
        [SerializeField] Tugboat tugboat;

        void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<NetworkManager>();
            }

            if (tugboat == null && networkManager != null)
            {
                tugboat = networkManager.TransportManager.Transport as Tugboat;
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 360f, 160f));
            GUILayout.Label("Fish-Net spike (Tugboat)");
            var nm = networkManager != null ? networkManager : InstanceFinder.NetworkManager;
            if (nm == null)
            {
                GUILayout.Label("No NetworkManager in scene.");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Server={nm.IsServerStarted} Client={nm.IsClientStarted}");
            if (GUILayout.Button("Start Host"))
            {
                nm.ServerManager.StartConnection();
                nm.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Start Client"))
            {
                nm.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Stop"))
            {
                nm.ServerManager.StopConnection(true);
                nm.ClientManager.StopConnection();
            }

            GUILayout.EndArea();
        }
    }
}
