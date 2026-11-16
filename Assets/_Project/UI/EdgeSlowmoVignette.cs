using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public static class EdgeSlowmoVignette
    {
        public const string ElementName = "edge-slowmo-veil";

        static readonly Color RimTint = new(0.16f, 0.04f, 0.03f, 1f);
        static Texture2D mask;

        public static Texture2D Mask
        {
            get
            {
                if (mask == null)
                {
                    mask = BuildMask();
                }

                return mask;
            }
        }

        public static void Paint(VisualElement hudRoot, EdgeSlowmoCue cue)
        {
            if (hudRoot == null)
            {
                return;
            }

            var veil = hudRoot.Q(ElementName);
            if (veil == null)
            {
                veil = new VisualElement
                {
                    name = ElementName,
                    pickingMode = PickingMode.Ignore,
                };
                veil.style.position = Position.Absolute;
                veil.style.left = 0f;
                veil.style.right = 0f;
                veil.style.top = 0f;
                veil.style.bottom = 0f;
                veil.style.backgroundImage = new StyleBackground(Mask);
                veil.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
                veil.style.unityBackgroundImageTintColor = RimTint;
                hudRoot.Insert(0, veil);
            }

            var show = cue.EdgeAlpha > 0f;
            veil.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            veil.style.opacity = cue.EdgeAlpha;
        }

        static Texture2D BuildMask()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "EdgeSlowmoVeil",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color[size * size];
            var max = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - max) / max;
                    var dy = (y - max) / max;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy) / 1.41421356f;
                    var t = Mathf.Clamp01((dist - 0.55f) / 0.45f);
                    var rim = t * t * (3f - 2f * t);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, rim);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }
    }
}
