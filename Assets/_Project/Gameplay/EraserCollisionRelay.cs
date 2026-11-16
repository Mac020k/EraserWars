using System;
using EraserWars.Audio;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class EraserCollisionRelay : MonoBehaviour
    {
        [SerializeField] float restitution = 1f;
        [SerializeField] float pairCooldownSeconds = 0.04f;
        [SerializeField] float minClosingSpeed = 2f;
        [SerializeField] float minLaunchSpeed = 2.4f;
        [SerializeField] float separationMeters = 0.02f;

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
            TryHandlePair(collision);
        }

        // Already-touching erasers never re-fire Enter after a flick shove; Stay carries those hits.
        void OnCollisionStay(Collision collision)
        {
            TryHandlePair(collision);
        }

        void TryHandlePair(Collision collision)
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

            // One body owns the pair so both hooks do not double-apply.
            if (self.PlayerIndex > other.PlayerIndex)
            {
                return;
            }

            if (Time.time < nextPairTime)
            {
                return;
            }

            if (!ApplyRestitutionImpulse(other, collision))
            {
                return;
            }

            nextPairTime = Time.time + Mathf.Max(0f, pairCooldownSeconds);
            hitstop?.NotifyEraserCollision();
            PlayAcceptedPairCue();
        }

        public void PlayAcceptedPairCue()
        {
            AudioBus.Ensure().Play(AudioCue.Collision);
        }

        bool ApplyRestitutionImpulse(EraserBody other, Collision collision)
        {
            if (collision.contactCount <= 0)
            {
                return false;
            }

            // Desk play is planar: vertical Stay normals (common when already touching) would
            // launch along Y then get wiped by the y-preserve below, so the hittee barely moves.
            if (!TryPlanarNormal(collision, self, other, out var n))
            {
                return false;
            }

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

        static bool TryPlanarNormal(
            Collision collision,
            EraserBody selfBody,
            EraserBody otherBody,
            out Vector3 normal)
        {
            normal = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector3.zero;
            normal.y = 0f;
            if (normal.sqrMagnitude >= 1e-8f)
            {
                normal.Normalize();
                return true;
            }

            // Degenerate / vertical contact: separate along desk-plane centers.
            normal = selfBody.Body.position - otherBody.Body.position;
            normal.y = 0f;
            if (normal.sqrMagnitude < 1e-8f)
            {
                return false;
            }

            normal.Normalize();
            return true;
        }
    }
}
