using UnityEngine;

namespace EraserWars.Gameplay
{
    public enum PhysicsScalePreset
    {
        OneMeter = 0,
        TenCentimeters = 1,
        OneCentimeter = 2,
    }

    public sealed class PhysicsScaleProbe : MonoBehaviour
    {
        [SerializeField] PhysicsScalePreset preset = PhysicsScalePreset.TenCentimeters;
        [SerializeField] Transform desk;
        [SerializeField] Transform eraser;

        public void ApplyPreset(PhysicsScalePreset value)
        {
            preset = value;
            var unitsPerMeter = value switch
            {
                PhysicsScalePreset.OneMeter => 1f,
                PhysicsScalePreset.TenCentimeters => 10f,
                PhysicsScalePreset.OneCentimeter => 100f,
                _ => 10f,
            };

            Physics.gravity = new Vector3(0f, -9.81f * unitsPerMeter, 0f);
            Time.fixedDeltaTime = 0.01f;

            if (desk != null)
            {
                desk.localScale = new Vector3(0.6f * unitsPerMeter, 0.04f * unitsPerMeter, 0.4f * unitsPerMeter);
            }

            if (eraser != null)
            {
                eraser.localScale = new Vector3(0.043f * unitsPerMeter, 0.011f * unitsPerMeter, 0.017f * unitsPerMeter);
            }
        }

        void Start()
        {
            ApplyPreset(preset);
        }
    }
}
