using UnityEngine;

namespace Keshipin.Gameplay
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
