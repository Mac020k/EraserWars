using System;
using EraserWars.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class AudioBusTests
    {
        bool hadBgm;
        bool hadSe;
        float prevBgm;
        float prevSe;

        [SetUp]
        public void SetUp()
        {
            hadBgm = PlayerPrefs.HasKey(GameSettings.BgmPercentKey);
            hadSe = PlayerPrefs.HasKey(GameSettings.SePercentKey);
            prevBgm = PlayerPrefs.GetFloat(GameSettings.BgmPercentKey);
            prevSe = PlayerPrefs.GetFloat(GameSettings.SePercentKey);
            PlayerPrefs.DeleteKey(GameSettings.BgmPercentKey);
            PlayerPrefs.DeleteKey(GameSettings.SePercentKey);
            DestroyBus();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyBus();
            Restore(GameSettings.BgmPercentKey, hadBgm, prevBgm);
            Restore(GameSettings.SePercentKey, hadSe, prevSe);
        }

        [Test]
        public void PercentKeysMatchTheOptionsContract()
        {
            Assert.AreEqual("eraserwars.settings.bgmPercent", GameSettings.BgmPercentKey);
            Assert.AreEqual("eraserwars.settings.sePercent", GameSettings.SePercentKey);
            Assert.AreEqual("opt-bgm", GameSettings.BgmSliderName);
            Assert.AreEqual("opt-se", GameSettings.SeSliderName);
            Assert.AreEqual(0f, GameSettings.PercentToGain(-20f), 0.0001f);
            Assert.AreEqual(1f, GameSettings.PercentToGain(140f), 0.0001f);
            Assert.AreEqual(0.7f, GameSettings.PercentToGain(70f), 0.0001f);
        }

        [Test]
        public void MissingPrefsUseTheSliderDefaults()
        {
            var bus = AudioBus.Ensure();
            bus.ApplyVolumes();
            Assert.AreEqual(0.7f, LoopSource(bus).volume, 0.001f);
            Assert.AreEqual(0.8f, OneShotSource(bus).volume, 0.001f);
        }

        [Test]
        public void SavedPercentsSetSourceVolumes()
        {
            GameSettings.SaveBgmPercent(0f);
            GameSettings.SaveSePercent(100f);
            var bus = AudioBus.Ensure();
            bus.ApplyVolumes();
            Assert.AreEqual(0f, LoopSource(bus).volume, 0.001f);
            Assert.AreEqual(1f, OneShotSource(bus).volume, 0.001f);
        }

        [Test]
        public void BgmClipLoopsAndIsNotSilent()
        {
            var bus = AudioBus.Ensure();
            var bgm = LoopSource(bus);
            Assert.IsTrue(bgm.loop);
            Assert.AreEqual("bgm-loop", bgm.clip.name);
            Assert.Greater(bgm.clip.length, 0.5f);
            Assert.Greater(Rms(bgm.clip), 0.02f);
            var data = Samples(bgm.clip);
            Assert.AreEqual(0f, data[0], 0.0001f);
            Assert.AreEqual(0f, data[data.Length - 1], 0.0001f);
        }

        [Test]
        public void FlickAndClickExposeDistinctCuesAtTheSeGain()
        {
            GameSettings.SaveSePercent(25f);
            var bus = AudioBus.Ensure();
            bus.Play(AudioCue.Flick);
            var se = OneShotSource(bus);
            Assert.AreEqual(0.25f, se.volume, 0.001f);
            Assert.AreEqual("flick", se.clip.name);
            Assert.Greater(Rms(se.clip), 0.02f);

            bus.Play(AudioCue.UiClick);
            Assert.AreEqual("ui-click", se.clip.name);
            Assert.Greater(Rms(se.clip), 0.02f);
            Assert.Less(se.clip.length, LoopSource(bus).clip.length);
        }

        [Test]
        public void SeSliderWritesPercentAndGain()
        {
            var bus = AudioBus.Ensure();
            var host = NewPanel(out var root);
            try
            {
                var slider = new Slider(0f, 100f) { name = GameSettings.SeSliderName, value = 80f };
                root.Add(slider);
                bus.BindRoot(root);
                slider.value = 40f;
                Assert.AreEqual(40f, PlayerPrefs.GetFloat(GameSettings.SePercentKey), 0.01f);
                Assert.AreEqual(0.4f, OneShotSource(bus).volume, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void BgmSliderWritesPercentAndGain()
        {
            var bus = AudioBus.Ensure();
            var host = NewPanel(out var root);
            try
            {
                var slider = new Slider(0f, 100f) { name = GameSettings.BgmSliderName, value = 70f };
                root.Add(slider);
                bus.BindRoot(root);
                slider.value = 10f;
                Assert.AreEqual(10f, PlayerPrefs.GetFloat(GameSettings.BgmPercentKey), 0.01f);
                Assert.AreEqual(0.1f, LoopSource(bus).volume, 0.001f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void ButtonClickUsesTheSavedSeGain()
        {
            var bus = AudioBus.Ensure();
            var host = NewPanel(out var root);
            try
            {
                var button = new Button { text = "ok" };
                root.Add(button);
                bus.BindRoot(root);
                GameSettings.SaveSePercent(15f);
                using (var evt = ClickEvent.GetPooled())
                {
                    evt.target = button;
                    button.SendEvent(evt);
                }

                Assert.AreEqual(0.15f, OneShotSource(bus).volume, 0.001f);
                Assert.AreEqual("ui-click", OneShotSource(bus).clip.name);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        static GameObject NewPanel(out VisualElement root)
        {
            var host = new GameObject("audio-ui-test");
            var keeper = host.AddComponent<PanelSettingsKeeper>();
            keeper.Settings = ScriptableObject.CreateInstance<PanelSettings>();
            var doc = host.AddComponent<UIDocument>();
            doc.panelSettings = keeper.Settings;
            root = doc.rootVisualElement;
            Assert.NotNull(root);
            return host;
        }

        sealed class PanelSettingsKeeper : MonoBehaviour
        {
            public PanelSettings Settings;

            void OnDestroy()
            {
                if (Settings != null)
                {
                    DestroyImmediate(Settings);
                }
            }
        }

        static AudioSource LoopSource(AudioBus bus)
        {
            var sources = bus.GetComponents<AudioSource>();
            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i].loop)
                {
                    return sources[i];
                }
            }

            Assert.Fail("looping BGM source missing");
            return null;
        }

        static AudioSource OneShotSource(AudioBus bus)
        {
            var sources = bus.GetComponents<AudioSource>();
            for (var i = 0; i < sources.Length; i++)
            {
                if (!sources[i].loop)
                {
                    return sources[i];
                }
            }

            Assert.Fail("SE source missing");
            return null;
        }

        static float Rms(AudioClip clip)
        {
            var data = Samples(clip);
            double sum = 0;
            for (var i = 0; i < data.Length; i++)
            {
                sum += data[i] * data[i];
            }

            return (float)Math.Sqrt(sum / data.Length);
        }

        static float[] Samples(AudioClip clip)
        {
            var data = new float[clip.samples * clip.channels];
            Assert.IsTrue(clip.GetData(data, 0));
            return data;
        }

        static void DestroyBus()
        {
            if (AudioBus.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }
        }

        static void Restore(string key, bool had, float value)
        {
            if (had)
            {
                PlayerPrefs.SetFloat(key, value);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}
