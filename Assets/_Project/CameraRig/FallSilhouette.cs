using UnityEngine;
using UnityEngine.Rendering;

namespace EraserWars.CameraRig
{
    public sealed class FallSilhouette : MonoBehaviour
    {
        public float EndsAtUnscaled { get; private set; }

        Material ownedMaterial;

        public static bool TryFlash(Renderer source, float seconds, float nowUnscaled, out GameObject ghost)
        {
            ghost = null;
            if (source == null)
            {
                return false;
            }

            var filter = source.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                return false;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                return false;
            }

            var mark = new GameObject("FallSilhouette");
            mark.hideFlags = Application.isPlaying ? HideFlags.None : HideFlags.DontSave;
            mark.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            mark.transform.localScale = source.transform.lossyScale;
            var mesh = mark.AddComponent<MeshFilter>();
            mesh.sharedMesh = filter.sharedMesh;
            var renderer = mark.AddComponent<MeshRenderer>();
            var material = new Material(shader);
            var color = FlashColor(source);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var cue = mark.AddComponent<FallSilhouette>();
            cue.ownedMaterial = material;
            cue.EndsAtUnscaled = nowUnscaled + FallLeave.Duration(seconds);
            ghost = mark;
            return true;
        }

        void Update()
        {
            if (Time.unscaledTime >= EndsAtUnscaled)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (ownedMaterial == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(ownedMaterial);
            }
            else
            {
                DestroyImmediate(ownedMaterial);
            }
        }

        static Color FlashColor(Renderer source)
        {
            var color = Color.white;
            var material = source.sharedMaterial;
            if (material != null)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    color = material.GetColor("_BaseColor");
                }
                else if (material.HasProperty("_Color"))
                {
                    color = material.GetColor("_Color");
                }
            }

            color = Color.Lerp(color, Color.white, 0.72f);
            color.a = 1f;
            return color;
        }
    }
}
