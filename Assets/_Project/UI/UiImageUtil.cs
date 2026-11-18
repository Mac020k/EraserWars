using EraserWars.Cards;
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
                new Length(texture.width, LengthUnit.Pixel),
                new Length(texture.height, LengthUnit.Pixel));
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

        /// <summary>
        /// Paints the baked square face bust (<see cref="CharacterDefinition.FaceSprite"/>) so it
        /// fills the element without distortion. Falls back to the top of the full still.
        /// </summary>
        public static void ApplyFaceCrop(VisualElement element, CharacterDefinition character)
        {
            ApplyFaceCrop(element, character != null ? character.FaceSprite : null);
        }

        public static void ApplyFaceCrop(VisualElement element, Sprite sprite)
        {
            if (element == null || sprite == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(sprite);
            element.style.backgroundColor = Color.clear;
            element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
            element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            element.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top);
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
