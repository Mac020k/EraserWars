using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EraserWars.Editor
{
    /// <summary>
    /// One-shot scene rebuild when Temp/eraserwars-rebuild.flag appears.
    /// Temp/eraserwars-recompile.flag forces a domain reload first (needed when Enter Play Mode Options disables reload).
    /// </summary>
    [InitializeOnLoad]
    static class AutoBuildOnce
    {
        const string FlagPath = "Temp/eraserwars-rebuild.flag";
        const string RecompileFlagPath = "Temp/eraserwars-recompile.flag";

        static AutoBuildOnce()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (File.Exists(RecompileFlagPath))
            {
                try
                {
                    File.Delete(RecompileFlagPath);
                }
                catch (IOException)
                {
                    return;
                }

                Debug.Log("[EraserWars] Requesting script compilation before rebuild.");
                CompilationPipeline.RequestScriptCompilation();
                return;
            }

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
