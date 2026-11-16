using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EraserWars.Tests.EditMode
{
    public sealed class EraserToonMaterialTests
    {
        const string PrefabPath = "Assets/Prefabs/Eraser.prefab";

        [Test]
        public void PrefabMaterialIsToonAndSeatTintStaysOnTheInstance()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, PrefabPath);
            var sharedRenderer = prefab.GetComponent<MeshRenderer>();
            Assert.IsNotNull(sharedRenderer);
            var shared = sharedRenderer.sharedMaterial;
            Assert.IsNotNull(shared);
            Assert.AreEqual("EraserWars/EraserToon", shared.shader.name);
            Assert.IsTrue(shared.HasProperty("_BaseColor"));
            Assert.IsTrue(shared.HasProperty("_Color"));
            Assert.IsTrue(shared.HasProperty("_EmissionColor"));
            Assert.GreaterOrEqual(shared.GetFloat("_Shade"), 0.5f);
            Assert.Less(shared.GetFloat("_OutlineWidth"), 0.004f);
            Assert.Greater(shared.GetFloat("_OutlineWidth"), 0f);
            var sharedColor = shared.GetColor("_BaseColor");

            var seat0 = SpawnAndTint(0);
            var seat1 = SpawnAndTint(1);
            Assert.AreEqual(0.92f, seat0.r, 0.001f);
            Assert.AreEqual(0.28f, seat0.g, 0.001f);
            Assert.AreEqual(0.35f, seat0.b, 0.001f);
            Assert.AreEqual(0.25f, seat1.r, 0.001f);
            Assert.AreEqual(0.45f, seat1.g, 0.001f);
            Assert.AreEqual(0.95f, seat1.b, 0.001f);
            Assert.AreEqual(sharedColor, shared.GetColor("_BaseColor"));
        }

        static Color SpawnAndTint(int playerIndex)
        {
            LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                var body = instance.GetComponent<EraserBody>();
                Assert.IsNotNull(body);
                var apply = typeof(MatchDirector).GetMethod(
                    "ApplyPlayerLook",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNotNull(apply, "ApplyPlayerLook was renamed.");
                apply.Invoke(null, new object[] { body, playerIndex });
                var material = instance.GetComponent<MeshRenderer>().material;
                var baseColor = material.GetColor("_BaseColor");
                var color = material.GetColor("_Color");
                Assert.AreEqual(baseColor, color);
                return baseColor;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
