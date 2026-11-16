using System.Collections;
using EraserWars.CameraRig;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.Gameplay
{
    public sealed class HitstopController : MonoBehaviour
    {
        public const string CameraShakePref = "eraserwars.cameraShake";

        [SerializeField] int shotBudgetFrames = 30;

        int usedThisShot;
        Coroutine running;
        SimulationMode modeBeforeFreeze;
        bool freezing;
        OrbitCamera orbit;
        Transform shaken;
        Vector3 shakenOrigin;
        bool holdingVisual;

        public bool IsFreezing => freezing;

        public void BeginShot()
        {
            usedThisShot = 0;
        }

        public void BindCamera(OrbitCamera camera)
        {
            if (camera != null)
            {
                orbit = camera;
            }
        }

        public void PresentAcceptedHit(NormalHit hit, Vector3 planarDirection, Transform visualChild)
        {
            if (hit.Impulse <= 0f)
            {
                return;
            }

            var frames = hit.Frames;
            var remaining = shotBudgetFrames - usedThisShot;
            if (remaining <= 0 || frames <= 0)
            {
                return;
            }

            frames = Mathf.Min(frames, remaining);
            usedThisShot += frames;

            var shakeOn = PlayerPrefs.GetInt(CameraShakePref, 1) != 0;
            orbit?.Punch(planarDirection, hit.KickMeters(shakeOn), Time.unscaledTime, frames / 60f);

            StopRunning();
            if (!Application.isPlaying)
            {
                return;
            }

            var amplitude = 0.0015f + 0.002f * hit.Strength;
            running = StartCoroutine(HitstopRoutine(frames, visualChild, amplitude));
        }

        void OnDisable()
        {
            StopRunning();
        }

        void StopRunning()
        {
            if (running != null)
            {
                StopCoroutine(running);
                running = null;
            }

            ReleaseFreeze();
        }

        void ReleaseFreeze()
        {
            RestoreVisual();
            if (!freezing)
            {
                return;
            }

            Physics.simulationMode = modeBeforeFreeze;
            freezing = false;
        }

        void RestoreVisual()
        {
            if (!holdingVisual || shaken == null)
            {
                holdingVisual = false;
                return;
            }

            shaken.localPosition = shakenOrigin;
            holdingVisual = false;
        }

        IEnumerator HitstopRoutine(int frames, Transform visualChild, float amplitude)
        {
            modeBeforeFreeze = Physics.simulationMode;
            freezing = true;
            Physics.simulationMode = SimulationMode.Script;
            if (visualChild != null && visualChild.GetComponent<Rigidbody>() == null)
            {
                shaken = visualChild;
                shakenOrigin = visualChild.localPosition;
                holdingVisual = true;
            }

            for (var i = 0; i < frames; i++)
            {
                if (holdingVisual && shaken != null)
                {
                    var decay = 1f - i / (float)frames;
                    var jitter = Random.insideUnitCircle * (amplitude * decay);
                    shaken.localPosition = shakenOrigin + new Vector3(jitter.x, 0f, jitter.y);
                }

                yield return null;
            }

            ReleaseFreeze();
            running = null;
        }
    }
}
