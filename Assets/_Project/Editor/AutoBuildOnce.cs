using System.IO;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    /// <summary>
    /// One-shot scene rebuild when Temp/eraserwars-rebuild.flag appears.
    /// </summary>
    [InitializeOnLoad]
    static class AutoBuildOnce
    {
        const string FlagPath = "Temp/eraserwars-rebuild.flag";

        static AutoBuildOnce()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (!File.Exists(FlagPath))
            {
                return;
            }

            EditorApplication.update -= Poll;
            TryRun();
            EditorApplication.update += Poll;
        }

        static void TryRun()
        {
            if (!File.Exists(FlagPath))
            {
                return;
            }

            try
            {
                File.Delete(FlagPath);
            }
            catch (IOException)
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += () => EditorApplication.delayCall += RunBuild;
                return;
            }

            RunBuild();
        }

        static void RunBuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[EraserWars] AutoBuildOnce deferred; still in Play Mode.");
                EditorApplication.delayCall += RunBuild;
                return;
            }

            Debug.Log("[EraserWars] AutoBuildOnce running ClassroomSceneBuilder.Build()");
            ClassroomSceneBuilder.Build();
        }
    }
}
