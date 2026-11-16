using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class FlickApplier : MonoBehaviour
    {
        [SerializeField] FlickTuning tuning;
        [SerializeField] float jMax = 0.24f;
        [SerializeField] float amplitude0 = FlickMath.DefaultSwayAmplitude;
        [SerializeField] float frequency0 = FlickMath.DefaultSwayFrequency;
        [SerializeField] float frequencySlope = FlickMath.DefaultSwayFrequencySlope;
        [SerializeField] float spinGain = 1f;

        public FlickTuning Tuning
        {
            get => tuning;
            set => tuning = value;
        }

        public void Apply(EraserBody eraser, FlickCommand command, float powerMultiplier = 1f)
        {
            Read(out var resolvedJMax, out var a0, out var f0, out var f1, out var gain);
            var (dx, dy) = FlickMath.SwayedDirection(
                command.DirectionX,
                command.DirectionY,
                command.DragLength01,
                command.ReleaseTime,
                eraser.SizeFactor,
                a0,
                f0,
                f1);
            var power = eraser.Power * Mathf.Max(0.05f, powerMultiplier);
            var impulseMag = FlickMath.Impulse(power, resolvedJMax, command.DragLength01);
            var impulse = new Vector3(dx, 0f, dy) * impulseMag;

            var box = eraser.GetComponent<BoxCollider>();
            var space = box != null ? box.transform : eraser.transform;
            var half = box != null ? box.size * 0.5f : Vector3.one * 0.5f;
            var center = box != null ? box.center : Vector3.zero;
            var local = FlickMath.LocalContact(
                command.ContactOffsetX,
                command.ContactOffsetY,
                half.x,
                half.z);
            var onComPlane = new Vector3(center.x + local.x, eraser.Body.centerOfMass.y, center.z + local.z);
            var worldContact = space.TransformPoint(onComPlane);
            var com = space.TransformPoint(eraser.Body.centerOfMass);
            var lever = worldContact - com;
            var yaw = FlickMath.YawImpulse(lever.x, lever.z, impulse.x, impulse.z, gain);
            var geometric = gain == 1f
                ? yaw
                : FlickMath.YawImpulse(lever.x, lever.z, impulse.x, impulse.z, 1f);
            eraser.ApplyFlick(impulse, worldContact, yaw - geometric);
        }

        void Read(out float resolvedJMax, out float a0, out float f0, out float f1, out float gain)
        {
            if (tuning != null)
            {
                resolvedJMax = tuning.JMax;
                a0 = tuning.Amplitude0;
                f0 = tuning.Frequency0;
                f1 = tuning.FrequencySlope;
                gain = tuning.SpinGain;
                return;
            }

            resolvedJMax = jMax;
            a0 = amplitude0;
            f0 = frequency0;
            f1 = frequencySlope;
            gain = spinGain;
        }
    }
}
