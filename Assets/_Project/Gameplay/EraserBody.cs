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
        [SerializeField] float friction = 0.4f;
        [SerializeField] float bounciness = 0.05f;
        [SerializeField] bool enableMotionTrail = true;

        Vector3 prefabLocalScale = Vector3.one;
        bool prefabScaleCaptured;
        PhysicsMaterial surface;

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
            friction = definition.Friction;
            bounciness = definition.Bounciness;
            var size = definition.SizeMeters;
            var ratio = new Vector3(
                size.x / ReferenceSizeMeters.x,
                size.y / ReferenceSizeMeters.y,
                size.z / ReferenceSizeMeters.z);
            transform.localScale = Vector3.Scale(prefabLocalScale, ratio);
            sizeFactor = Mathf.Max(ratio.x, ratio.z);

            ApplyPhysicsTuning();
            ApplySurfaceMaterial(friction, bounciness);
        }

        public void ApplyModifiers(EffectModifiers modifiers)
        {
            ApplyPhysicsTuning(modifiers.Mass, modifiers.Speed);
            ApplySurfaceMaterial(friction * modifiers.Friction, bounciness * modifiers.Bounciness);
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

        void ApplyPhysicsTuning(float massMultiplier = 1f, float speedMultiplier = 1f)
        {
            if (body == null)
            {
                return;
            }

            body.mass = mass * Mathf.Max(0.05f, massMultiplier);
            body.linearDamping = Mathf.Max(0.05f, 1.5f / Mathf.Max(0.1f, speed * speedMultiplier));
            body.angularDamping = 1.2f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Friction above 1 is legal in PhysX; glue cards need it to out-grip the desk.
        void ApplySurfaceMaterial(float surfaceFriction, float surfaceBounciness)
        {
            var collider = GetComponentInChildren<Collider>();
            if (collider == null)
            {
                return;
            }

            if (surface == null)
            {
                surface = new PhysicsMaterial("EraserSurface")
                {
                    frictionCombine = PhysicsMaterialCombine.Average,
                    bounceCombine = PhysicsMaterialCombine.Average,
                };
            }

            surface.dynamicFriction = Mathf.Clamp(surfaceFriction, 0f, 2f);
            surface.staticFriction = Mathf.Clamp(surfaceFriction, 0f, 2f);
            surface.bounciness = Mathf.Clamp01(surfaceBounciness);
            collider.sharedMaterial = surface;
        }

        void FixedUpdate()
        {
            // Snapshot before the solver so OnCollisionEnter can rebuild approach speeds.
            VelocityBeforePhysics = body.linearVelocity;
        }

        public void ApplyFlick(Vector3 worldImpulse, Vector3 contactPoint, float extraYawImpulse = 0f)
        {
            body.WakeUp();
            body.AddForceAtPosition(worldImpulse, contactPoint, ForceMode.Impulse);
            if (extraYawImpulse != 0f)
            {
                body.AddTorque(new Vector3(0f, extraYawImpulse, 0f), ForceMode.Impulse);
            }

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
