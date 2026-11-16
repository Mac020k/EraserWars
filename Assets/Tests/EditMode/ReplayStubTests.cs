using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Audio;
using EraserWars.Core;
using EraserWars.Gameplay;
using EraserWars.Persistence;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.EditMode
{
    public sealed class ReplayStubTests
    {
        string directory;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "eraserwars-replay-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (AudioBus.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(AudioBus.Instance.gameObject);
            }

            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void RoundTripKeepsAimPowerContactAndSeat()
        {
            var command = new FlickCommand(new PlayerId(2), 0.5f, -0.25f, 0.75f, 0.125f, -0.5f, 0.2f);
            var text = ReplayStub.Remember(command, directory);
            var loaded = ReplayStub.Read(ReplayStub.PathFor(directory));

            Assert.AreEqual(2, loaded.Seat);
            Assert.AreEqual(0.5f, loaded.AimX);
            Assert.AreEqual(-0.25f, loaded.AimY);
            Assert.AreEqual(0.75f, loaded.Power);
            Assert.AreEqual(0.125f, loaded.ContactX);
            Assert.AreEqual(-0.5f, loaded.ContactY);
            Assert.AreEqual(text, File.ReadAllText(ReplayStub.PathFor(directory)));
            StringAssert.Contains("seat=2", text);
            StringAssert.Contains("power=0.75", text);
            StringAssert.Contains("aim=0.5 -0.25", text);
            StringAssert.Contains("contact=0.125 -0.5", text);
        }

        [Test]
        public void ASecondFlickReplacesTheFirst()
        {
            ReplayStub.Remember(new FlickCommand(new PlayerId(0), 1f, 0f, 0.2f, 0f, 0f, 0.1f), directory);
            ReplayStub.Remember(new FlickCommand(new PlayerId(1), -1f, 0.5f, 1f, 0.25f, 0.25f, 0.3f), directory);

            var loaded = ReplayStub.Read(ReplayStub.PathFor(directory));
            Assert.AreEqual(1, loaded.Seat);
            Assert.AreEqual(-1f, loaded.AimX);
            Assert.AreEqual(0.5f, loaded.AimY);
            Assert.AreEqual(1f, loaded.Power);
            Assert.AreEqual(0.25f, loaded.ContactX);
            Assert.AreEqual(0.25f, loaded.ContactY);
            Assert.AreEqual(1, Directory.GetFiles(directory, "last-flick.txt").Length);
        }

        [Test]
        public void TruncatedAndUnknownFieldsThrow()
        {
            var truncated = Path.Combine(directory, "short.txt");
            File.WriteAllText(truncated, "eraserwars-flick 1\n");
            Assert.Throws<FormatException>(() => ReplayStub.Read(truncated));

            var unknown = Path.Combine(directory, "extra.txt");
            File.WriteAllText(unknown, "eraserwars-flick 1\nbonus=1\n");
            Assert.Throws<FormatException>(() => ReplayStub.Read(unknown));
        }

        [Test]
        public void OddAimAndASeatOutsideTheTableThrow()
        {
            var odd = Path.Combine(directory, "odd.txt");
            File.WriteAllText(odd, "eraserwars-flick 1\nseat=0\naim=1\npower=0.5\ncontact=0 0\n");
            Assert.Throws<FormatException>(() => ReplayStub.Read(odd));

            Assert.Throws<ArgumentException>(() =>
                ReplayStub.From(new FlickCommand(new PlayerId(4), 1f, 0f, 0.5f, 0f, 0f, 0.1f)));
        }

        [Test]
        public void AcceptedFlickOverwritesTheTempFileAndARejectedFlickLeavesIt()
        {
            var root = new GameObject("ReplayStubRig");
            try
            {
                var template = GameObject.CreatePrimitive(PrimitiveType.Cube);
                template.name = "EraserTemplate";
                template.SetActive(false);
                template.transform.SetParent(root.transform, false);
                template.AddComponent<Rigidbody>();
                var eraserPrefab = template.AddComponent<EraserBody>();

                var spawns = new Transform[2];
                for (var i = 0; i < spawns.Length; i++)
                {
                    spawns[i] = new GameObject("Spawn_" + i).transform;
                    spawns[i].SetParent(root.transform, false);
                    spawns[i].position = new Vector3(i * 0.3f, 0.8f, 0f);
                }

                var directorGo = new GameObject("Director");
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
                var physics = directorGo.GetComponent<EffectPhysicsSync>();
                if (physics == null)
                {
                    physics = directorGo.AddComponent<EffectPhysicsSync>();
                }

                SetField(director, "effectPhysics", physics);

                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, director.State.Phase);

                var path = ReplayStub.PathFor(ReplayStub.DefaultDirectory());
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                var seat = director.State.CurrentPlayerId;
                director.SubmitFlick(new FlickCommand(seat, 0.5f, -0.25f, 0.75f, 0.125f, -0.5f, 0.2f));
                var loaded = ReplayStub.Read(path);
                Assert.AreEqual(seat.Value, loaded.Seat);
                Assert.AreEqual(0.5f, loaded.AimX);
                Assert.AreEqual(-0.25f, loaded.AimY);
                Assert.AreEqual(0.75f, loaded.Power);
                Assert.AreEqual(0.125f, loaded.ContactX);
                Assert.AreEqual(-0.5f, loaded.ContactY);
                Assert.AreEqual(Phase.Simulating, director.State.Phase);

                director.SubmitFlick(new FlickCommand(seat, 1f, 0f, 0.1f, 0f, 0f, 0f));
                var still = ReplayStub.Read(path);
                Assert.AreEqual(0.5f, still.AimX);
                Assert.AreEqual(0.75f, still.Power);
                Assert.AreEqual(Phase.Simulating, director.State.Phase);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static void SetField(UnityEngine.Object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, name);
            field.SetValue(target, value);
        }
    }
}
