using System;

namespace Keshipin.Core
{
    public static class FlickMath
    {
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

        public static (float x, float y) DirectionFromAngle(float radians)
        {
            return (MathF.Cos(radians), MathF.Sin(radians));
        }
    }
}
