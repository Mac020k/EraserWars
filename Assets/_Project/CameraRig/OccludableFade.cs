using UnityEngine;

namespace EraserWars.CameraRig
{
    /// <summary>
    /// Hides wall/ceiling meshes completely while they block the orbit camera (ADR-0009 / 5A).
    /// </summary>
    public sealed class OccludableFade : MonoBehaviour
    {
        [SerializeField] float hideSpeed = 14f;

        Renderer[] renderers;
        float currentVisible = 1f;
        float targetVisible = 1f;
        bool hidden;

        public void SetOccluded(bool occluded)
        {
            targetVisible = occluded ? 0f : 1f;
        }

        void Awake()
        {
            // Colliders stay enabled so CameraOcclusionFader sphere-casts keep hitting.
            renderers = GetComponentsInChildren<Renderer>(true);
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

        void ApplyHidden(bool hide)
        {
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
