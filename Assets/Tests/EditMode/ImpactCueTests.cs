using System.Collections.Generic;
using EraserWars.Audio;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class ImpactCueTests
    {
        readonly List<GameObject> spawned = new();
        bool hadSe;
        float prevSe;

        [SetUp]
        public void SetUp()
        {
            hadSe = PlayerPrefs.HasKey(GameSettings.SePercentKey);
            prevSe = PlayerPrefs.GetFloat(GameSettings.SePercentKey);
            PlayerPrefs.DeleteKey(GameSettings.SePercentKey);
            if (AudioBus.Instance != null)
            {
                Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] != null)
                {
                    Object.DestroyImmediate(spawned[i]);
                }
            }

            spawned.Clear();
            if (AudioBus.Instance != null)
            {
                Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }

            if (hadSe)
            {
                PlayerPrefs.SetFloat(GameSettings.SePercentKey, prevSe);
            }
            else
            {
                PlayerPrefs.DeleteKey(GameSettings.SePercentKey);
            }
        }

        [Test]
        public void AcceptedPairPlaysCollisionAtTheSavedSeGain()
        {
            GameSettings.SaveSePercent(20f);
            var relay = Track(new GameObject("pair")).AddComponent<EraserCollisionRelay>();
            relay.PlayAcceptedPairCue();
            var se = SeSource();
            Assert.AreEqual(0.2f, se.volume, 0.001f);
            Assert.AreEqual("eraser-hit", se.clip.name);
        }

        [Test]
        public void FloorContactPlaysFallOnceUntilTheFallResets()
        {
            GameSettings.SaveSePercent(50f);
            var detector = Track(new GameObject("eraser")).AddComponent<FalloutDetector>();
            var desk = Box("Desk");
            var floor = Box("ClassroomFloor");
            var slab = Box("slab", floor.transform);

            detector.TryPlayFloorHit(null);
            detector.TryPlayFloorHit(desk);
            Assert.IsTrue(AudioBus.Instance == null);

            detector.TryPlayFloorHit(slab);
            var se = SeSource();
            Assert.AreEqual("floor-hit", se.clip.name);
            Assert.AreEqual(0.5f, se.volume, 0.001f);

            GameSettings.SaveSePercent(10f);
            detector.TryPlayFloorHit(floor);
            Assert.AreEqual(0.5f, se.volume, 0.001f);

            detector.ResetReport();
            detector.TryPlayFloorHit(floor);
            Assert.AreEqual(0.1f, se.volume, 0.001f);
            Assert.AreEqual("floor-hit", se.clip.name);
        }

        GameObject Track(GameObject go)
        {
            spawned.Add(go);
            return go;
        }

        BoxCollider Box(string objectName, Transform parent = null)
        {
            var go = Track(new GameObject(objectName));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go.AddComponent<BoxCollider>();
        }

        static AudioSource SeSource()
        {
            var bus = AudioBus.Instance;
            Assert.NotNull(bus);
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
    }
}
