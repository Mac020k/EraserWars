using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class ClassroomBootstrap : MonoBehaviour
    {
        [SerializeField] MatchDirector director;
        [SerializeField] int players = 2;

        void Start()
        {
            if (director != null)
            {
                director.StartHotseat(players);
            }
        }
    }
}
