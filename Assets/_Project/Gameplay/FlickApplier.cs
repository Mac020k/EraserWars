using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class FlickApplier : MonoBehaviour
    {
        [SerializeField] float jMax = 0.24f;
        [SerializeField] float amplitude0 = FlickMath.DefaultSwayAmplitude;
        [SerializeField] float frequency0 = FlickMath.DefaultSwayFrequency;
        [SerializeField] float frequencySlope = FlickMath.DefaultSwayFrequencySlope;

        public void Apply(EraserBody eraser, FlickCommand command)
        {
            var (dx, dy) = FlickMath.SwayedDirection(
                command.DirectionX,
                command.DirectionY,
                command.DragLength01,
                command.ReleaseTime,
                eraser.SizeFactor,
                amplitude0,
                frequency0,
                frequencySlope);
            var impulseMag = FlickMath.Impulse(eraser.Power, jMax, command.DragLength01);
            var impulse = new Vector3(dx, 0f, dy) * impulseMag;

            var half = eraser.Body.GetComponent<Collider>() is BoxCollider box
                ? Vector3.Scale(box.size * 0.5f, eraser.transform.lossyScale)
                : Vector3.one * 0.01f;
            var localContact = new Vector3(
                command.ContactOffsetX * half.x,
                half.y,
                command.ContactOffsetY * half.z);
            var worldContact = eraser.transform.TransformPoint(localContact);
            eraser.ApplyFlick(impulse, worldContact);
        }
    }
}
