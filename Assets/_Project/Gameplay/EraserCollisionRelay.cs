using System;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class EraserCollisionRelay : MonoBehaviour
    {
        [SerializeField] float restitution = 0.92f;
        [SerializeField] float pairCooldownSeconds = 0.05f;

        EraserBody self;
        HitstopController hitstop;
        Func<bool> simulating;
        float nextPairTime;

        public void Bind(HitstopController controller, Func<bool> isSimulating)
        {
            hitstop = controller;
            simulating = isSimulating;
        }

        void Awake()
        {
            self = GetComponent<EraserBody>();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (self == null || simulating == null || !simulating())
            {
                return;
            }

            var other = collision.collider.GetComponentInParent<EraserBody>();
            if (other == null || other == self)
            {
                return;
            }

            // One body owns the pair so both OnCollisionEnter hooks do not double-apply.
            if (self.GetInstanceID() > other.GetInstanceID())
            {
                return;
            }

            if (Time.time < nextPairTime)
            {
                return;
            }

            nextPairTime = Time.time + Mathf.Max(0f, pairCooldownSeconds);
            ApplyRestitutionImpulse(other, collision);
            hitstop?.NotifyEraserCollision();
        }

        void ApplyRestitutionImpulse(EraserBody other, Collision collision)
        {
            if (collision.contactCount <= 0)
            {
                return;
            }

            var n = collision.GetContact(0).normal;
            if (n.sqrMagnitude < 1e-8f)
            {
                return;
            }

            n.Normalize();

            // Undo PhysX's solve using the reported impulse, then re-solve with our e.
            var impulse = collision.impulse;
            var m1 = Mathf.Max(1e-5f, self.Body.mass);
            var m2 = Mathf.Max(1e-5f, other.Body.mass);
            var v1Pre = self.Body.linearVelocity - impulse / m1;
            var v2Pre = other.Body.linearVelocity + impulse / m2;

            var v1n = Vector3.Dot(v1Pre, n);
            var v2n = Vector3.Dot(v2Pre, n);
            if (!CollisionMath.IsClosing(v1n, v2n))
            {
                return;
            }

            CollisionMath.ResolveNormalSpeeds(m1, m2, v1n, v2n, restitution, out var v1nOut, out var v2nOut);

            var v1 = v1Pre + n * (v1nOut - v1n);
            var v2 = v2Pre + n * (v2nOut - v2n);
            v1.y = self.Body.linearVelocity.y;
            v2.y = other.Body.linearVelocity.y;

            self.Body.linearVelocity = v1;
            other.Body.linearVelocity = v2;
            self.ClampSpeedPublic();
            other.ClampSpeedPublic();
        }
    }
}
