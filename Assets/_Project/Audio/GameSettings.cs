using UnityEngine;

namespace EraserWars.Audio
{
    public static class GameSettings
    {
        public const string BgmPercentKey = "eraserwars.settings.bgmPercent";
        public const string SePercentKey = "eraserwars.settings.sePercent";
        public const string BgmSliderName = "opt-bgm";
        public const string SeSliderName = "opt-se";
        public const float DefaultBgmPercent = 70f;
        public const float DefaultSePercent = 80f;

        public static float ClampPercent(float percent)
        {
            return Mathf.Clamp(percent, 0f, 100f);
        }

        public static float PercentToGain(float percent)
        {
            return ClampPercent(percent) / 100f;
        }

        public static void SaveBgmPercent(float percent)
        {
            PlayerPrefs.SetFloat(BgmPercentKey, ClampPercent(percent));
        }

        public static void SaveSePercent(float percent)
        {
            PlayerPrefs.SetFloat(SePercentKey, ClampPercent(percent));
        }

        public static float LoadBgmGain()
        {
            return PercentToGain(PlayerPrefs.GetFloat(BgmPercentKey, DefaultBgmPercent));
        }

        public static float LoadSeGain()
        {
            return PercentToGain(PlayerPrefs.GetFloat(SePercentKey, DefaultSePercent));
        }
    }
}
