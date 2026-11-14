using UnityEngine;

namespace Keshipin.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EraserBody : MonoBehaviour
    {
        [SerializeField] Rigidbody body;
        [SerializeField] Transform visualRoot;
        [SerializeField] float mass = 0.02f;
        [SerializeField] float power = 1f;
        [SerializeField] float speed = 0.65f;
        [SerializeField] float maxSpeed = 1.8f;
        [SerializeField] float sizeFactor = 1f;

        public Rigidbody Body => body;
        public Transform VisualRoot => visualRoot != null ? visualRoot : transform;
        public float Power => power;
        public float SizeFactor => sizeFactor;
        public int PlayerIndex { get; set; }

        void Reset()
        {
            body = GetComponent<Rigidbody>();
        }

        void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            body.mass = mass;
            body.linearDamping = Mathf.Max(0.05f, 1.5f / Mathf.Max(0.1f, speed));
            body.angularDamping = 1.2f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void ApplyFlick(Vector3 worldImpulse, Vector3 contactPoint)
        {
            body.WakeUp();
            body.AddForceAtPosition(worldImpulse, contactPoint, ForceMode.Impulse);
            ClampLinearSpeed();
        }

        void ClampLinearSpeed()
        {
            var cap = Mathf.Max(0f, maxSpeed);
            var velocity = body.linearVelocity;
            if (velocity.magnitude <= cap)
            {
                return;
            }

            body.linearVelocity = cap <= 0f ? Vector3.zero : velocity.normalized * cap;
        }

        public bool IsSettled(float linearEpsilon, float angularEpsilon)
        {
            return body.linearVelocity.sqrMagnitude <= linearEpsilon * linearEpsilon
                   && body.angularVelocity.sqrMagnitude <= angularEpsilon * angularEpsilon;
        }
    }
}
