using System;

namespace EraserWars.Core
{
    /// <summary>
    /// 1D collision along a contact normal with restitution.
    /// Formulas: momentum conservation + e = (v2' - v1') / (v1 - v2).
    /// Equal masses and e = 1 exchange normal speeds (striker slows, target flies).
    /// </summary>
    public static class CollisionMath
    {
        public static void ResolveNormalSpeeds(
            float mass1,
            float mass2,
            float normalSpeed1,
            float normalSpeed2,
            float restitution,
            out float resolved1,
            out float resolved2)
        {
            if (mass1 <= 0f || mass2 <= 0f)
            {
                resolved1 = normalSpeed1;
                resolved2 = normalSpeed2;
                return;
            }

            var e = restitution;
            if (e < 0f) e = 0f;
            if (e > 1f) e = 1f;

            var sum = mass1 + mass2;
            resolved1 = (mass1 * normalSpeed1 + mass2 * normalSpeed2 - mass2 * e * (normalSpeed1 - normalSpeed2)) / sum;
            resolved2 = (mass1 * normalSpeed1 + mass2 * normalSpeed2 + mass1 * e * (normalSpeed1 - normalSpeed2)) / sum;
        }

        public static bool IsClosing(float normalSpeed1, float normalSpeed2, float epsilon = 1e-4f)
        {
            return normalSpeed1 - normalSpeed2 < -epsilon;
        }

        public static float ClosingSpeed(float normalSpeed1, float normalSpeed2)
        {
            return MathF.Max(0f, normalSpeed2 - normalSpeed1);
        }
    }
}
