using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    /// <summary>
    /// Assigns UITK background images. Fit uses ScaleToFit; face tiles use ScaleAndCrop
    /// with face zoom/position provided by USS (.char-tile-art).
    /// </summary>
    public static class UiImageUtil
    {
        public static void ApplyFit(VisualElement element, Texture2D texture)
        {
            if (element == null || texture == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(texture);
            element.style.backgroundColor = Color.clear;
#pragma warning disable CS0618
            element.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#pragma warning restore CS0618
        }

        public static void ApplyFit(VisualElement element, Sprite sprite)
        {
            if (element == null || sprite == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(sprite);
            element.style.backgroundColor = Color.clear;
#pragma warning disable CS0618
            element.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#pragma warning restore CS0618
        }

        public static void ApplyFaceCrop(VisualElement element, Sprite sprite)
        {
            if (element == null || sprite == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(sprite);
            element.style.backgroundColor = Color.clear;
#pragma warning disable CS0618
            element.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
#pragma warning restore CS0618
        }

        public static void Clear(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.style.backgroundImage = StyleKeyword.None;
        }
    }
}
