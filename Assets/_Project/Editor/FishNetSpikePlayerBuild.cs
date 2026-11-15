using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class FishNetSpikePlayerBuild
    {
        const string ScenePath = "Assets/Scenes/FishNet_Spike.unity";
        const string OutputPath = "Builds/FishNetSpike/EraserWars.exe";

        [MenuItem("EraserWars/Build Fish-Net Spike Windows Player")]
        public static void BuildWindowsPlayer()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[EraserWars] Stop Play Mode before building the Fish-Net spike player.");
                return;
            }

            if (!File.Exists(ScenePath))
            {
                Debug.LogError($"[EraserWars] Missing {ScenePath}. Run EraserWars/Build Fish-Net Spike Scene first.");
                return;
            }

            var dir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[EraserWars] Fish-Net spike player build failed: {report.summary.result}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            Debug.Log($"[EraserWars] Fish-Net spike player built at {OutputPath}");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
    }
}
