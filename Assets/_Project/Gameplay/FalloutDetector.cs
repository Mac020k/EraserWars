using System;
using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class FalloutDetector : MonoBehaviour
    {
        [SerializeField] float deskTopY = 0.75f;
        [SerializeField] float fallDepth = 0.05f;
        [SerializeField] LayerMask deskMask = ~0;

        public event Action<EraserBody> Fell;

        EraserBody eraser;
        bool reported;

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
