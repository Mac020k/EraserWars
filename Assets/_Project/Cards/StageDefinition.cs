using UnityEngine;

namespace EraserWars.Cards
{
    [CreateAssetMenu(menuName = "EraserWars/Stage", fileName = "Stage_")]
    public sealed class StageDefinition : ScriptableObject
    {
        public string NameKey;
        public string SceneName;
        public bool AllowObstacles;
        public int RecommendedPlayers = 4;
    }
}
