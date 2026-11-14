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
                if (!Physics.Raycast(position + Vector3.up * 0.2f, Vector3.down, 0.5f, deskMask))
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
