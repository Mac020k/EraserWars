using System;

namespace EraserWars.Core
{
    /// <summary>
    /// Momentum exchanged along the contact normal, and the hitstop and camera kick it buys.
    /// TapImpulse is a 0.02 kg eraser leaving at the 2.4 m/s launch floor.
    /// FullImpulse is two of those closing at 6 m/s with restitution 1
    /// (reduced mass times (1 + e) times closing speed).
    /// </summary>
    public readonly struct NormalHit
    {
        public const float TapImpulse = 0.048f;
        public const float FullImpulse = 0.12f;
        public const int MinFrames = 2;
        public const int MaxFrames = 10;
        public const float TapKickMeters = 0.012f;
        public const float FullKickMeters = 0.05f;

        public float Impulse { get; }
        public float Sign { get; }
        public bool LaunchIsSecond { get; }

        public NormalHit(float impulse, float sign, bool launchIsSecond)
        {
            Impulse = impulse;
            Sign = sign;
            LaunchIsSecond = launchIsSecond;
        }

        public static NormalHit FromSpeeds(
            float mass1,
            float before1,
            float after1,
            float mass2,
            float before2,
            float after2)
        {
            var delta1 = after1 - before1;
            var delta2 = after2 - before2;
            var change1 = MathF.Abs(mass1 * delta1);
            var change2 = MathF.Abs(mass2 * delta2);
            var launchIsSecond = change2 >= change1;
            var delta = launchIsSecond ? delta2 : delta1;
            var sign = 1f;
            if (delta < 0f)
            {
                sign = -1f;
            }

            return new NormalHit(MathF.Max(change1, change2), sign, launchIsSecond);
        }

        public float Strength
        {
            get
            {
                var span = FullImpulse - TapImpulse;
                if (span <= 0f)
                {
                    return 0f;
                }

                var t = (Impulse - TapImpulse) / span;
                if (t < 0f)
                {
                    return 0f;
                }

                if (t > 1f)
                {
                    return 1f;
                }

                return t;
            }
        }

        public int Frames =>
            (int)Math.Round(
                MinFrames + (MaxFrames - MinFrames) * Strength,
                MidpointRounding.AwayFromZero);

        public float KickMeters(bool cameraShake)
        {
            if (!cameraShake || Impulse <= 0f)
            {
                return 0f;
            }

            return TapKickMeters + (FullKickMeters - TapKickMeters) * Strength;
        }
    }
}
