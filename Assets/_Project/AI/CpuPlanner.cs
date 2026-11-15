using EraserWars.Core;
using UnityEngine;

namespace EraserWars.AI
{
    public sealed class CpuPlanner : MonoBehaviour
    {
        [SerializeField] int candidateCount = 12;

        public FlickCommand Plan(PlayerId player, Vector3 selfPos, Vector3 targetPos)
        {
            var best = Vector3.Normalize(targetPos - selfPos);
            var drag = Mathf.Clamp01(0.35f + candidateCount * 0.01f);
            return new FlickCommand(player, best.x, best.z, drag, 0f, 0.2f, 0.15f);
        }
    }
}
