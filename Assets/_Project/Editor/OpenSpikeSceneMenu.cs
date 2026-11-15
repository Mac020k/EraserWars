using UnityEditor;
using UnityEditor.SceneManagement;

namespace EraserWars.Editor
{
    public static class OpenSpikeSceneMenu
    {
        const string FishNetSpikePath = "Assets/Scenes/FishNet_Spike.unity";
        const string ClassroomPath = "Assets/Scenes/Classroom_Casual.unity";

        [MenuItem("EraserWars/Open Fish-Net Spike Scene")]
        public static void OpenFishNetSpike()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(FishNetSpikePath);
        }

        [MenuItem("EraserWars/Open Classroom Casual Scene")]
        public static void OpenClassroomCasual()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ClassroomPath);
        }
    }
}
