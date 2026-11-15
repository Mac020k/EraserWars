using System.Collections;
using System.IO;
using System.Text;
using FishNet.Managing;
using UnityEngine;

namespace EraserWars.Net
{
    /// <summary>
    /// Hands-free Host/Client smoke: Editor starts Host + canned flick; player build starts Client.
    /// Writes pose delta to Temp/fishnet-dual-smoke-result.txt for the dual-instance record.
    /// </summary>
    public sealed class FishNetSpikeAutoSmoke : MonoBehaviour
    {
        [SerializeField] NetworkManager networkManager;
        [SerializeField] FishNetSpikeSession session;
        [SerializeField] bool autoHostInEditor = true;
        [SerializeField] bool autoClientInPlayer = true;
        [SerializeField] float settleTimeoutSeconds = 8f;

        void Awake()
        {
            if (networkManager == null)
            {
                networkManager = FindAnyObjectByType<NetworkManager>();
            }

            if (session == null)
            {
                session = FindAnyObjectByType<FishNetSpikeSession>();
            }
        }

        IEnumerator Start()
        {
            WriteResult("boot", $"editor={Application.isEditor} autoHost={autoHostInEditor} autoClient={autoClientInPlayer}");
            yield return null;
            if (networkManager == null)
            {
                networkManager = FindAnyObjectByType<NetworkManager>();
            }

            if (session == null)
            {
                session = FindAnyObjectByType<FishNetSpikeSession>();
            }

            if (networkManager == null || session == null)
            {
                WriteResult("error", "missing NetworkManager or FishNetSpikeSession");
                yield break;
            }

            if (Application.isEditor)
            {
                if (!autoHostInEditor)
                {
                    WriteResult("skip", "autoHostInEditor=false");
                    yield break;
                }

                yield return RunHostSmoke();
            }
            else if (autoClientInPlayer)
            {
                yield return RunClientSmoke();
            }
            else
            {
                WriteResult("skip", "autoClientInPlayer=false");
            }
        }

        IEnumerator RunHostSmoke()
        {
            Debug.Log("[FishNetAutoSmoke] Starting Host...");
            networkManager.ServerManager.StartConnection();
            networkManager.ClientManager.StartConnection();
            yield return new WaitForSecondsRealtime(0.75f);
            session.UiRequestCannedFlick();
            var elapsed = 0f;
            while (elapsed < settleTimeoutSeconds && !session.HasReportedPose)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            var delta = session.PoseDeltaMeters;
            var status = session.HasReportedPose
                ? $"host-ok delta={delta:F4} status={session.LastStatus}"
                : $"host-timeout status={session.LastStatus}";
            WriteResult(session.HasReportedPose ? "host-settled" : "host-timeout", status);
            Debug.Log($"[FishNetAutoSmoke] {status}");
        }

        IEnumerator RunClientSmoke()
        {
            Debug.Log("[FishNetAutoSmoke] Starting Client...");
            networkManager.ClientManager.StartConnection();
            var elapsed = 0f;
            while (elapsed < settleTimeoutSeconds + 4f && !session.HasReportedPose)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Give NetworkTransform a moment after the observers report.
            yield return new WaitForSecondsRealtime(1.25f);
            var delta = session.PoseDeltaMeters;
            var status = session.HasReportedPose
                ? $"client-ok Local vs host delta={delta:F4} m status={session.LastStatus}"
                : $"client-timeout status={session.LastStatus}";
            WriteResult(session.HasReportedPose ? "client-settled" : "client-timeout", status);
            Debug.Log($"[FishNetAutoSmoke] {status}");
        }

        static void WriteResult(string phase, string detail)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"phase={phase}");
            sb.AppendLine($"detail={detail}");
            sb.AppendLine($"utc={System.DateTime.UtcNow:O}");
            var body = sb.ToString();
            var fileName = Application.isEditor
                ? "fishnet-dual-smoke-host.txt"
                : "fishnet-dual-smoke-client.txt";

            foreach (var dir in ResolveOutputDirs())
            {
                try
                {
                    Directory.CreateDirectory(dir);
                    File.WriteAllText(Path.Combine(dir, fileName), body);
                    Debug.Log($"[FishNetAutoSmoke] wrote {dir}/{fileName} ({phase})");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[FishNetAutoSmoke] write failed ({dir}): {ex.Message}");
                }
            }
        }

        static string[] ResolveOutputDirs()
        {
            if (Application.isEditor)
            {
                return new[]
                {
                    Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp")),
                };
            }

            return new[]
            {
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "Temp")),
                Application.persistentDataPath,
            };
        }
    }
}
