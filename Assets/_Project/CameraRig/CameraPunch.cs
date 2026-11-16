using UnityEngine;

namespace EraserWars.CameraRig
{
    public readonly struct CameraPunch
    {
        public const float MinDistanceScale = 0.85f;
        public const float MaxDistanceScale = 1.6f;

        public Vector3 Direction { get; }
        public float Meters { get; }
        public float StartedUnscaled { get; }
        public float Duration { get; }

        public CameraPunch(Vector3 direction, float meters, float startedUnscaled, float duration)
        {
            Direction = direction;
            Meters = meters;
            StartedUnscaled = startedUnscaled;
            Duration = duration;
        }

        public static float DistanceScale(float cameraDistance, float referenceDistance)
        {
            if (referenceDistance <= 0f)
            {
                return 1f;
            }

            var scale = cameraDistance / referenceDistance;
            if (scale < MinDistanceScale)
            {
                return MinDistanceScale;
            }

            if (scale > MaxDistanceScale)
            {
                return MaxDistanceScale;
            }

            return scale;
        }

        public static CameraPunch Begin(Vector3 worldDirection, float meters, float nowUnscaled, float duration)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 1e-8f)
            {
                worldDirection = Vector3.zero;
            }
            else
            {
                worldDirection.Normalize();
            }

            if (meters < 0f)
            {
                meters = 0f;
            }

            if (duration < 0f)
            {
                duration = 0f;
            }

            return new CameraPunch(worldDirection, meters, nowUnscaled, duration);
        }

        public Vector3 OffsetAt(float nowUnscaled)
        {
            if (Meters <= 0f || Duration <= 0f || Direction.sqrMagnitude < 1e-8f)
            {
                return Vector3.zero;
            }

            var elapsed = nowUnscaled - StartedUnscaled;
            if (elapsed < 0f || elapsed >= Duration)
            {
                return Vector3.zero;
            }

            var remain = 1f - elapsed / Duration;
            return Direction * (Meters * remain * remain);
        }
    }
}
