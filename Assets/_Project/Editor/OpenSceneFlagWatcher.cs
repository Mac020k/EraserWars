using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
        const string FlagRebuildFishNet = "Temp/rebuild-fishnet-spike.flag";
        const string FlagRebuildClassroom = "Temp/rebuild-classroom.flag";

        static OpenSceneFlagWatcher()
        {
            EditorApplication.update += PollFlags;
        }

        static void PollFlags()
        {
            TryStopPlay();
            TryRebuild();
            TryOpen(FlagFishNet, "Assets/Scenes/FishNet_Spike.unity");
            TryOpen(FlagClassroom, "Assets/Scenes/Classroom_Casual.unity");
            TryEnterPlay();
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

        static void TryEnterPlay()
        {
            if (!ConsumeFlag(FlagEnterPlay))
            {
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
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

            // Agent flags must not block on "Save Untitled?" dialogs.
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[EraserWars] Opened {scenePath} via flag {flagRelative}");
        }
    }
}
