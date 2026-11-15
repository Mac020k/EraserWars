using UnityEngine;

namespace EraserWars.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EraserBody : MonoBehaviour
    {
        [SerializeField] Rigidbody body;
        [SerializeField] Transform visualRoot;
        [SerializeField] float mass = 0.02f;
        [SerializeField] float power = 1f;
        [SerializeField] float speed = 0.65f;
        [SerializeField] float maxSpeed = 3.6f;
        [SerializeField] float sizeFactor = 1f;
        [SerializeField] bool enableMotionTrail = true;

        public Rigidbody Body => body;
        public Transform VisualRoot => visualRoot != null ? visualRoot : transform;
        public float Power => power;
        public float SizeFactor => sizeFactor;
        public float MaxSpeed => maxSpeed;
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
            EnsureMotionTrail();
        }

        public void ApplyFlick(Vector3 worldImpulse, Vector3 contactPoint)
        {
            body.WakeUp();
            body.AddForceAtPosition(worldImpulse, contactPoint, ForceMode.Impulse);
            ClampLinearSpeed();
        }

        public void ClampSpeedPublic()
        {
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

        void EnsureMotionTrail()
        {
            if (!enableMotionTrail)
            {
                return;
            }

            var root = VisualRoot;
            var trail = root.GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = root.gameObject.AddComponent<TrailRenderer>();
            }

            trail.time = 0.4f;
            trail.minVertexDistance = 0.002f;
            trail.widthMultiplier = 0.012f;
            trail.emitting = true;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (trail.sharedMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader != null)
                {
                    trail.sharedMaterial = new Material(shader);
                    trail.sharedMaterial.color = new Color(1f, 1f, 1f, 0.55f);
                }
            }
        }

        public bool IsSettled(float linearEpsilon, float angularEpsilon)
        {
            return body.linearVelocity.sqrMagnitude <= linearEpsilon * linearEpsilon
                   && body.angularVelocity.sqrMagnitude <= angularEpsilon * angularEpsilon;
        }
    }
}
