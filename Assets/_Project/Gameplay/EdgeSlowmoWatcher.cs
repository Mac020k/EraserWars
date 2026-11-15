using UnityEngine;

namespace EraserWars.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EdgeSlowmoWatcher : MonoBehaviour
    {
        [SerializeField] float slowmoScale = 0.4f;
        [SerializeField] float duration = 0.22f;
        [SerializeField] float cooldown = 0.6f;
        [SerializeField] float edgeMargin = 0.05f;
        [SerializeField] float fallDepth = 0.05f;
        [SerializeField] float deskTopY = 0.75f;

        // x is half the combined desk width. y is half the depth. Classroom tops are 1.2 by 0.8 at the origin.
        [SerializeField] Vector2 deskHalfExtents = new(0.6f, 0.4f);

        enum SlowmoPhase
        {
            Armed,
            Slowing,
            Disarmed,
        }

        MatchDirector director;
        HitstopController hitstop;
        SlowmoPhase phase = SlowmoPhase.Armed;
        float slowUntil;
        float cooldownUntil;
        float scaleBefore = 1f;
        bool scaleApplied;
        bool nearMiss;

        void Awake()
        {
            director = GetComponent<MatchDirector>();
            hitstop = GetComponent<HitstopController>();
        }

        public void BeginMatch()
        {
            RestoreScale();
            phase = SlowmoPhase.Armed;
            nearMiss = false;
            slowUntil = 0f;
            cooldownUntil = 0f;
        }

        void OnDisable()
        {
            RestoreScale();
            if (phase == SlowmoPhase.Slowing)
            {
                phase = SlowmoPhase.Disarmed;
            }
        }

        void Update()
        {
            Tick();
        }

        void FixedUpdate()
        {
            nearMiss = SampleNearMiss();
            Tick();
        }

        void Tick()
        {
            var freezing = hitstop != null && hitstop.IsFreezing;
            var now = Time.unscaledTime;

            if (phase == SlowmoPhase.Slowing)
            {
                // Eraser-hit freeze measures 0.08s with Time.time. A lowered timescale would stretch that freeze.
                if (freezing || now >= slowUntil)
                {
                    RestoreScale();
                    phase = SlowmoPhase.Disarmed;
                }

                return;
            }

            if (phase == SlowmoPhase.Disarmed)
            {
                if (now < cooldownUntil || nearMiss)
                {
                    return;
                }

                phase = SlowmoPhase.Armed;
            }

            if (phase != SlowmoPhase.Armed || freezing || !nearMiss || slowmoScale <= 0f)
            {
                return;
            }

            if (director == null || !director.IsSimulating())
            {
                return;
            }

            scaleBefore = Time.timeScale;
            Time.timeScale = slowmoScale;
            scaleApplied = true;
            // Wall clock. Scaled time would turn 0.22s into about half a second at 0.4.
            slowUntil = now + duration;
            cooldownUntil = now + cooldown;
            phase = SlowmoPhase.Slowing;
        }

        bool SampleNearMiss()
        {
            if (director == null)
            {
                return false;
            }

            var erasers = director.Erasers;
            for (var i = 0; i < erasers.Count; i++)
            {
                var eraser = erasers[i];
                if (eraser == null || !eraser.isActiveAndEnabled || eraser.Body == null)
                {
                    continue;
                }

                if (IsNearMiss(eraser.Body.position))
                {
                    return true;
                }
            }

            return false;
        }

        bool IsNearMiss(Vector3 position)
        {
            if (IsInFallBand(position.y))
            {
                return true;
            }

            return IsSupportedNearEdge(position);
        }

        // FalloutDetector reports once y is below deskTopY - fallDepth.
        // Resting centers sit above deskTopY (desk marker is the cube center, surface is higher),
        // so the band is only the slice under the marker.
        bool IsInFallBand(float y)
        {
            return y <= deskTopY && y >= deskTopY - fallDepth;
        }

        bool IsSupportedNearEdge(Vector3 position)
        {
            if (position.y < deskTopY)
            {
                return false;
            }

            var insetX = deskHalfExtents.x - Mathf.Abs(position.x);
            var insetZ = deskHalfExtents.y - Mathf.Abs(position.z);
            if (insetX < 0f || insetZ < 0f)
            {
                return false;
            }

            return Mathf.Min(insetX, insetZ) <= edgeMargin;
        }

        void RestoreScale()
        {
            if (!scaleApplied)
            {
                return;
            }

            Time.timeScale = scaleBefore;
            scaleApplied = false;
        }
    }
}
