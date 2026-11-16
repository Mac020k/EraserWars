using UnityEngine;

namespace EraserWars.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public sealed class DeskDragZone : MonoBehaviour
    {
        [SerializeField] float dragPerSecond = 8f;

        public float DragPerSecond
        {
            get => dragPerSecond;
            set => dragPerSecond = value;
        }

        void OnTriggerStay(Collider other)
        {
            var body = other.attachedRigidbody;
            if (body == null || body.isKinematic || body.GetComponent<EraserBody>() == null)
            {
                return;
            }

            var keep = Mathf.Exp(-dragPerSecond * Time.fixedDeltaTime);
            var v = body.linearVelocity;
            body.linearVelocity = new Vector3(v.x * keep, v.y, v.z * keep);
        }
    }
}
