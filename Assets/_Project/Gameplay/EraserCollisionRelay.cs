using System;
using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class EraserCollisionRelay : MonoBehaviour
    {
        EraserBody self;
        HitstopController hitstop;
        Func<bool> simulating;

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
            if (self == null || hitstop == null || simulating == null || !simulating())
            {
                return;
            }

            var other = collision.collider.GetComponentInParent<EraserBody>();
            if (other == null || other == self)
            {
                return;
            }

            hitstop.NotifyEraserCollision();
        }
    }
}
