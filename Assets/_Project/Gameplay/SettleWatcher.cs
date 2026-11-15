using System;
using System.Collections.Generic;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class SettleWatcher : MonoBehaviour
    {
        [SerializeField] float linearEpsilon = 0.02f;
        [SerializeField] float angularEpsilon = 0.05f;
        [SerializeField] float settleHoldSeconds = 0.3f;
        [SerializeField] float maxSimSeconds = 8f;

        readonly List<EraserBody> erasers = new();
        float settledTimer;
        float simTimer;
        bool watching;

        public event Action Settled;

        public void Begin(IEnumerable<EraserBody> bodies)
        {
            erasers.Clear();
            erasers.AddRange(bodies);
            settledTimer = 0f;
            simTimer = 0f;
            watching = true;
        }

        public void Cancel()
        {
            watching = false;
            erasers.Clear();
            settledTimer = 0f;
            simTimer = 0f;
        }

        void FixedUpdate()
        {
            if (!watching)
            {
                return;
            }

            simTimer += Time.fixedDeltaTime;
            var allSettled = true;
            for (var i = 0; i < erasers.Count; i++)
            {
                var eraser = erasers[i];
                if (eraser == null)
                {
                    continue;
                }

                if (!eraser.IsSettled(linearEpsilon, angularEpsilon))
                {
                    allSettled = false;
                    break;
                }
            }

            if (allSettled)
            {
                settledTimer += Time.fixedDeltaTime;
            }
            else
            {
                settledTimer = 0f;
            }

            if (settledTimer >= settleHoldSeconds || simTimer >= maxSimSeconds)
            {
                watching = false;
                Settled?.Invoke();
            }
        }
    }
}
