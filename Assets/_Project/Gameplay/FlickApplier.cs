using Keshipin.Core;
using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class FlickApplier : MonoBehaviour
    {
        [SerializeField] float jMax = 0.12f;
        [SerializeField] float amplitude0 = 0.35f;
        [SerializeField] float frequency0 = 0.8f;
        [SerializeField] float frequencySlope = 2.5f;

        public void Apply(EraserBody eraser, FlickCommand command)
        {
            var aim = Mathf.Atan2(command.DirectionY, command.DirectionX);
            var swayed = FlickMath.AimAngleWithSway(
                aim,
                command.DragLength01,
                command.ReleaseTime,
                eraser.SizeFactor,
                amplitude0,
                frequency0,
                frequencySlope);
            var (dx, dy) = FlickMath.DirectionFromAngle(swayed);
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
