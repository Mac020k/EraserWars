using System.Collections;
using EraserWars.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.PlayMode
{
    public sealed class AudioBusPlayModeTests
    {
        bool hadBgm;
        float prevBgm;

        [SetUp]
        public void SetUp()
        {
            hadBgm = PlayerPrefs.HasKey(GameSettings.BgmPercentKey);
            prevBgm = PlayerPrefs.GetFloat(GameSettings.BgmPercentKey);
            PlayerPrefs.DeleteKey(GameSettings.BgmPercentKey);
            if (AudioBus.Instance != null)
            {
                Object.Destroy(AudioBus.Instance.gameObject);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (AudioBus.Instance != null)
            {
                Object.Destroy(AudioBus.Instance.gameObject);
            }

            if (hadBgm)
            {
                PlayerPrefs.SetFloat(GameSettings.BgmPercentKey, prevBgm);
            }
            else
            {
                PlayerPrefs.DeleteKey(GameSettings.BgmPercentKey);
            }
        }

        [UnityTest]
        public IEnumerator BgmLoopPlaysAtTheDefaultGain()
        {
            var bus = AudioBus.Ensure();
            yield return null;
            AudioSource bgm = null;
            var sources = bus.GetComponents<AudioSource>();
            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i].loop)
                {
                    bgm = sources[i];
                }
            }

            Assert.NotNull(bgm);
            Assert.IsTrue(bgm.isPlaying);
            Assert.AreEqual("bgm-loop", bgm.clip.name);
            Assert.AreEqual(0.7f, bgm.volume, 0.001f);
        }
    }
}
