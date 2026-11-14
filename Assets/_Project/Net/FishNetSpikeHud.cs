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
        [SerializeField] FishNetSpikeSession session;

        void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindAnyObjectByType<NetworkManager>();
            }

            if (tugboat == null && networkManager != null)
            {
                tugboat = networkManager.TransportManager.Transport as Tugboat;
            }

            if (session == null)
            {
                session = FindAnyObjectByType<FishNetSpikeSession>();
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 420f, 260f));
            GUILayout.Label("Fish-Net spike (Tugboat + HostCommandGate)");
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

            GUI.enabled = nm.IsServerStarted || nm.IsClientStarted;
            if (GUILayout.Button("Fire canned flick (HostCommandGate)"))
            {
                if (session != null)
                {
                    session.UiRequestCannedFlick();
                }
            }

            GUI.enabled = true;
            if (session != null)
            {
                GUILayout.Label($"Status: {session.LastStatus}");
                if (session.HasReportedPose)
                {
                    var p = session.LastReportedPose;
                    GUILayout.Label($"Host settle: ({p.x:F3}, {p.y:F3}, {p.z:F3})");
                    var delta = session.PoseDeltaMeters;
                    if (delta >= 0f)
                    {
                        GUILayout.Label($"Local vs host delta: {delta:F4} m");
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
