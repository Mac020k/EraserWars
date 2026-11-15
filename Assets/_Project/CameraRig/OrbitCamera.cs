using UnityEngine;
using UnityEngine.InputSystem;

namespace EraserWars.CameraRig
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        // Wide enough that the 1.2×0.8 desk, four legs, and floor contact all fit at match start.
        const float OverviewDistance = 4.55f;
        const float OverviewYaw = 28f;
        const float OverviewPitch = 36f;
        // Bias the look target toward the floor so legs do not clip out of frame.
        const float OverviewFocusBiasY = -0.34f;

        [SerializeField] Transform focus;
        [SerializeField] float distance = OverviewDistance;
        [SerializeField] float yaw = OverviewYaw;
        [SerializeField] float pitch = OverviewPitch;
        [SerializeField] float mouseSensitivity = 0.2f;
        [SerializeField] float zoomSpeed = 0.12f;
        [SerializeField] float chaseLerp = 10f;
        [SerializeField] float chaseDistanceScale = 0.88f;
        [SerializeField] float minDistance = 1.4f;
        [SerializeField] float maxDistance = 6.5f;

        Transform chaseTarget;
        Vector3 smoothedFocus;
        bool hasSmoothedFocus;

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

        public void FrameDeskOverview()
        {
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

                distance = Mathf.Clamp(
                    distance - mouse.scroll.ReadValue().y * zoomSpeed * 0.01f,
                    minDistance,
                    maxDistance);
            }

            Apply(immediate: false);
        }

        void Apply(bool immediate)
        {
            var deskFocus = focus != null ? focus.position : new Vector3(0f, 0.77f, 0f);
            Vector3 anchor;
            if (chaseTarget != null)
            {
                if (!hasSmoothedFocus)
                {
                    smoothedFocus = chaseTarget.position;
                    hasSmoothedFocus = true;
                }

                if (immediate)
                {
                    smoothedFocus = chaseTarget.position;
                }
                else
                {
                    smoothedFocus = Vector3.Lerp(
                        smoothedFocus,
                        chaseTarget.position,
                        1f - Mathf.Exp(-chaseLerp * Time.deltaTime));
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
            if (chaseTarget != null && mode == Mode.Orbit)
            {
                useDistance *= chaseDistanceScale;
            }

            var rotation = Quaternion.Euler(usePitch, yaw, 0f);
            transform.position = anchor + rotation * (Vector3.back * useDistance);
            transform.LookAt(anchor);
        }

        static Vector3 OverviewAnchor(Vector3 deskFocus)
        {
            return deskFocus + new Vector3(0f, OverviewFocusBiasY, 0f);
        }
    }
}
