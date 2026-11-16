using System;
using EraserWars.Audio;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class FalloutDetector : MonoBehaviour
    {
        [SerializeField] float deskTopY = 0.75f;
        [SerializeField] float fallDepth = 0.05f;
        [SerializeField] LayerMask deskMask = ~0;

        public event Action<EraserBody> Fell;

        EraserBody eraser;
        bool reported;
        bool floorCuePlayed;

        void Awake()
        {
            eraser = GetComponent<EraserBody>();
        }

        void FixedUpdate()
        {
            if (reported || eraser == null)
            {
                return;
            }

            var position = eraser.Body.position;
            if (position.y < deskTopY - fallDepth)
            {
                if (!IsSupportedByDesk(position))
                {
                    reported = true;
                    Fell?.Invoke(eraser);
                }
            }
        }

        public void ResetReport()
        {
            reported = false;
            floorCuePlayed = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            TryPlayFloorHit(collision.collider);
        }

        public void TryPlayFloorHit(Collider other)
        {
            if (floorCuePlayed || other == null || !IsFloor(other.transform))
            {
                return;
            }

            floorCuePlayed = true;
            AudioBus.Ensure().Play(AudioCue.Fall);
        }

        static bool IsFloor(Transform node)
        {
            for (var current = node; current != null; current = current.parent)
            {
                if (current.name.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        bool IsSupportedByDesk(Vector3 position)
        {
            var origin = position + Vector3.up * 0.2f;
            var hits = Physics.RaycastAll(origin, Vector3.down, 0.5f, deskMask, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < hits.Length; i++)
            {
                var hitTransform = hits[i].collider.transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (reported)
            {
                return;
            }

            if (other.CompareTag("KillZone"))
            {
                reported = true;
                Fell?.Invoke(eraser);
            }
        }
    }
}
