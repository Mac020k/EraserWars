using System.Collections;
using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class HitstopController : MonoBehaviour
    {
        [SerializeField] int minFrames = 2;
        [SerializeField] int maxFrames = 10;
        [SerializeField] int shotBudgetFrames = 30;
        [SerializeField] float eraserHitFreezeSeconds = 0.08f;
        [SerializeField] float eraserHitCooldownSeconds = 0.15f;

        int usedThisShot;
        float nextEraserHitTime;
        Coroutine running;
        Coroutine freezeRoutine;
        SimulationMode modeBeforeFreeze;
        bool freezing;

        public bool IsFreezing => freezing;

        public void BeginShot()
        {
            usedThisShot = 0;
        }

        public void Pulse(float impulseMagnitude, Transform visualRoot)
        {
            var frames = Mathf.Clamp(Mathf.RoundToInt(impulseMagnitude * 40f), minFrames, maxFrames);
            var remaining = shotBudgetFrames - usedThisShot;
            if (remaining <= 0)
            {
                return;
            }

            frames = Mathf.Min(frames, remaining);
            usedThisShot += frames;
            if (running != null)
            {
                StopCoroutine(running);
            }

            running = StartCoroutine(HitstopRoutine(frames, visualRoot));
        }

        public void NotifyEraserCollision()
        {
            if (freezing || Time.time < nextEraserHitTime)
            {
                return;
            }

            nextEraserHitTime = Time.time + Mathf.Max(0f, eraserHitCooldownSeconds);
            freezeRoutine = StartCoroutine(FreezeRoutine(Mathf.Max(0f, eraserHitFreezeSeconds)));
        }

        void OnDisable()
        {
            if (!freezing)
            {
                return;
            }

            if (freezeRoutine != null)
            {
                StopCoroutine(freezeRoutine);
                freezeRoutine = null;
            }

            Physics.simulationMode = modeBeforeFreeze;
            freezing = false;
        }

        IEnumerator FreezeRoutine(float seconds)
        {
            modeBeforeFreeze = Physics.simulationMode;
            freezing = true;
            Physics.simulationMode = SimulationMode.Script;
            var end = Time.time + seconds;
            while (Time.time < end)
            {
                yield return null;
            }

            if (!freezing)
            {
                yield break;
            }

            Physics.simulationMode = modeBeforeFreeze;
            freezing = false;
            freezeRoutine = null;
        }

        IEnumerator HitstopRoutine(int frames, Transform visualRoot)
        {
            var previous = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            var origin = visualRoot != null ? visualRoot.localPosition : Vector3.zero;
            for (var i = 0; i < frames; i++)
            {
                if (visualRoot != null)
                {
                    var decay = 1f - i / (float)frames;
                    visualRoot.localPosition = origin + Random.insideUnitSphere * (0.002f * decay);
                }

                yield return null;
            }

            if (visualRoot != null)
            {
                visualRoot.localPosition = origin;
            }

            Physics.simulationMode = previous;
            running = null;
        }
    }
}
