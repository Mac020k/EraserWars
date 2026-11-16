using UnityEngine;

namespace EraserWars.Gameplay
{
    public readonly struct EdgeSlowmoCue
    {
        public const string ReduceMotionPref = "eraserwars.reduceMotion";
        public const float AttackSeconds = 0.05f;
        public const float FloorAlpha = 0.36f;
        public const float PeakAlpha = 0.84f;
        public const float ReduceMotionAlpha = 0.22f;

        public float EdgeAlpha { get; }

        EdgeSlowmoCue(float edgeAlpha)
        {
            EdgeAlpha = edgeAlpha;
        }

        public static EdgeSlowmoCue Idle => new(0f);

        public static bool ReduceMotionPreferred()
        {
            return PlayerPrefs.HasKey(ReduceMotionPref)
                && PlayerPrefs.GetInt(ReduceMotionPref, 0) != 0;
        }

        public static EdgeSlowmoCue At(bool slowing, float elapsedUnscaled, bool reduceMotion)
        {
            if (!slowing)
            {
                return Idle;
            }

            if (reduceMotion)
            {
                return new EdgeSlowmoCue(ReduceMotionAlpha);
            }

            var attack = Mathf.Clamp01(elapsedUnscaled / AttackSeconds);
            return new EdgeSlowmoCue(Mathf.Lerp(FloorAlpha, PeakAlpha, attack));
        }
    }
}
