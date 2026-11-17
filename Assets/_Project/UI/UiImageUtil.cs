using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    /// <summary>
    /// Assigns UITK background images. Fit uses ScaleToFit. Face tiles zoom with
    /// background-size and a top anchor so the head fills the circle.
    /// </summary>
    public static class UiImageUtil
    {
        public static void ApplyGlyph(VisualElement element, Texture2D texture)
        {
            if (element == null || texture == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(texture);
#pragma warning disable CS0618
            element.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
#pragma warning restore CS0618
        }

        public static void ApplyPattern(VisualElement element, Texture2D texture)
        {
            if (element == null || texture == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(texture);
            element.style.backgroundColor = Color.clear;
            element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
            element.style.backgroundSize = new BackgroundSize(
                new Length(48, LengthUnit.Pixel),
                new Length(48, LengthUnit.Pixel));
        }

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

        public const float FaceZoomPercent = 780f;
        public const float FaceAnchorPercent = 2f;

        public static void ApplyFaceCrop(VisualElement element, Sprite sprite)
        {
            if (element == null || sprite == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(sprite);
            element.style.backgroundColor = Color.clear;
            element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            element.style.backgroundSize = new BackgroundSize(
                new Length(FaceZoomPercent, LengthUnit.Percent),
                new Length(FaceZoomPercent, LengthUnit.Percent));
            element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            element.style.backgroundPositionY = new BackgroundPosition(
                BackgroundPositionKeyword.Top,
                new Length(FaceAnchorPercent, LengthUnit.Percent));
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
