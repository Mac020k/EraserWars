using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class RuleSetSwitch : MonoBehaviour
    {
        public bool UsePointsTrial;

        public Keshipin.Core.RuleSet Current =>
            UsePointsTrial ? Keshipin.Core.RuleSet.PointsTrial : Keshipin.Core.RuleSet.CasualStock;
    }
}
