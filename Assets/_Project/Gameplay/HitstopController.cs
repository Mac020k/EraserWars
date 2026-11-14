using System.Collections;
using UnityEngine;

namespace Keshipin.Gameplay
{
    public sealed class HitstopController : MonoBehaviour
    {
        [SerializeField] int minFrames = 2;
        [SerializeField] int maxFrames = 10;
        [SerializeField] int shotBudgetFrames = 30;

        int usedThisShot;
        Coroutine running;

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
