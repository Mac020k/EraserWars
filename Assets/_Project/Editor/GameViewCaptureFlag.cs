using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Editor
{
    /// <summary>
    /// Drop Temp/capture-game.flag (body = output PNG path, optional) to save the Game view and log
    /// what paints it: cameras and every UIDocument's panel clear settings.
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewCaptureFlag
    {
        const string Flag = "Temp/capture-game.flag";
        const string DefaultOutput = "Temp/capture-game.png";

        static GameViewCaptureFlag()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var flagPath = Path.Combine(projectRoot, Flag);
            if (!File.Exists(flagPath) || !EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                return;
            }

            var body = File.ReadAllText(flagPath).Trim();
            File.Delete(flagPath);
            var output = Path.GetFullPath(Path.Combine(projectRoot, body.Length > 0 ? body : DefaultOutput));
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);

            foreach (var cam in Camera.allCameras)
            {
                Debug.Log(
                    $"[EraserWars] capture-game: camera '{cam.name}' enabled={cam.enabled} clear={cam.clearFlags} " +
                    $"depth={cam.depth} pos={cam.transform.position} target={(cam.targetTexture != null ? cam.targetTexture.name : "screen")}");
            }

            foreach (var doc in Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include))
            {
                var ps = doc.panelSettings;
                Debug.Log(
                    $"[EraserWars] capture-game: doc '{doc.name}' active={doc.gameObject.activeInHierarchy} enabled={doc.enabled} " +
                    $"sort={doc.sortingOrder} display={doc.rootVisualElement?.resolvedStyle.display} " +
                    $"panel={(ps != null ? ps.name : "(none)")} clearColor={ps?.clearColor} colorClearValue={ps?.colorClearValue}");
            }

            EditorApplication.isPaused = false;
            ScreenCapture.CaptureScreenshot(output);
            EditorApplication.QueuePlayerLoopUpdate();
            Debug.Log($"[EraserWars] capture-game: queued {output}");
        }
    }
}
