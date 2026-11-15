using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class ClassroomBootstrap : MonoBehaviour
    {
        [SerializeField] MatchDirector director;
        [SerializeField] int players = 2;
        [SerializeField] bool autoStartHotseat;

        void Start()
        {
            if (!autoStartHotseat || director == null)
            {
                return;
            }

            director.StartHotseat(players);
        }
    }
}
