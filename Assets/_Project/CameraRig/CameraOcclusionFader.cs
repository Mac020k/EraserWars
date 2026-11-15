using System.Collections.Generic;
using UnityEngine;

namespace EraserWars.CameraRig
{
    /// <summary>
    /// Fades walls/ceilings that sit between the camera and the desk focus (ADR-0009 / 5A).
    /// </summary>
    public sealed class CameraOcclusionFader : MonoBehaviour
    {
        [SerializeField] Transform focus;
        [SerializeField] float radius = 0.18f;
        [SerializeField] LayerMask occluderMask = ~0;

        readonly HashSet<OccludableFade> active = new();
        readonly HashSet<OccludableFade> frameHits = new();
        OccludableFade[] known = System.Array.Empty<OccludableFade>();
        float refreshTimer;

        public void SetFocus(Transform value)
        {
            focus = value;
        }

        void LateUpdate()
        {
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                known = FindObjectsByType<OccludableFade>();
                refreshTimer = 1f;
            }

            if (focus == null)
            {
                ClearActive();
                return;
            }

            frameHits.Clear();
            var origin = transform.position;
            var target = focus.position;
            var delta = target - origin;
            var distance = delta.magnitude;
            if (distance > 0.05f)
            {
                var hits = Physics.SphereCastAll(
                    origin,
                    radius,
                    delta / distance,
                    distance,
                    occluderMask,
                    QueryTriggerInteraction.Ignore);
                for (var i = 0; i < hits.Length; i++)
                {
                    var fade = hits[i].collider.GetComponentInParent<OccludableFade>();
                    if (fade == null)
                    {
                        continue;
                    }

                    frameHits.Add(fade);
                    if (active.Add(fade))
                    {
                        fade.SetOccluded(true);
                    }
                }
            }

            active.RemoveWhere(fade =>
            {
                if (frameHits.Contains(fade))
                {
                    return false;
                }

                if (fade != null)
                {
                    fade.SetOccluded(false);
                }

                return true;
            });
        }

        void ClearActive()
        {
            foreach (var fade in active)
            {
                if (fade != null)
                {
                    fade.SetOccluded(false);
                }
            }

            active.Clear();
        }
    }
}
