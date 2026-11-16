using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Audio;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.EditMode
{
    public sealed class CardCutInCueTests
    {
        readonly List<UnityEngine.Object> owned = new();
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
                UnityEngine.Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
            if (AudioBus.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(AudioBus.Instance.gameObject);
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
        public void RejectedPlayDoesNotStartACutInOrASlash()
        {
            GameSettings.SaveSePercent(40f);
            var director = Track(DirectorHost());
            var card = GiveCommon(director, "ワックス");
            StartMatch(director);

            var player = director.State.CurrentPlayerId;
            Assert.IsTrue(director.State.Players[player.Value].Hand.Contains(card.Id));
            director.PlayCard(999);

            Assert.AreEqual(Phase.CardSelect, director.State.Phase);
            Assert.IsFalse(director.IsCardCutInShowing);
            Assert.IsTrue(AudioBus.Instance == null);
        }

        [Test]
        public void CommonCutInPlaysCardSlashAtTheSavedSeGain()
        {
            GameSettings.SaveSePercent(40f);
            var director = Track(DirectorHost());
            var card = GiveCommon(director, "ワックス");
            StartMatch(director);

            director.PlayCard(card.IdValue);

            Assert.IsTrue(director.TryGetCardCutIn(out var cut));
            Assert.IsFalse(cut.Signature);
            var se = SeSource();
            Assert.AreEqual(0.4f, se.volume, 0.001f);
            AssertSlash(se.clip, "card-play");
        }

        [Test]
        public void SignatureCutInPlaysABrighterSlashAtTheSameGain()
        {
            GameSettings.SaveSePercent(40f);
            var director = Track(DirectorHost());
            var card = GiveSignature(director, "全力スマッシュ");
            StartMatch(director);

            director.PlayCard(card.IdValue);

            Assert.IsTrue(director.TryGetCardCutIn(out var cut));
            Assert.IsTrue(cut.Signature);
            var se = SeSource();
            Assert.AreEqual(0.4f, se.volume, 0.001f);
            AssertSlash(se.clip, "card-play-signature");
            var signatureClip = se.clip;

            AudioBus.Instance.Play(AudioCue.CardPlay);
            Assert.AreEqual(0.4f, se.volume, 0.001f);
            AssertSlash(se.clip, "card-play");
            Assert.Greater(Brightness(signatureClip), Brightness(se.clip));
            Assert.AreEqual(signatureClip.length, se.clip.length, 0.0001f);
        }

        MatchDirector DirectorHost()
        {
            var root = Track(new GameObject("CardCutInCueRig"));
            var surface = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            surface.name = "PlaySurface";
            surface.transform.SetParent(root.transform, false);

            var template = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            template.name = "EraserTemplate";
            template.SetActive(false);
            template.transform.SetParent(root.transform, false);
            template.AddComponent<Rigidbody>();
            template.AddComponent<FalloutDetector>();
            var eraserPrefab = template.AddComponent<EraserBody>();

            var spawns = new Transform[2];
            for (var i = 0; i < spawns.Length; i++)
            {
                var spawn = Track(new GameObject($"Spawn_{i}"));
                spawn.transform.SetParent(root.transform, false);
                spawn.transform.position = new Vector3(i * 0.3f, 0.8f, 0f);
                spawns[i] = spawn.transform;
            }

            var directorGo = Track(new GameObject("Director"));
            directorGo.SetActive(false);
            directorGo.transform.SetParent(root.transform, false);
            var flickApplier = directorGo.AddComponent<FlickApplier>();
            var settleWatcher = directorGo.AddComponent<SettleWatcher>();
            var director = directorGo.AddComponent<MatchDirector>();
            SetField(director, "flickApplier", flickApplier);
            SetField(director, "settleWatcher", settleWatcher);
            SetField(director, "spawnPoints", spawns);
            SetField(director, "eraserPrefab", eraserPrefab);
            SetField(director, "showDebug", false);
            directorGo.SetActive(true);
            return director;
        }

        CardDefinition GiveCommon(MatchDirector director, string name)
        {
            var card = OwnedCard(name, false);
            director.SetSeatRoster(new CharacterDefinition[] { null, null });
            director.SetSeatCommons(0, new[] { card });
            return card;
        }

        CardDefinition GiveSignature(MatchDirector director, string name)
        {
            var card = OwnedCard(name, true);
            var character = Track(ScriptableObject.CreateInstance<CharacterDefinition>());
            character.DisplayNameJa = "赤星";
            character.NameKey = "akaboshi";
            character.SignatureA = card;
            director.SetSeatRoster(new[] { character, character });
            return card;
        }

        CardDefinition OwnedCard(string name, bool signature)
        {
            var card = Track(ScriptableObject.CreateInstance<CardDefinition>());
            card.IdValue = signature ? 42 : 7;
            card.DisplayNameJa = name;
            card.NameKey = name;
            card.IsSignature = signature;
            card.FamilyKey = signature ? CardFamilies.Signature : CardFamilies.Stationery;
            card.EffectKinds = new List<string>();
            return card;
        }

        void StartMatch(MatchDirector director)
        {
            director.SetStartMenuOpen(false);
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
            director.StartHotseat(2);
            for (var i = 0; i < director.Erasers.Count; i++)
            {
                Track(director.Erasers[i].gameObject);
            }

            Assert.AreEqual(Phase.CardSelect, director.State.Phase);
        }

        T Track<T>(T obj) where T : UnityEngine.Object
        {
            owned.Add(obj);
            return obj;
        }

        static void AssertSlash(AudioClip clip, string clipName)
        {
            Assert.AreEqual(clipName, clip.name);
            Assert.Greater(clip.length, 0.05f);
            Assert.Less(clip.length, CardCutIn.SlashSeconds);
            Assert.Greater(Rms(clip), 0.02f);
            var data = Samples(clip);
            Assert.AreEqual(0f, data[0], 0.0001f);
            Assert.AreEqual(0f, data[data.Length - 1], 0.0001f);
        }

        static float Brightness(AudioClip clip)
        {
            var data = Samples(clip);
            double energy = 0;
            double stepEnergy = 0;
            for (var i = 1; i < data.Length; i++)
            {
                var delta = data[i] - data[i - 1];
                stepEnergy += delta * delta;
                energy += data[i] * data[i];
            }

            Assert.Greater(energy, 0d);
            return (float)(stepEnergy / energy);
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

        static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{target.GetType().Name}.{name} was renamed.");
            field.SetValue(target, value);
        }
    }
}
