using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Keshipin.Editor
{
    [InitializeOnLoad]
    public static class AutoRunClassroomBuild
    {
        const string ScenePath = "Assets/Scenes/Classroom_Casual.unity";
        const string FlagKey = "Keshipin.AutoRunClassroomBuild.Done";

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

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunOnce;
                return;
            }

            if (File.Exists(Path.GetFullPath(ScenePath)))
            {
                SessionState.SetBool(FlagKey, true);
                return;
            }

            var ok = EditorApplication.ExecuteMenuItem("Keshipin/Build Classroom Casual Scene");
            if (ok || File.Exists(Path.GetFullPath(ScenePath)))
            {
                SessionState.SetBool(FlagKey, true);
                Debug.Log("[Keshipin] Auto-built Classroom Casual Scene.");
            }
            else
            {
                Debug.LogWarning("[Keshipin] Auto-build menu item failed; will retry.");
                EditorApplication.delayCall += RunOnce;
            }
        }
    }
}
