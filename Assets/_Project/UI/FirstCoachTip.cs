using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public static class FirstCoachTip
    {
        public const string PrefKey = "eraserwars.firstCoach";
        public const string StripName = "home-tip";
        public const string DismissName = "home-tip-dismiss";

        public static bool IsDismissed => PlayerPrefs.GetInt(PrefKey, 0) != 0;

        public static void Dismiss()
        {
            if (IsDismissed)
            {
                return;
            }

            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }

        public static void Apply(VisualElement root)
        {
            var strip = root?.Q(StripName);
            if (strip == null)
            {
                return;
            }

            var show = !IsDismissed;
            strip.EnableInClassList("hidden", !show);
            strip.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            var dismiss = strip.Q<Button>(DismissName);
            if (dismiss == null || dismiss.ClassListContains("home-tip-bound"))
            {
                return;
            }

            dismiss.AddToClassList("home-tip-bound");
            dismiss.RegisterCallback<ClickEvent>(_ =>
            {
                Dismiss();
                strip.EnableInClassList("hidden", true);
                strip.style.display = DisplayStyle.None;
            });
        }
    }
}
