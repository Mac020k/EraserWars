using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace EraserWars.Net
{
    /// <summary>
    /// Minimal Host/Client HUD for the Fish-Net compile and Tugboat smoke check.
    /// </summary>
    public sealed class FishNetSpikeHud : MonoBehaviour
    {
        [SerializeField] NetworkManager networkManager;
        [SerializeField] Tugboat tugboat;
        [SerializeField] FishNetSpikeSession session;

        GUIStyle titleStyle;
        GUIStyle bodyStyle;
        GUIStyle deltaStyle;
        GUIStyle hintStyle;
        bool stylesReady;

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
            EnsureStyles();
            var area = new Rect(12f, 12f, 460f, 318f);
            DrawPanel(area);

            GUILayout.BeginArea(new Rect(area.x + 16f, area.y + 14f, area.width - 32f, area.height - 28f));
            GUILayout.Label("ERASER WARS", titleStyle);
            GUILayout.Label("Fish-Net Spike  ·  Host / Client pose check", hintStyle);
            GUILayout.Space(6f);
            var nm = networkManager != null ? networkManager : InstanceFinder.NetworkManager;
            if (nm == null)
            {
                GUILayout.Label("No NetworkManager in scene.", bodyStyle);
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Server={nm.IsServerStarted}   Client={nm.IsClientStarted}", bodyStyle);
            GUILayout.Space(4f);
            if (GUILayout.Button("Start Host", GUILayout.Height(36f)))
            {
                nm.ServerManager.StartConnection();
                nm.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Start Client", GUILayout.Height(36f)))
            {
                nm.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Stop", GUILayout.Height(30f)))
            {
                nm.ServerManager.StopConnection(true);
                nm.ClientManager.StopConnection();
            }

            GUI.enabled = nm.IsServerStarted || nm.IsClientStarted;
            if (GUILayout.Button("Fire canned flick (HostCommandGate)", GUILayout.Height(38f)))
            {
                if (session != null)
                {
                    session.UiRequestCannedFlick();
                }
            }

            GUI.enabled = true;
            if (session != null)
            {
                GUILayout.Space(8f);
                GUILayout.Label($"Status: {session.LastStatus}", bodyStyle);
                if (session.HasReportedPose)
                {
                    var p = session.LastReportedPose;
                    GUILayout.Label($"Host settle: ({p.x:F3}, {p.y:F3}, {p.z:F3})", bodyStyle);
                    var delta = session.PoseDeltaMeters;
                    if (delta >= 0f)
                    {
                        GUILayout.Label($"Local vs host delta: {delta:F4} m", deltaStyle);
                    }
                }
            }

            GUILayout.EndArea();
        }

        static void DrawPanel(Rect area)
        {
            var prev = GUI.color;
            GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.90f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = new Color(0.95f, 0.78f, 0.28f, 0.95f);
            GUI.DrawTexture(new Rect(area.x, area.y, area.width, 3f), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.08f);
            GUI.DrawTexture(new Rect(area.x, area.yMax - 1f, area.width, 1f), Texture2D.whiteTexture);
            GUI.color = prev;
        }

        void EnsureStyles()
        {
            if (stylesReady)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
            };
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.78f, 0.80f, 0.74f) },
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.92f, 0.93f, 0.88f) },
            };
            deltaStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.93f, 0.55f) },
            };
            stylesReady = true;
        }
    }
}
