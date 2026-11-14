using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class TrajectoryPreview : MonoBehaviour
    {
        [SerializeField] LineRenderer line;
        [SerializeField] int points = 24;
        [SerializeField] float step = 0.02f;
        [SerializeField] bool enabledInRanked;

        public void Show(Vector3 origin, Vector3 velocity)
        {
            if (line == null)
            {
                return;
            }

            if (!enabledInRanked)
            {
                // Ranked disables preview via MatchDirector / RuleSet.AimAssist.
            }

            line.positionCount = points;
            var p = origin;
            var v = velocity;
            for (var i = 0; i < points; i++)
            {
                line.SetPosition(i, p);
                v += Physics.gravity * step;
                p += v * step;
            }
        }

        public void Clear()
        {
            if (line != null)
            {
                line.positionCount = 0;
            }
        }
    }
}
