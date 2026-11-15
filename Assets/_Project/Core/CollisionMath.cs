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

        /// <summary>
        /// Raise a weak approach to <paramref name="minClosing"/> so short-range taps still transfer.
        /// Speeds the body with the stronger approach (more negative along n) into the other.
        /// </summary>
        public static void EnsureMinimumClosing(
            ref float normalSpeed1,
            ref float normalSpeed2,
            float minClosing)
        {
            if (minClosing <= 0f)
            {
                return;
            }

            var closing = ClosingSpeed(normalSpeed1, normalSpeed2);
            if (closing >= minClosing)
            {
                return;
            }

            var deficit = minClosing - closing;
            // Body 1 approaches when its normal speed is lower. Push it further into the contact.
            if (normalSpeed1 <= normalSpeed2)
            {
                normalSpeed1 -= deficit;
            }
            else
            {
                normalSpeed2 -= deficit;
            }
        }

        /// <summary>
        /// After resolve, guarantee the launched body leaves at least <paramref name="minLaunch"/> along -n from the hitter.
        /// </summary>
        public static void EnsureMinimumLaunch(
            float mass1,
            float mass2,
            float normalSpeed1Before,
            float normalSpeed2Before,
            ref float resolved1,
            ref float resolved2,
            float minLaunch)
        {
            if (minLaunch <= 0f)
            {
                return;
            }

            // The launched body is the one that gained outbound motion away from the approacher.
            // If body1 was approaching (lower n-speed), body2 should leave with more negative n-speed.
            var body1WasApproaching = normalSpeed1Before < normalSpeed2Before;
            if (body1WasApproaching)
            {
                if (resolved2 > -minLaunch)
                {
                    var bump = -minLaunch - resolved2;
                    resolved2 += bump;
                    resolved1 -= bump * (mass2 / MathF.Max(1e-5f, mass1));
                }
            }
            else if (resolved1 > -minLaunch)
            {
                var bump = -minLaunch - resolved1;
                resolved1 += bump;
                resolved2 -= bump * (mass1 / MathF.Max(1e-5f, mass2));
            }
        }
    }
}
