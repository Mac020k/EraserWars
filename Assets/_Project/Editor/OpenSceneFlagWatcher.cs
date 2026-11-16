using System.IO;
using EraserWars.Core;
using EraserWars.Gameplay;
using EraserWars.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Editor
{
    /// <summary>
    /// Opens a scene / toggles Play when an external agent drops a one-shot flag under Temp/.
    /// </summary>
    [InitializeOnLoad]
    public static class OpenSceneFlagWatcher
    {
        const string FlagFishNet = "Temp/open-fishnet-spike.flag";
        const string FlagClassroom = "Temp/open-classroom-casual.flag";
        const string FlagStopPlay = "Temp/stop-play.flag";
        const string FlagEnterPlay = "Temp/enter-play.flag";
        const string FlagUnpause = "Temp/unpause.flag";
        const string FlagStartHotseat2 = "Temp/start-hotseat-2.flag";
        const string FlagFlickCurrent = "Temp/flick-current.flag";
        const string FlagDumpUi = "Temp/dump-ui.flag";
        const string FlagHeartbeat = "Temp/heartbeat.flag";
        const string FlagRebuildFishNet = "Temp/rebuild-fishnet-spike.flag";
        const string FlagRebuildClassroom = "Temp/rebuild-classroom.flag";

        static int heartbeatLeft;
        static int heartbeatLastFrame = -1;

        static OpenSceneFlagWatcher()
        {
            EditorApplication.update += PollFlags;
        }

        static void PollFlags()
        {
            TryStopPlay();
            TryUnpause();
            TryRebuild();
            TryOpen(FlagFishNet, "Assets/Scenes/FishNet_Spike.unity");
            TryOpen(FlagClassroom, "Assets/Scenes/Classroom_Casual.unity");
            TryEnterPlay();
            TryStartHotseat2();
            TryFlickCurrent();
            TryDumpUi();
            TickHeartbeat();
        }

        static void TryStopPlay()
        {
            if (!ConsumeFlag(FlagStopPlay))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }

        static void TryUnpause()
        {
            if (!ConsumeFlag(FlagUnpause))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPaused = false;
            }
        }

        static void TryEnterPlay()
        {
            if (!ConsumeFlag(FlagEnterPlay))
            {
                return;
            }

            ConsoleWindowUtil.ClearErrorPause();
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPaused = false;
                EditorApplication.isPlaying = true;
            }
            else
            {
                EditorApplication.isPaused = false;
            }
        }

        static void TryStartHotseat2()
        {
            if (!PeekFlag(FlagStartHotseat2))
            {
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagStartHotseat2))
            {
                return;
            }

            ForceUnpausedPlay();
            var menu = Object.FindAnyObjectByType<StartMenuController>();
            if (menu != null)
            {
                menu.BeginMatch(2);
                ForceUnpausedPlay();
                Debug.Log($"[EraserWars] start-hotseat-2: BeginMatch(2) paused={EditorApplication.isPaused}");
                return;
            }

            Debug.LogWarning("[EraserWars] start-hotseat-2: StartMenuController missing");
        }

        static void ForceUnpausedPlay()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            ConsoleWindowUtil.ClearErrorPause();
            EditorApplication.isPaused = false;
            EditorApplication.QueuePlayerLoopUpdate();
        }

        static void TryFlickCurrent()
        {
            if (!PeekFlag(FlagFlickCurrent))
            {
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagFlickCurrent))
            {
                return;
            }

            ForceUnpausedPlay();
            var director = Object.FindAnyObjectByType<MatchDirector>();
            if (director == null || director.State == null)
            {
                Debug.LogWarning("[EraserWars] flick-current: MatchDirector/state missing");
                return;
            }

            if (!director.TryGetCurrentEraser(out _))
            {
                Debug.LogWarning("[EraserWars] flick-current: no current eraser");
                return;
            }

            var command = new FlickCommand(
                director.State.CurrentPlayerId,
                0f,
                1f,
                0.85f,
                0f,
                0f,
                0.2f);
            director.SubmitFlick(command);
            ForceUnpausedPlay();
            Debug.Log(
                $"[EraserWars] flick-current: P{director.State.CurrentPlayerId.Value + 1} phase={director.State.Phase} paused={EditorApplication.isPaused}");
        }

        static void TryDumpUi()
        {
            if (!ConsumeFlag(FlagDumpUi))
            {
                return;
            }

            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include);
            Debug.Log(
                $"[EraserWars] dump-ui: {docs.Length} UIDocument(s) playing={EditorApplication.isPlaying} paused={EditorApplication.isPaused}");
            foreach (var doc in docs)
            {
                var go = doc.gameObject;
                var tree = doc.visualTreeAsset != null ? doc.visualTreeAsset.name : "(none)";
                var display = doc.rootVisualElement != null
                    ? doc.rootVisualElement.resolvedStyle.display.ToString()
                    : "(null-root)";
                Debug.Log(
                    $"[EraserWars] dump-ui: '{go.name}' active={go.activeInHierarchy} enabled={doc.enabled} tree={tree} display={display} sort={doc.sortingOrder}");
            }

            var director = Object.FindAnyObjectByType<MatchDirector>();
            var hud = Object.FindAnyObjectByType<MatchHudController>();
            var hudBits = "(no-hud)";
            if (hud != null)
            {
                var hudDoc = hud.GetComponent<UIDocument>();
                var hudRoot = hudDoc?.rootVisualElement?.Q("root") ?? hudDoc?.rootVisualElement;
                var phase = hudRoot?.Q<Label>("phase")?.text;
                var timer = hudRoot?.Q<Label>("timer")?.text;
                var hidden = hudRoot != null && hudRoot.ClassListContains("hidden");
                hudBits =
                    $"phaseLabel='{phase}' timer='{timer}' hiddenClass={hidden} display={hudRoot?.resolvedStyle.display}";
            }

            Debug.Log(
                $"[EraserWars] dump-ui: match phase={director?.State?.Phase} players={director?.State?.Players.Length} turnLeft={director?.State?.TurnTimeLeft:0.0} frame={Time.frameCount} | {hudBits}");
        }

        static void TickHeartbeat()
        {
            if (ConsumeFlag(FlagHeartbeat))
            {
                heartbeatLeft = 90;
                heartbeatLastFrame = -1;
                ForceUnpausedPlay();
                Debug.Log("[EraserWars] heartbeat: armed 90 editor updates");
            }

            if (heartbeatLeft <= 0 || !EditorApplication.isPlaying)
            {
                return;
            }

            heartbeatLeft--;
            ForceUnpausedPlay();
            // Player loop can stall at frame 1 with Enter Play Mode Options; Step advances it.
            if (Time.frameCount <= 1 || Time.frameCount == heartbeatLastFrame)
            {
                EditorApplication.Step();
            }

            var frame = Time.frameCount;
            if (heartbeatLeft % 15 == 0 || frame != heartbeatLastFrame)
            {
                var director = Object.FindAnyObjectByType<MatchDirector>();
                Debug.Log(
                    $"[EraserWars] heartbeat: left={heartbeatLeft} frame={frame} deltaFrame={(heartbeatLastFrame < 0 ? 0 : frame - heartbeatLastFrame)} paused={EditorApplication.isPaused} turnLeft={director?.State?.TurnTimeLeft:0.00} phase={director?.State?.Phase}");
                heartbeatLastFrame = frame;
            }
        }

        static void TryRebuild()
        {
            if (PeekFlag(FlagRebuildFishNet))
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }

                if (ConsumeFlag(FlagRebuildFishNet))
                {
                    FishNetSpikeSceneBuilder.Build();
                }
            }

            if (PeekFlag(FlagRebuildClassroom))
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }

                if (ConsumeFlag(FlagRebuildClassroom))
                {
                    ClassroomSceneBuilder.Build();
                }
            }
        }

        static bool PeekFlag(string flagRelative)
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var flagPath = Path.Combine(root, flagRelative.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(flagPath);
        }

        static bool ConsumeFlag(string flagRelative)
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var flagPath = Path.Combine(root, flagRelative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(flagPath))
            {
                return false;
            }

            try
            {
                File.Delete(flagPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void TryOpen(string flagRelative, string scenePath)
        {
            if (!ConsumeFlag(flagRelative))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[EraserWars] Opened {scenePath} via flag {flagRelative}");
        }
    }

    static class ConsoleWindowUtil
    {
        public static void ClearErrorPause()
        {
            EditorPrefs.SetBool("ConsoleWindow.ErrorPause", false);
            try
            {
                var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
                var setErrorPause = type?.GetMethod(
                    "SetConsoleErrorPause",
                    System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Public);
                setErrorPause?.Invoke(null, new object[] { false });
            }
            catch
            {
                // Best-effort; agent can still ForceUnpausedPlay.
            }
        }
    }
}
