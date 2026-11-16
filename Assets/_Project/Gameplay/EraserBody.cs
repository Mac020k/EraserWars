using EraserWars.Cards;
using UnityEngine;

namespace EraserWars.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EraserBody : MonoBehaviour
    {
        static readonly Vector3 ReferenceSizeMeters = new(0.043f, 0.011f, 0.017f);

        [SerializeField] Rigidbody body;
        [SerializeField] Transform visualRoot;
        [SerializeField] float mass = 0.02f;
        [SerializeField] float power = 1f;
        [SerializeField] float speed = 0.65f;
        [SerializeField] float maxSpeed = 3.6f;
        [SerializeField] float sizeFactor = 1f;
        [SerializeField] bool enableMotionTrail = true;

        Vector3 prefabLocalScale = Vector3.one;
        bool prefabScaleCaptured;

        public Rigidbody Body => body;
        public Transform VisualRoot => visualRoot != null ? visualRoot : transform;
        public float Power => power;
        public float SizeFactor => sizeFactor;
        public float MaxSpeed => maxSpeed;
        public Vector3 VelocityBeforePhysics { get; private set; }
        public int PlayerIndex { get; set; }
        public EraserDefinition Definition { get; private set; }

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

            CapturePrefabScale();
            ApplyPhysicsTuning();
            EnsureMotionTrail();
        }

        public void ApplyDefinition(EraserDefinition definition)
        {
            Definition = definition;
            CapturePrefabScale();
            if (definition == null)
            {
                ApplyPhysicsTuning();
                return;
            }

            mass = definition.Mass;
            power = definition.Power;
            speed = definition.Speed;
            var size = definition.SizeMeters;
            var ratio = new Vector3(
                size.x / ReferenceSizeMeters.x,
                size.y / ReferenceSizeMeters.y,
                size.z / ReferenceSizeMeters.z);
            transform.localScale = Vector3.Scale(prefabLocalScale, ratio);
            sizeFactor = Mathf.Max(ratio.x, ratio.z);

            ApplyPhysicsTuning();
            ApplySurfaceMaterial(definition.Friction, definition.Bounciness);
        }

        void CapturePrefabScale()
        {
            if (prefabScaleCaptured)
            {
                return;
            }

            prefabLocalScale = transform.localScale;
            prefabScaleCaptured = true;
        }

        void ApplyPhysicsTuning()
        {
            if (body == null)
            {
                return;
            }

            body.mass = mass;
            body.linearDamping = Mathf.Max(0.05f, 1.5f / Mathf.Max(0.1f, speed));
            body.angularDamping = 1.2f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        void ApplySurfaceMaterial(float friction, float bounciness)
        {
            var collider = GetComponentInChildren<Collider>();
            if (collider == null)
            {
                return;
            }

            var material = new PhysicsMaterial("EraserSurface")
            {
                dynamicFriction = Mathf.Clamp01(friction),
                staticFriction = Mathf.Clamp01(friction),
                bounciness = Mathf.Clamp01(bounciness),
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Average,
            };
            collider.material = material;
        }

        void FixedUpdate()
        {
            // Snapshot before the solver so OnCollisionEnter can rebuild approach speeds.
            VelocityBeforePhysics = body.linearVelocity;
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
            if (body == null)
            {
                return true;
            }

            return body.linearVelocity.sqrMagnitude <= linearEpsilon * linearEpsilon
                   && body.angularVelocity.sqrMagnitude <= angularEpsilon * angularEpsilon;
        }
    }
}
