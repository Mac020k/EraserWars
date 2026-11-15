using UnityEngine;

namespace EraserWars.CameraRig
{
    /// <summary>
    /// Hides wall/ceiling meshes completely while they block the orbit camera (ADR-0009 / 5A).
    /// Optional plane: hide when the camera is on the exterior side and focus is interior
    /// (covers looking in through open window bays that sphere-casts miss).
    /// </summary>
    public sealed class OccludableFade : MonoBehaviour
    {
        [SerializeField] float hideSpeed = 14f;
        [SerializeField] bool useSeparationPlane;
        [SerializeField] Vector3 planePoint;
        [SerializeField] Vector3 planeNormalInward = Vector3.right;

        Renderer[] renderers;
        float currentVisible = 1f;
        float targetVisible = 1f;
        bool hidden;

        public void ConfigureSeparationPlane(Vector3 point, Vector3 inwardNormal)
        {
            useSeparationPlane = true;
            planePoint = point;
            planeNormalInward = inwardNormal.normalized;
        }

        public bool SeparatesCameraFromFocus(Vector3 cameraPosition, Vector3 focusPosition)
        {
            if (!useSeparationPlane)
            {
                return false;
            }

            var n = planeNormalInward.sqrMagnitude > 0.0001f ? planeNormalInward.normalized : Vector3.up;
            var camSide = Vector3.Dot(cameraPosition - planePoint, n);
            var focusSide = Vector3.Dot(focusPosition - planePoint, n);
            // Camera outside (negative), focus inside (positive).
            return camSide < -0.05f && focusSide > 0.05f;
        }

        public void SetOccluded(bool occluded)
        {
            targetVisible = occluded ? 0f : 1f;
        }

        void Awake()
        {
            // Colliders stay enabled so CameraOcclusionFader sphere-casts keep hitting.
            // Re-query each hide so wall-mounted children (chalk, curtains, lights) stay in sync.
            RefreshRenderers();
        }

        void LateUpdate()
        {
            currentVisible = Mathf.MoveTowards(currentVisible, targetVisible, hideSpeed * Time.unscaledDeltaTime);
            var wantHidden = currentVisible < 0.05f;
            if (wantHidden == hidden)
            {
                return;
            }

            hidden = wantHidden;
            ApplyHidden(hidden);
        }

        void RefreshRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        void ApplyHidden(bool hide)
        {
            RefreshRenderers();
            if (renderers == null)
            {
                return;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = !hide;
                }
            }
        }

        void OnDisable()
        {
            if (hidden)
            {
                hidden = false;
                ApplyHidden(false);
            }
        }
    }
}
