using UnityEngine;
using UnityEngine.InputSystem;

namespace EraserWars.CameraRig
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        // Default play framing: 2×2 arena fills most of the view (not a wide room overview).
        const float OverviewDistance = 1.85f;
        const float OverviewYaw = -22f;
        const float OverviewPitch = 54f;
        // Bias slightly toward the desk top so the play surface dominates the frame.
        const float OverviewFocusBiasY = 0.06f;

        [SerializeField] Transform focus;
        [SerializeField] float distance = OverviewDistance;
        [SerializeField] float yaw = OverviewYaw;
        [SerializeField] float pitch = OverviewPitch;
        [SerializeField] float mouseSensitivity = 0.2f;
        // Wheel notches are ~±120; this lands ~0.9–1.2m per notch at mid range.
        [SerializeField] float zoomSpeed = 0.85f;
        [SerializeField] float chaseLerp = 10f;
        [SerializeField] float chaseDistanceScale = 0.88f;
        [SerializeField] float minDistance = 1.0f;
        // About classroom-wide (7×9): beyond this the shell fills the frame without empty void.
        [SerializeField] float maxDistance = 7f;

        const float FallGlanceDistanceScale = 0.72f;

        Transform chaseTarget;
        Vector3 smoothedFocus;
        bool hasSmoothedFocus;
        FallLeave leave;
        CameraPunch punch;

        public enum Mode
        {
            Orbit,
            TopDown,
            CloseUp,
        }

        Mode mode = Mode.Orbit;

        public void SetChaseTarget(Transform target)
        {
            chaseTarget = target;
            if (target != null && !hasSmoothedFocus)
            {
                smoothedFocus = target.position;
                hasSmoothedFocus = true;
            }
        }

        public void ClearChaseTarget()
        {
            chaseTarget = null;
        }

        public Vector3 FallLeaveFocus => leave.Focus;

        public float FallLeaveUntilUnscaled => leave.UntilUnscaled;

        public bool FallLeaveActiveAt(float nowUnscaled) => leave.Active(nowUnscaled);

        public void PlayFallLeave(Vector3 worldFocus, float nowUnscaled, float seconds)
        {
            leave = FallLeave.Begin(worldFocus, nowUnscaled, seconds);
        }

        public float PunchDuration => punch.Duration;

        public Vector3 PunchOffsetAt(float nowUnscaled) => punch.OffsetAt(nowUnscaled);

        public void Punch(Vector3 worldDirection, float meters, float nowUnscaled, float duration)
        {
            var scaled = meters * CameraPunch.DistanceScale(distance, OverviewDistance);
            punch = CameraPunch.Begin(worldDirection, scaled, nowUnscaled, duration);
        }

        public void FrameDeskOverview()
        {
            leave = default;
            punch = default;
            chaseTarget = null;
            distance = OverviewDistance;
            yaw = OverviewYaw;
            pitch = OverviewPitch;
            mode = Mode.Orbit;
            if (focus != null)
            {
                smoothedFocus = OverviewAnchor(focus.position);
                hasSmoothedFocus = true;
            }

            Apply(immediate: true);
        }

        void Start()
        {
            FrameDeskOverview();
        }

        void LateUpdate()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame)
                {
                    mode = mode == Mode.TopDown ? Mode.Orbit : Mode.TopDown;
                }

                if (keyboard.cKey.wasPressedThisFrame)
                {
                    mode = mode == Mode.CloseUp ? Mode.Orbit : Mode.CloseUp;
                }
            }

            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    yaw += delta.x * mouseSensitivity;
                    pitch -= delta.y * mouseSensitivity;
                    pitch = Mathf.Clamp(pitch, 16f, 82f);
                }

                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    // Proportional zoom: same feel near and far, much snappier than the old *0.01 path.
                    var step = scroll * zoomSpeed * 0.01f * Mathf.Max(1.2f, distance);
                    distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
                }
            }

            Apply(immediate: false);
        }

        void Apply(bool immediate)
        {
            var deskFocus = focus != null ? focus.position : new Vector3(0f, 0.77f, 0f);
            var glancing = leave.Active(Time.unscaledTime);
            Vector3 anchor;
            if (glancing || chaseTarget != null)
            {
                var desired = glancing ? leave.Focus : chaseTarget.position;
                if (!hasSmoothedFocus)
                {
                    smoothedFocus = desired;
                    hasSmoothedFocus = true;
                }

                if (immediate)
                {
                    smoothedFocus = desired;
                }
                else
                {
                    var step = glancing ? Time.unscaledDeltaTime : Time.deltaTime;
                    smoothedFocus = Vector3.Lerp(
                        smoothedFocus,
                        desired,
                        1f - Mathf.Exp(-chaseLerp * step));
                }

                anchor = smoothedFocus;
            }
            else
            {
                smoothedFocus = OverviewAnchor(deskFocus);
                hasSmoothedFocus = true;
                anchor = smoothedFocus;
            }

            var usePitch = mode == Mode.TopDown ? 85f : mode == Mode.CloseUp ? 32f : pitch;
            var useDistance = mode == Mode.CloseUp ? distance * 0.42f : distance;
            if (glancing && mode == Mode.Orbit)
            {
                useDistance *= FallGlanceDistanceScale;
            }
            else if (chaseTarget != null && mode == Mode.Orbit)
            {
                useDistance *= chaseDistanceScale;
            }

            var rotation = Quaternion.Euler(usePitch, yaw, 0f);
            var kick = punch.OffsetAt(Time.unscaledTime);
            transform.position = anchor + rotation * (Vector3.back * useDistance) + kick;
            transform.LookAt(anchor);
        }

        static Vector3 OverviewAnchor(Vector3 deskFocus)
        {
            return deskFocus + new Vector3(0f, OverviewFocusBiasY, 0f);
        }
    }
}
