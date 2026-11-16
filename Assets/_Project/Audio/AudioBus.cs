using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Audio
{
    public enum AudioCue
    {
        UiClick,
        Flick,
        Collision,
        Fall,
        Pass,
    }

    public sealed class AudioBus : MonoBehaviour
    {
        const int SampleRate = 22050;
        const string BoundClass = "audio-bus-bound";

        static AudioClip bgmClip;
        static AudioClip uiClickClip;
        static AudioClip flickClip;
        static AudioClip collisionClip;
        static AudioClip fallClip;
        static AudioClip passClip;

        AudioSource bgmSource;
        AudioSource seSource;

        public static AudioBus Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            Ensure();
        }

        public static AudioBus Ensure()
        {
            if (Instance != null)
            {
                Instance.Install();
                return Instance;
            }

            var existing = FindAnyObjectByType<AudioBus>(FindObjectsInactive.Include);
            if (existing != null)
            {
                existing.Install();
                return existing;
            }

            var go = new GameObject(nameof(AudioBus));
            var bus = go.AddComponent<AudioBus>();
            bus.Install();
            return bus;
        }

        public void ApplyVolumes()
        {
            bgmSource.volume = GameSettings.LoadBgmGain();
            seSource.volume = GameSettings.LoadSeGain();
        }

        public void Play(AudioCue cue)
        {
            seSource.volume = GameSettings.LoadSeGain();
            seSource.clip = ClipFor(cue);
            seSource.PlayOneShot(seSource.clip);
        }

        public void BindRoot(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            if (!root.ClassListContains(BoundClass))
            {
                root.AddToClassList(BoundClass);
                root.RegisterCallback<ClickEvent>(OnUiClick);
            }

            BindSlider(root, GameSettings.BgmSliderName, OnBgmSlider);
            BindSlider(root, GameSettings.SeSliderName, OnSeSlider);
        }

        bool installed;

        void Awake()
        {
            Install();
        }

        void Install()
        {
            if (installed)
            {
                return;
            }

            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }

                return;
            }

            Instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (FindAnyObjectByType<AudioListener>() == null)
            {
                gameObject.AddComponent<AudioListener>();
            }

            bgmSource = gameObject.AddComponent<AudioSource>();
            seSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            seSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
            seSource.spatialBlend = 0f;
            bgmSource.clip = BgmClip();
            ApplyVolumes();
            installed = true;
        }

        void Start()
        {
            ScanUi();
            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }

        void LateUpdate()
        {
            ApplyVolumes();
            ScanUi();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void ScanUi()
        {
            var docs = FindObjectsByType<UIDocument>(FindObjectsInactive.Include);
            for (var i = 0; i < docs.Length; i++)
            {
                BindRoot(docs[i].rootVisualElement);
            }
        }

        static void BindSlider(VisualElement root, string sliderName, EventCallback<ChangeEvent<float>> handler)
        {
            var slider = root.Q<Slider>(sliderName);
            if (slider == null || slider.ClassListContains(BoundClass))
            {
                return;
            }

            slider.AddToClassList(BoundClass);
            slider.RegisterValueChangedCallback(handler);
        }

        void OnBgmSlider(ChangeEvent<float> evt)
        {
            GameSettings.SaveBgmPercent(evt.newValue);
            ApplyVolumes();
        }

        void OnSeSlider(ChangeEvent<float> evt)
        {
            GameSettings.SaveSePercent(evt.newValue);
            ApplyVolumes();
        }

        void OnUiClick(ClickEvent evt)
        {
            if (evt.target is not VisualElement target || !IsButton(target))
            {
                return;
            }

            Play(AudioCue.UiClick);
        }

        static bool IsButton(VisualElement element)
        {
            for (var node = element; node != null; node = node.parent)
            {
                if (node is Button)
                {
                    return true;
                }
            }

            return false;
        }

        static AudioClip ClipFor(AudioCue cue)
        {
            switch (cue)
            {
                case AudioCue.UiClick:
                    return UiClickClip();
                case AudioCue.Flick:
                    return FlickClip();
                case AudioCue.Collision:
                    return CollisionClip();
                case AudioCue.Fall:
                    return FallClip();
                case AudioCue.Pass:
                    return PassClip();
                default:
                    throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
            }
        }

        static AudioClip BgmClip()
        {
            if (bgmClip == null)
            {
                bgmClip = BuildBgm();
            }

            return bgmClip;
        }

        static AudioClip UiClickClip()
        {
            if (uiClickClip == null)
            {
                uiClickClip = BuildClick();
            }

            return uiClickClip;
        }

        static AudioClip FlickClip()
        {
            if (flickClip == null)
            {
                flickClip = BuildFlick();
            }

            return flickClip;
        }

        static AudioClip CollisionClip()
        {
            if (collisionClip == null)
            {
                collisionClip = BuildCollision();
            }

            return collisionClip;
        }

        static AudioClip FallClip()
        {
            if (fallClip == null)
            {
                fallClip = BuildFall();
            }

            return fallClip;
        }

        static AudioClip BuildBgm()
        {
            var data = new float[SampleRate];
            AddNote(data, 0.00, 0.22, 523.25, 0.28f);
            AddNote(data, 0.25, 0.22, 659.25, 0.28f);
            AddNote(data, 0.50, 0.22, 783.99, 0.28f);
            AddNote(data, 0.75, 0.22, 659.25, 0.28f);
            data[0] = 0f;
            data[data.Length - 1] = 0f;
            return MakeClip("bgm-loop", data);
        }

        static AudioClip BuildClick()
        {
            var data = new float[(int)(0.045 * SampleRate)];
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (double)SampleRate;
                var env = (float)Math.Exp(-t * 70.0) * (1f - (float)i / data.Length);
                data[i] = env * 0.55f * (float)Math.Sin(2 * Math.PI * 1800 * t);
            }

            data[0] = 0f;
            data[data.Length - 1] = 0f;
            return MakeClip("ui-click", data);
        }

        static AudioClip BuildFlick()
        {
            var data = new float[(int)(0.11 * SampleRate)];
            var rng = new System.Random(20261116);
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (double)SampleRate;
                var env = (float)Math.Sin(Math.PI * i / (data.Length - 1));
                var noise = (float)(rng.NextDouble() * 2 - 1);
                var hz = 640.0 - 3600.0 * t;
                if (hz < 80.0)
                {
                    hz = 80.0;
                }

                var tone = (float)Math.Sin(2 * Math.PI * hz * t);
                data[i] = env * (0.35f * noise + 0.4f * tone);
            }

            data[0] = 0f;
            data[data.Length - 1] = 0f;
            return MakeClip("flick", data);
        }

        static AudioClip BuildCollision()
        {
            var data = new float[(int)(0.07 * SampleRate)];
            var rng = new System.Random(20261117);
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (double)SampleRate;
                var env = (float)Math.Exp(-t * 48.0);
                var noise = (float)(rng.NextDouble() * 2 - 1);
                var thump = (float)Math.Sin(2 * Math.PI * 210.0 * t);
                data[i] = env * (0.42f * noise + 0.5f * thump);
            }

            data[0] = 0f;
            data[data.Length - 1] = 0f;
            return MakeClip("eraser-hit", data);
        }

        static AudioClip PassClip()
        {
            if (passClip == null)
            {
                passClip = BuildPass();
            }

            return passClip;
        }

        static AudioClip BuildFall()
        {
            var data = new float[(int)(0.22 * SampleRate)];
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (double)SampleRate;
                var env = (float)Math.Exp(-t * 11.0);
                var body = (float)Math.Sin(2 * Math.PI * 74.0 * t);
                var knock = (float)Math.Sin(2 * Math.PI * 155.0 * t);
                data[i] = env * (0.72f * body + 0.22f * knock);
            }

            data[0] = 0f;
            data[data.Length - 1] = 0f;
            return MakeClip("floor-hit", data);
        }

        static AudioClip BuildPass()
        {
            var data = new float[(int)(0.18 * SampleRate)];
            AddNote(data, 0.00, 0.08, 988.0, 0.45f);
            AddNote(data, 0.08, 0.10, 740.0, 0.40f);
            data[0] = 0f;
            data[data.Length - 1] = 0f;
            return MakeClip("turn-pass", data);
        }

        static void AddNote(float[] data, double start, double duration, double hz, float amp)
        {
            var i0 = (int)Math.Round(start * SampleRate);
            var n = (int)Math.Round(duration * SampleRate);
            for (var i = 0; i < n; i++)
            {
                var index = i0 + i;
                if ((uint)index >= (uint)data.Length)
                {
                    break;
                }

                var u = n == 1 ? 0d : i / (double)(n - 1);
                var env = (float)Math.Sin(Math.PI * u);
                var t = i / (double)SampleRate;
                data[index] += amp * env * (float)Math.Sin(2 * Math.PI * hz * t);
            }
        }

        static AudioClip MakeClip(string clipName, float[] data)
        {
            var clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
