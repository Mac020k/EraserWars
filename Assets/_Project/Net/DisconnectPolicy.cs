using UnityEngine;

namespace Keshipin.Net
{
    /// <summary>Disconnect handling: wait 30s then force-skip each turn.</summary>
    public sealed class DisconnectPolicy
    {
        public const float WaitSeconds = 30f;
    }
}
