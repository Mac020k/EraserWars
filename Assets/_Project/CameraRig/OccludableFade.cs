using UnityEngine;
using UnityEngine.Rendering;

namespace EraserWars.CameraRig
{
    /// <summary>
    /// Marks wall/ceiling meshes that should fade when they block the orbit camera.
    /// </summary>
    public sealed class OccludableFade : MonoBehaviour
    {
        [SerializeField] float occludedAlpha = 0.22f;
        [SerializeField] float fadeSpeed = 6f;

        Renderer[] renderers;
        Material[] opaqueMats;
        Material[] fadeMats;
        float currentAlpha = 1f;
        float targetAlpha = 1f;
        bool usingFade;

        public void SetOccluded(bool occluded)
        {
            targetAlpha = occluded ? occludedAlpha : 1f;
        }

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            opaqueMats = new Material[renderers.Length];
            fadeMats = new Material[renderers.Length];
            for (var i = 0; i < renderers.Length; i++)
            {
                opaqueMats[i] = renderers[i].sharedMaterial;
                if (opaqueMats[i] == null)
                {
                    continue;
                }

                fadeMats[i] = new Material(opaqueMats[i])
                {
                    name = opaqueMats[i].name + "_Fade",
                };
                ConfigureTransparent(fadeMats[i]);
            }
        }

        void LateUpdate()
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            var wantFade = currentAlpha < 0.995f;
            if (wantFade != usingFade)
            {
                usingFade = wantFade;
                SwapMaterials(wantFade);
            }

            if (!usingFade)
            {
                return;
            }

            for (var i = 0; i < fadeMats.Length; i++)
            {
                var mat = fadeMats[i];
                if (mat == null)
                {
                    continue;
                }

                if (mat.HasProperty("_BaseColor"))
                {
                    var c = mat.GetColor("_BaseColor");
                    c.a = currentAlpha;
                    mat.SetColor("_BaseColor", c);
                }

                if (mat.HasProperty("_Color"))
                {
                    var c = mat.GetColor("_Color");
                    c.a = currentAlpha;
                    mat.SetColor("_Color", c);
                }
            }
        }

        void SwapMaterials(bool fade)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                renderers[i].sharedMaterial = fade ? fadeMats[i] : opaqueMats[i];
            }
        }

        static void ConfigureTransparent(Material mat)
        {
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
            }

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }
}
