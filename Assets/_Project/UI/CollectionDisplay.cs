using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public static class CollectionDisplay
    {
        public const float StillMaxWidth = 220f;
        public const float StillMaxHeight = 340f;
        public const float StillLockedSize = 220f;
        public const float CardFrameWidth = 210f;
        public const float CardFrameHeight = 294f;

        public static int DisplayedMass(float kilograms)
        {
            return Mathf.RoundToInt(kilograms * 1000f);
        }

        public static void FitStillFrame(VisualElement stage, float pixelWidth, float pixelHeight)
        {
            if (stage == null)
            {
                return;
            }

            var widthPx = Mathf.Max(1f, pixelWidth);
            var heightPx = Mathf.Max(1f, pixelHeight);
            var width = StillMaxWidth;
            var height = width * (heightPx / widthPx);
            if (height > StillMaxHeight)
            {
                height = StillMaxHeight;
                width = height * (widthPx / heightPx);
            }

            stage.style.width = width;
            stage.style.height = height;
            stage.style.overflow = Overflow.Hidden;
        }

        public static void FitCardFrame(VisualElement stage)
        {
            if (stage == null)
            {
                return;
            }

            stage.style.width = CardFrameWidth;
            stage.style.height = CardFrameHeight;
            stage.style.overflow = Overflow.Hidden;
        }
    }
}
