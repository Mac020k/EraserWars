using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EraserWars.Editor
{
    [InitializeOnLoad]
    public static class AutoRunClassroomBuild
    {
        const string ScenePath = "Assets/Scenes/Classroom_Casual.unity";
        const string FlagKey = "EraserWars.AutoRunClassroomBuild.Done";

        static AutoRunClassroomBuild()
        {
            EditorApplication.delayCall += RunOnce;
        }

        static void RunOnce()
        {
            if (SessionState.GetBool(FlagKey, false))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunOnce;
                return;
            }

            if (File.Exists(Path.GetFullPath(ScenePath)))
            {
                SessionState.SetBool(FlagKey, true);
                return;
            }

            var ok = EditorApplication.ExecuteMenuItem("EraserWars/Build Classroom Casual Scene");
            if (ok || File.Exists(Path.GetFullPath(ScenePath)))
            {
                SessionState.SetBool(FlagKey, true);
                Debug.Log("[EraserWars] Auto-built Classroom Casual Scene.");
            }
            else
            {
                Debug.LogWarning("[EraserWars] Auto-build menu item failed; will retry.");
                EditorApplication.delayCall += RunOnce;
            }
        }
    }
}
