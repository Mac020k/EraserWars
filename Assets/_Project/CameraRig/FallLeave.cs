using UnityEngine;

namespace EraserWars.CameraRig
{
    public readonly struct FallLeave
    {
        public const float DefaultSeconds = 0.45f;
        public const float MaxSeconds = 0.75f;

        public Vector3 Focus { get; }
        public float UntilUnscaled { get; }

        public FallLeave(Vector3 focus, float untilUnscaled)
        {
            Focus = focus;
            UntilUnscaled = untilUnscaled;
        }

        public static FallLeave Begin(Vector3 focus, float nowUnscaled, float requestedSeconds)
        {
            return new FallLeave(focus, nowUnscaled + Duration(requestedSeconds));
        }

        internal static float Duration(float requestedSeconds)
        {
            return Mathf.Clamp(requestedSeconds, 0.05f, MaxSeconds);
        }

        public bool Active(float nowUnscaled) => nowUnscaled < UntilUnscaled;
    }
}
