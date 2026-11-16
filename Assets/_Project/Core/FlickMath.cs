using System;

namespace EraserWars.Core
{
    public readonly struct FlickAimSample
    {
        public FlickAimSample(
            float lengthMeters,
            float halfAngleRadians,
            float centerX,
            float centerY,
            float releaseX,
            float releaseY,
            float leftX,
            float leftY,
            float rightX,
            float rightY)
        {
            LengthMeters = lengthMeters;
            HalfAngleRadians = halfAngleRadians;
            CenterX = centerX;
            CenterY = centerY;
            ReleaseX = releaseX;
            ReleaseY = releaseY;
            LeftX = leftX;
            LeftY = leftY;
            RightX = rightX;
            RightY = rightY;
        }

        public float LengthMeters { get; }
        public float HalfAngleRadians { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float ReleaseX { get; }
        public float ReleaseY { get; }
        public float LeftX { get; }
        public float LeftY { get; }
        public float RightX { get; }
        public float RightY { get; }
    }

    public static class FlickMath
    {
        public const float DefaultSwayAmplitude = 0.35f;
        public const float DefaultSwayFrequency = 0.8f;
        public const float DefaultSwayFrequencySlope = 2.5f;

        public static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        public static float Impulse(float power, float jMax, float dragLength01)
        {
            var l = Clamp01(dragLength01);
            return power * jMax * MathF.Pow(l, 1.5f);
        }

        public static float AimAngleWithSway(
            float l,
            float t,
            float sizeFactor,
            float a0,
            float f0,
            float f1)
        {
            var length = Clamp01(l);
            var amplitude = a0 * length * length * sizeFactor;
            var frequency = f0 + f1 * length * sizeFactor;
            return amplitude * MathF.Sin(2f * MathF.PI * frequency * t);
        }

        public static float AimAngleWithSway(
            float aimRadians,
            float l,
            float t,
            float sizeFactor,
            float a0,
            float f0,
            float f1)
        {
            return aimRadians + AimAngleWithSway(l, t, sizeFactor, a0, f0, f1);
        }

        public static (float x, float y) SwayedDirection(
            float directionX,
            float directionY,
            float dragLength01,
            float releaseTime,
            float sizeFactor,
            float a0,
            float f0,
            float f1)
        {
            var aim = MathF.Atan2(directionY, directionX);
            var swayed = AimAngleWithSway(aim, dragLength01, releaseTime, sizeFactor, a0, f0, f1);
            return DirectionFromAngle(swayed);
        }

        public static (float x, float y) DirectionFromAngle(float radians)
        {
            return (MathF.Cos(radians), MathF.Sin(radians));
        }

        public static FlickAimSample SampleAim(
            float directionX,
            float directionY,
            float dragLength01,
            float heldSeconds,
            float sizeFactor,
            float metersAtFullDrag,
            float a0,
            float f0,
            float f1)
        {
            var length01 = Clamp01(dragLength01);
            var aim = MathF.Atan2(directionY, directionX);
            var half = a0 * length01 * length01 * sizeFactor;
            var center = DirectionFromAngle(aim);
            var release = SwayedDirection(
                directionX,
                directionY,
                length01,
                heldSeconds,
                sizeFactor,
                a0,
                f0,
                f1);
            var left = DirectionFromAngle(aim + half);
            var right = DirectionFromAngle(aim - half);
            var meters = metersAtFullDrag > 0f ? metersAtFullDrag * length01 : 0f;
            return new FlickAimSample(
                meters,
                half,
                center.x,
                center.y,
                release.x,
                release.y,
                left.x,
                left.y,
                right.x,
                right.y);
        }
    }
}
