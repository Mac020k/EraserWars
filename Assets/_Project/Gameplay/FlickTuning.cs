using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    [CreateAssetMenu(menuName = "EraserWars/Flick Tuning", fileName = "FlickTuning")]
    public sealed class FlickTuning : ScriptableObject
    {
        public float JMax = 0.24f;
        public float Amplitude0 = FlickMath.DefaultSwayAmplitude;
        public float Frequency0 = FlickMath.DefaultSwayFrequency;
        public float FrequencySlope = FlickMath.DefaultSwayFrequencySlope;
        public float SpinGain = 1f;
    }
}
