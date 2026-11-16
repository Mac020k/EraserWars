using System;
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
        // 48° FOV. This distance keeps the 1.2×0.8 m arena inside a near-overhead frame.
        const float TopDownPitch = 87f;
        const float TopDownDistance = 1.75f;
        // Nearer and lower than chase orbit so the active eraser reads large, and an aim stroke still fits.
        const float CloseUpPitch = 32f;
        const float CloseUpDistance = 0.9f;

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
        Transform activeEraser;
        Vector3 smoothedFocus;
        bool hasSmoothedFocus;
        FallLeave leave;

        public enum Mode
        {
            Orbit,
            TopDown,
            CloseUp,
        }

        Mode mode = Mode.Orbit;

        public Mode View => mode;

        public void SetChaseTarget(Transform target)
        {
            chaseTarget = target;
            if (target != null && !hasSmoothedFocus)
            {
                smoothedFocus = target.position;
                hasSmoothedFocus = true;
            }
        }

        public void SetActiveEraser(Transform eraser)
        {
            activeEraser = eraser;
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

        public void ToggleOverview()
        {
            mode = mode == Mode.TopDown ? Mode.Orbit : Mode.TopDown;
        }

        public void ToggleCloseUp()
        {
            mode = mode == Mode.CloseUp ? Mode.Orbit : Mode.CloseUp;
        }

        public void Snap()
        {
            Apply(immediate: true, Time.unscaledTime, 0f, 0f);
        }

        public void Tick(float nowUnscaled, float unscaledDelta, float scaledDelta)
        {
            Apply(immediate: false, nowUnscaled, unscaledDelta, scaledDelta);
        }

        public void FrameDeskOverview()
        {
            leave = default;
            chaseTarget = null;
            distance = OverviewDistance;
            yaw = OverviewYaw;
            pitch = OverviewPitch;
            mode = Mode.Orbit;
            if (focus != null)
            {
                smoothedFocus = DeskAnchor();
                hasSmoothedFocus = true;
            }

            Apply(immediate: true, Time.unscaledTime, 0f, 0f);
        }

        void Start()
        {
            FrameDeskOverview();
        }

        void LateUpdate()
        {
            ReadViewKeys();
            ReadOrbitDrag();
            Tick(Time.unscaledTime, Time.unscaledDeltaTime, Time.deltaTime);
        }

        void ReadViewKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                ToggleOverview();
            }

            if (keyboard.cKey.wasPressedThisFrame)
            {
                ToggleCloseUp();
            }
        }

        void ReadOrbitDrag()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.rightButton.isPressed)
            {
                var delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                if (mode == Mode.Orbit)
                {
                    pitch -= delta.y * mouseSensitivity;
                    pitch = Mathf.Clamp(pitch, 16f, 82f);
                }
            }

            if (mode != Mode.Orbit)
            {
                return;
            }

            var scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Proportional zoom: same feel near and far, much snappier than the old *0.01 path.
                var step = scroll * zoomSpeed * 0.01f * Mathf.Max(1.2f, distance);
                distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
            }
        }

        void Apply(bool immediate, float nowUnscaled, float unscaledDelta, float scaledDelta)
        {
            switch (mode)
            {
                case Mode.TopDown:
                    Place(DeskAnchor(), TopDownPitch, yaw, TopDownDistance);
                    return;
                case Mode.CloseUp:
                    Place(CloseUpAnchor(), CloseUpPitch, yaw, CloseUpDistance);
                    return;
                case Mode.Orbit:
                    PlaceOrbit(immediate, nowUnscaled, unscaledDelta, scaledDelta);
                    return;
                default:
                    var unexpected = mode;
                    throw new ArgumentOutOfRangeException(nameof(mode), unexpected, null);
            }
        }

        void PlaceOrbit(bool immediate, float nowUnscaled, float unscaledDelta, float scaledDelta)
        {
            var glancing = leave.Active(nowUnscaled);
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
                    var step = glancing ? unscaledDelta : scaledDelta;
                    smoothedFocus = Vector3.Lerp(
                        smoothedFocus,
                        desired,
                        1f - Mathf.Exp(-chaseLerp * step));
                }

                anchor = smoothedFocus;
            }
            else
            {
                smoothedFocus = DeskAnchor();
                hasSmoothedFocus = true;
                anchor = smoothedFocus;
            }

            var useDistance = distance;
            if (glancing)
            {
                useDistance *= FallGlanceDistanceScale;
            }
            else if (chaseTarget != null)
            {
                useDistance *= chaseDistanceScale;
            }

            Place(anchor, pitch, yaw, useDistance);
        }

        Vector3 CloseUpAnchor()
        {
            var subject = activeEraser != null ? activeEraser : chaseTarget;
            return subject != null ? subject.position : DeskAnchor();
        }

        Vector3 DeskAnchor()
        {
            var deskFocus = focus != null ? focus.position : new Vector3(0f, 0.77f, 0f);
            return OverviewAnchor(deskFocus);
        }

        void Place(Vector3 anchor, float usePitch, float useYaw, float useDistance)
        {
            var rotation = Quaternion.Euler(usePitch, useYaw, 0f);
            transform.position = anchor + rotation * (Vector3.back * useDistance);
            transform.LookAt(anchor);
        }

        static Vector3 OverviewAnchor(Vector3 deskFocus)
        {
            return deskFocus + new Vector3(0f, OverviewFocusBiasY, 0f);
        }
    }
}
