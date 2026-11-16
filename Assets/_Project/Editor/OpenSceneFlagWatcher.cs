using System.IO;
using EraserWars.Cards;
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
        const string FlagKnockOut = "Temp/knock-out.flag";
        const string FlagRematch = "Temp/rematch.flag";
        const string FlagRebuildFishNet = "Temp/rebuild-fishnet-spike.flag";
        const string FlagRebuildClassroom = "Temp/rebuild-classroom.flag";
        const string FlagImportContent = "Temp/import-content.flag";
        const string FlagOpenCollection = "Temp/open-collection.flag";
        const string FlagRefreshAssets = "Temp/refresh-assets.flag";
        const string FlagPlayFirstCard = "Temp/play-first-card.flag";
        const string FlagForceSettle = "Temp/force-settle.flag";

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
            TryRefreshAssets();
            TryRebuild();
            TryImportContent();
            TryOpen(FlagFishNet, "Assets/Scenes/FishNet_Spike.unity");
            TryOpen(FlagClassroom, "Assets/Scenes/Classroom_Casual.unity");
            TryEnterPlay();
            TryStartHotseat2();
            TryFlickCurrent();
            TryKnockOut();
            TryRematch();
            TryOpenCollection();
            TryPlayFirstCard();
            TryForceSettle();
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

        static void TryKnockOut()
        {
            if (!PeekFlag(FlagKnockOut))
            {
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagKnockOut))
            {
                return;
            }

            ForceUnpausedPlay();
            var director = Object.FindAnyObjectByType<MatchDirector>();
            if (director?.State == null)
            {
                Debug.LogWarning("[EraserWars] knock-out: no match");
                return;
            }

            // Knock out the non-active seat so the current player can win after settle.
            var victim = director.State.CurrentPlayerId.Value == 0 ? 1 : 0;
            if (director.State.Phase != Phase.Simulating)
            {
                director.SubmitFlick(new FlickCommand(
                    director.State.CurrentPlayerId, 0f, 1f, 0.35f, 0f, 0f, 0.1f));
            }

            var stocks = director.State.GetPlayer(new PlayerId(victim)).Stocks;
            for (var i = 0; i < Mathf.Max(1, stocks); i++)
            {
                director.DebugKnockOut(victim);
            }

            director.DebugSettle();
            Object.FindAnyObjectByType<MatchHudController>()?.RefreshFromState();
            Debug.Log(
                $"[EraserWars] knock-out: victim=P{victim + 1} phase={director.State.Phase} winner={director.State.Winner}");
        }

        static void TryRematch()
        {
            if (!ConsumeFlag(FlagRematch))
            {
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            ForceUnpausedPlay();
            var director = Object.FindAnyObjectByType<MatchDirector>();
            if (director == null)
            {
                Debug.LogWarning("[EraserWars] rematch: MatchDirector missing");
                return;
            }

            director.Rematch();
            Object.FindAnyObjectByType<MatchHudController>()?.RefreshFromState();
            Debug.Log($"[EraserWars] rematch: phase={director.State?.Phase} players={director.State?.Players.Length}");
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

            var scaleBits = "(no-erasers)";
            if (director != null && director.Erasers.Count > 0)
            {
                var parts = new System.Collections.Generic.List<string>();
                for (var i = 0; i < director.Erasers.Count; i++)
                {
                    var e = director.Erasers[i];
                    if (e == null)
                    {
                        continue;
                    }

                    var s = e.transform.localScale;
                    parts.Add($"P{e.PlayerIndex + 1}=({s.x:0.000},{s.y:0.000},{s.z:0.000})");
                }

                scaleBits = string.Join(" ", parts);
            }

            var rosterBits = "(no-defs)";
            if (director != null && director.Erasers.Count > 0)
            {
                var names = new System.Collections.Generic.List<string>();
                for (var i = 0; i < director.Erasers.Count; i++)
                {
                    var e = director.Erasers[i];
                    if (e == null)
                    {
                        continue;
                    }

                    var def = e.Definition != null ? e.Definition.NameKey : "default";
                    names.Add($"P{e.PlayerIndex + 1}:{def}:m={e.Body.mass:0.000}");
                }

                rosterBits = string.Join(" ", names);
            }

            Debug.Log(
                $"[EraserWars] dump-ui: match phase={director?.State?.Phase} players={director?.State?.Players.Length} turnLeft={director?.State?.TurnTimeLeft:0.0} frame={Time.frameCount} | {hudBits} | {scaleBits} | {rosterBits}");
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
            // If the player loop is wedged, Step once so timers/HUD can move.
            if (heartbeatLastFrame >= 0 && Time.frameCount == heartbeatLastFrame)
            {
                EditorApplication.Step();
                EditorApplication.isPaused = false;
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

        static void TryRefreshAssets()
        {
            if (!ConsumeFlag(FlagRefreshAssets))
            {
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("[EraserWars] refresh-assets: AssetDatabase.Refresh done");
        }

        static void TryForceSettle()
        {
            if (!PeekFlag(FlagForceSettle))
            {
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagForceSettle))
            {
                return;
            }

            ForceUnpausedPlay();
            var director = Object.FindAnyObjectByType<MatchDirector>();
            if (director == null)
            {
                Debug.LogWarning("[EraserWars] force-settle: no director");
                return;
            }

            director.DebugSettle();
            Debug.Log($"[EraserWars] force-settle: phase={director.State?.Phase}");
        }

        static void TryPlayFirstCard()
        {
            if (!PeekFlag(FlagPlayFirstCard))
            {
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagPlayFirstCard))
            {
                return;
            }

            ForceUnpausedPlay();
            var director = Object.FindAnyObjectByType<MatchDirector>();
            if (director?.State == null || director.State.Players.Length == 0)
            {
                Debug.LogWarning("[EraserWars] play-first-card: no match");
                return;
            }

            var hand = director.State.Players[director.State.CurrentPlayerId.Value].Hand;
            if (hand.Length == 0)
            {
                Debug.LogWarning("[EraserWars] play-first-card: empty hand");
                return;
            }

            var id = hand[0].Value;
            director.PlayCard(id);
            Debug.Log(
                $"[EraserWars] play-first-card: played={id} phase={director.State.Phase} handLeft={director.State.Players[director.State.CurrentPlayerId.Value].Hand.Length} effects={director.State.Effects.Length}");
        }

        static void TryImportContent()
        {
            if (!PeekFlag(FlagImportContent))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagImportContent))
            {
                return;
            }

            RosterCsvImporter.ImportAll();
            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(ContentCatalogBuilder.CatalogPath);
            Debug.Log(
                $"[EraserWars] import-content: cards={catalog?.Cards?.Count ?? 0} characters={catalog?.Characters?.Count ?? 0} erasers={catalog?.Erasers?.Count ?? 0}");
        }

        static void TryOpenCollection()
        {
            if (!PeekFlag(FlagOpenCollection))
            {
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            if (!ConsumeFlag(FlagOpenCollection))
            {
                return;
            }

            ForceUnpausedPlay();
            var menu = Object.FindAnyObjectByType<StartMenuController>(FindObjectsInactive.Include);
            if (menu == null)
            {
                Debug.LogWarning("[EraserWars] open-collection: StartMenuController missing");
                return;
            }

            if (!menu.gameObject.activeSelf)
            {
                menu.ShowMenu(suspendMatch: menu.GetComponentInParent<Transform>() != null);
            }

            menu.gameObject.SetActive(true);
            menu.ShowMenu(suspendMatch: false);
            menu.OpenCollection();

            var catalog = Resources.Load<ContentCatalog>("EraserWars/ContentCatalog");
            var panel = menu.GetComponent<UIDocument>()?.rootVisualElement?.Q("collection-panel");
            var hidden = panel == null || panel.ClassListContains("hidden");
            var listCount = panel?.Q<ScrollView>("collection-list")?.childCount ?? -1;
            Debug.Log(
                $"[EraserWars] open-collection: catalogCards={catalog?.Cards?.Count ?? 0} catalogChars={catalog?.Characters?.Count ?? 0} panelHidden={hidden} listChildren={listCount}");
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
