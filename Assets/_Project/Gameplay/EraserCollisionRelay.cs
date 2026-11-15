using System;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class EraserCollisionRelay : MonoBehaviour
    {
        [SerializeField] float restitution = 1f;
        [SerializeField] float pairCooldownSeconds = 0.04f;
        [SerializeField] float minClosingSpeed = 1.25f;
        [SerializeField] float minLaunchSpeed = 1.4f;
        [SerializeField] float separationMeters = 0.004f;

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
            if (self.PlayerIndex > other.PlayerIndex)
            {
                return;
            }

            if (Time.time < nextPairTime)
            {
                return;
            }

            nextPairTime = Time.time + Mathf.Max(0f, pairCooldownSeconds);
            if (!ApplyRestitutionImpulse(other, collision))
            {
                return;
            }

            hitstop?.NotifyEraserCollision();
        }

        bool ApplyRestitutionImpulse(EraserBody other, Collision collision)
        {
            if (collision.contactCount <= 0)
            {
                return false;
            }

            var n = collision.GetContact(0).normal;
            if (n.sqrMagnitude < 1e-8f)
            {
                return false;
            }

            n.Normalize();

            var m1 = Mathf.Max(1e-5f, self.Body.mass);
            var m2 = Mathf.Max(1e-5f, other.Body.mass);

            // Prefer velocities cached before this physics step; fall back to undoing PhysX impulse.
            var v1Pre = self.VelocityBeforePhysics;
            var v2Pre = other.VelocityBeforePhysics;
            if (v1Pre.sqrMagnitude < 1e-10f && v2Pre.sqrMagnitude < 1e-10f)
            {
                var impulse = collision.impulse;
                v1Pre = self.Body.linearVelocity - impulse / m1;
                v2Pre = other.Body.linearVelocity + impulse / m2;
            }

            var v1n = Vector3.Dot(v1Pre, n);
            var v2n = Vector3.Dot(v2Pre, n);
            var closingBeforeBoost = CollisionMath.ClosingSpeed(v1n, v2n);

            // Close-range taps often arrive with tiny closing speed after PhysX softens them.
            CollisionMath.EnsureMinimumClosing(ref v1n, ref v2n, minClosingSpeed);
            if (!CollisionMath.IsClosing(v1n, v2n))
            {
                return false;
            }

            var v1nBefore = v1n;
            var v2nBefore = v2n;
            CollisionMath.ResolveNormalSpeeds(m1, m2, v1n, v2n, restitution, out var v1nOut, out var v2nOut);
            CollisionMath.EnsureMinimumLaunch(
                m1,
                m2,
                v1nBefore,
                v2nBefore,
                ref v1nOut,
                ref v2nOut,
                minLaunchSpeed);

            var v1 = v1Pre - n * Vector3.Dot(v1Pre, n) + n * v1nOut;
            var v2 = v2Pre - n * Vector3.Dot(v2Pre, n) + n * v2nOut;
            v1.y = self.Body.linearVelocity.y;
            v2.y = other.Body.linearVelocity.y;

            self.Body.linearVelocity = v1;
            other.Body.linearVelocity = v2;
            self.ClampSpeedPublic();
            other.ClampSpeedPublic();

            // Nudge apart so the next solver step does not immediately cancel the launch.
            var sep = n * separationMeters;
            self.Body.position += sep;
            other.Body.position -= sep;

            // Weak contacts still count as hits for feedback once we forced a launch.
            return closingBeforeBoost > 0.02f || CollisionMath.ClosingSpeed(v1nBefore, v2nBefore) >= minClosingSpeed * 0.5f;
        }
    }
}
