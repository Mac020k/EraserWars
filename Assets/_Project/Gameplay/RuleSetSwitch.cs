using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class RuleSetSwitch : MonoBehaviour
    {
        public bool UsePointsTrial;

        public EraserWars.Core.RuleSet Current =>
            UsePointsTrial ? EraserWars.Core.RuleSet.PointsTrial : EraserWars.Core.RuleSet.CasualStock;
    }
}
