using UnityEngine;
using UnityEngine.InputSystem;

namespace Keshipin.CameraRig
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        [SerializeField] Transform focus;
        [SerializeField] float distance = 1.8f;
        [SerializeField] float yaw = 0f;
        [SerializeField] float pitch = 45f;
        [SerializeField] float mouseSensitivity = 0.2f;
        [SerializeField] float zoomSpeed = 0.1f;

        public enum Mode
        {
            Orbit,
            TopDown,
            CloseUp,
        }

        Mode mode = Mode.Orbit;

        void Update()
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
                    pitch = Mathf.Clamp(pitch, 15f, 80f);
                }

                distance = Mathf.Clamp(
                    distance - mouse.scroll.ReadValue().y * zoomSpeed * 0.01f,
                    0.6f,
                    4f);
            }

            Apply();
        }

        void Apply()
        {
            if (focus == null)
            {
                return;
            }

            var usePitch = mode == Mode.TopDown ? 85f : mode == Mode.CloseUp ? 35f : pitch;
            var useDistance = mode == Mode.CloseUp ? distance * 0.45f : distance;
            var rotation = Quaternion.Euler(usePitch, yaw, 0f);
            transform.position = focus.position + rotation * (Vector3.back * useDistance);
            transform.LookAt(focus.position);
        }
    }
}
