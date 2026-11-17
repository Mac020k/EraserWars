using EraserWars.Core;
using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using AudioPrefs = EraserWars.Audio.GameSettings;

namespace EraserWars.UI
{
    /// <summary>
    /// Match feel in PlayerPrefs. EraserWars.UI references Gameplay, so MatchDirector cannot call this type.
    /// Read Keys with PlayerPrefs. Volumes are 0..1 gains. Bgm/Se also mirror Audio.GameSettings percent keys
    /// so AudioBus and Options stay one contract. FlickScale is 0.5..1.5. TurnSeconds is 10..60.
    /// CameraShake is 0 or 1. UiScale is always 1.2. WinMode is 0 for Stock or 1 for Points.
    /// ReduceMotion is 0 or 1 on EdgeSlowmoCue.ReduceMotionPref. ColorblindSeats is 0 or 1 on SeatPalette.PrefKey.
    /// ApplyVolume sets AudioListener.volume. MatchDirector reads Keys.WinMode and Keys.TurnSeconds.
    /// </summary>
    public sealed record GameSettings(
        float Master,
        float Bgm,
        float Se,
        float FlickScale,
        int TurnSeconds,
        bool CameraShake,
        float UiScale,
        EraserWars.Core.WinMode WinMode,
        bool ReduceMotion,
        bool ColorblindSeats)
    {
        public const float DefaultMaster = 1f;
        public const float DefaultBgm = 0.7f;
        public const float DefaultSe = 0.8f;
        public const float DefaultFlickScale = 1f;
        public const int DefaultTurnSeconds = 20;
        public const bool DefaultCameraShake = true;
        public const float LargeUiScale = 1.2f;
        public const float DefaultUiScale = LargeUiScale;
        public const bool DefaultReduceMotion = false;
        public const bool DefaultColorblindSeats = false;

        public static class Keys
        {
            public const string Master = "eraserwars.master";
            public const string Bgm = "eraserwars.bgm";
            public const string Se = "eraserwars.se";
            public const string FlickScale = "eraserwars.flickScale";
            public const string TurnSeconds = "eraserwars.turnSeconds";
            public const string CameraShake = "eraserwars.cameraShake";
            public const string UiScale = "eraserwars.uiScale";
            public const string WinMode = "eraserwars.winMode";
            public const string ReduceMotion = EdgeSlowmoCue.ReduceMotionPref;
            public const string ColorblindSeats = SeatPalette.PrefKey;
        }

        public static GameSettings Defaults { get; } = new(
            DefaultMaster,
            DefaultBgm,
            DefaultSe,
            DefaultFlickScale,
            DefaultTurnSeconds,
            DefaultCameraShake,
            DefaultUiScale,
            EraserWars.Core.WinMode.Stock,
            DefaultReduceMotion,
            DefaultColorblindSeats);

        public static GameSettings Current { get; private set; } = Defaults;

        public static GameSettings Load()
        {
            var next = new GameSettings(
                PlayerPrefs.GetFloat(Keys.Master, DefaultMaster),
                AudioPrefs.LoadBgmGain(),
                AudioPrefs.LoadSeGain(),
                PlayerPrefs.GetFloat(Keys.FlickScale, DefaultFlickScale),
                PlayerPrefs.GetInt(Keys.TurnSeconds, DefaultTurnSeconds),
                PlayerPrefs.GetInt(Keys.CameraShake, DefaultCameraShake ? 1 : 0) != 0,
                PlayerPrefs.GetFloat(Keys.UiScale, DefaultUiScale),
                RuleSet.WinModeFromPref(PlayerPrefs.GetInt(Keys.WinMode, (int)EraserWars.Core.WinMode.Stock)),
                PlayerPrefs.GetInt(Keys.ReduceMotion, 0) != 0,
                PlayerPrefs.GetInt(Keys.ColorblindSeats, 0) != 0);
            Current = next.Clamped();
            return Current;
        }

        public static void Save(GameSettings next)
        {
            Current = next.Clamped();
            PlayerPrefs.SetFloat(Keys.Master, Current.Master);
            PlayerPrefs.SetFloat(Keys.Bgm, Current.Bgm);
            PlayerPrefs.SetFloat(Keys.Se, Current.Se);
            AudioPrefs.SaveBgmPercent(Current.Bgm * 100f);
            AudioPrefs.SaveSePercent(Current.Se * 100f);
            PlayerPrefs.SetFloat(Keys.FlickScale, Current.FlickScale);
            PlayerPrefs.SetInt(Keys.TurnSeconds, Current.TurnSeconds);
            PlayerPrefs.SetInt(Keys.CameraShake, Current.CameraShake ? 1 : 0);
            PlayerPrefs.SetFloat(Keys.UiScale, Current.UiScale);
            PlayerPrefs.SetInt(Keys.WinMode, (int)Current.WinMode);
            PlayerPrefs.SetInt(Keys.ReduceMotion, Current.ReduceMotion ? 1 : 0);
            PlayerPrefs.SetInt(Keys.ColorblindSeats, Current.ColorblindSeats ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ResetToDefaults()
        {
            Save(Defaults);
        }

        const string RuntimePanelName = "EraserWarsPanelRuntime";

        static PanelSettings sourceAsset;
        static PanelSettings sharedRuntimePanel;

        public static void ApplyVolume()
        {
            AudioListener.volume = Current.Master;
        }

        // Clone the shared PanelSettings so play mode does not write the asset.
        public static void ApplyUiScale(UIDocument menuDocument)
        {
            if (menuDocument == null)
            {
                return;
            }

            if (sourceAsset == null)
            {
                var current = menuDocument.panelSettings;
                if (current == null || current.name == RuntimePanelName)
                {
                    return;
                }

                sourceAsset = current;
            }

            if (sharedRuntimePanel == null)
            {
                sharedRuntimePanel = Object.Instantiate(sourceAsset);
                sharedRuntimePanel.name = RuntimePanelName;
            }

            var resolution = new Vector2Int(
                Mathf.Max(1, Mathf.RoundToInt(1920f / Current.UiScale)),
                Mathf.Max(1, Mathf.RoundToInt(1080f / Current.UiScale)));
            if (sharedRuntimePanel.referenceResolution != resolution)
            {
                sharedRuntimePanel.referenceResolution = resolution;
            }

            var documents = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include);
            for (var i = 0; i < documents.Length; i++)
            {
                var document = documents[i];
                if (document != null && document.panelSettings == sourceAsset)
                {
                    document.panelSettings = sharedRuntimePanel;
                }
            }
        }

        public GameSettings Clamped() => new(
            SnapVolume(Master),
            SnapVolume(Bgm),
            SnapVolume(Se),
            SnapFlick(FlickScale),
            SnapTurnSeconds(TurnSeconds),
            CameraShake,
            SnapUiScale(UiScale),
            RuleSet.WinModeFromPref((int)WinMode),
            ReduceMotion,
            ColorblindSeats);

        public static float SnapVolume(float volume) => Mathf.Clamp01(volume);

        public static float SnapFlick(float scale) => Mathf.Clamp(scale, 0.5f, 1.5f);

        public static int SnapTurnSeconds(float seconds)
        {
            var snapped = Mathf.RoundToInt(seconds / 5f) * 5;
            return Mathf.Clamp(snapped, 10, 60);
        }

        public static float SnapUiScale(float scale) => LargeUiScale;
    }
}
