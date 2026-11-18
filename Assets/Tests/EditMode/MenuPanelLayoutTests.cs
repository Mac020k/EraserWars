using System;
using System.Reflection;
using EraserWars.CameraRig;
using EraserWars.Gameplay;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class MenuPanelLayoutTests
    {
        const string MenuPath = "Assets/UI/StartMenu.uxml";

        GameObject uiHost;
        GameObject cameraHost;
        GameObject directorHost;
        PanelSettings panelSettings;
        RenderTexture panelTexture;
        float volume;
        bool hadReduceMotion;
        int reduceMotion;

        [SetUp]
        public void SetUp()
        {
            volume = AudioListener.volume;
            hadReduceMotion = PlayerPrefs.HasKey(GameSettings.Keys.ReduceMotion);
            reduceMotion = PlayerPrefs.GetInt(GameSettings.Keys.ReduceMotion, 0);
            PlayerPrefs.SetInt(GameSettings.Keys.ReduceMotion, 0);
            GameSettings.Load();
        }

        [TearDown]
        public void TearDown()
        {
            if (uiHost != null)
            {
                UnityEngine.Object.DestroyImmediate(uiHost);
            }

            if (directorHost != null)
            {
                UnityEngine.Object.DestroyImmediate(directorHost);
            }

            if (cameraHost != null)
            {
                UnityEngine.Object.DestroyImmediate(cameraHost);
            }

            if (panelTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(panelTexture);
            }

            if (panelSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(panelSettings);
            }

            AudioListener.volume = volume;
            if (hadReduceMotion)
            {
                PlayerPrefs.SetInt(GameSettings.Keys.ReduceMotion, reduceMotion);
            }
            else
            {
                PlayerPrefs.DeleteKey(GameSettings.Keys.ReduceMotion);
            }

            GameSettings.Load();
        }

        [Test]
        public void DestinationPagesFillTheHostAndScrollInside()
        {
            var menu = Mount();
            menu.OpenOptions();
            var root = menu.GetComponent<UIDocument>().rootVisualElement;
            var host = root.Q("menu-content");
            host.style.width = 1000;
            host.style.height = 800;
            Layout(root);
            menu.OpenOptions();
            Layout(root);
            root = menu.GetComponent<UIDocument>().rootVisualElement;
            host = root.Q("menu-content");
            var page = root.Q("options-panel");
            Assert.IsFalse(page.ClassListContains("hidden"));
            Assert.IsFalse(page is ScrollView);
            Assert.AreEqual(StyleKeyword.Auto, page.style.minWidth.keyword);
            Assert.NotNull(root.Q<ScrollView>("options-scroll"));

            var hostWidth = host.resolvedStyle.width;
            if (hostWidth > 2f)
            {
                Assert.AreEqual(0.92f, page.resolvedStyle.width / hostWidth, 0.02f);
                Assert.AreEqual(0.92f, page.resolvedStyle.height / host.resolvedStyle.height, 0.02f);
            }
            else
            {
                Assert.AreEqual(LengthUnit.Percent, page.style.left.value.unit);
                Assert.AreEqual(4f, page.style.left.value.value, 0.01f);
                Assert.AreEqual(StyleKeyword.Auto, page.style.width.keyword);
            }

            var characters = root.Q<ScrollView>("collection-strip-scroll");
            Assert.AreEqual(ScrollViewMode.Horizontal, characters.mode);
            Assert.AreEqual(ScrollerVisibility.AlwaysVisible, characters.horizontalScrollerVisibility);

            var seats = root.Q<ScrollView>("seat-strip-scroll");
            Assert.AreEqual(ScrollViewMode.Horizontal, seats.mode);
            Assert.AreEqual(ScrollerVisibility.Auto, seats.horizontalScrollerVisibility);
            Assert.NotNull(seats.Q("seat-strip"));

            var cards = root.Q<ScrollView>("deck-card-list");
            Assert.AreEqual(ScrollViewMode.Vertical, cards.mode);
            Assert.AreEqual(ScrollerVisibility.Hidden, cards.horizontalScrollerVisibility);
            Assert.IsTrue(cards.ClassListContains("deck-grid"));

            var chips = root.Q<ScrollView>("deck-character-list");
            Assert.AreEqual(ScrollViewMode.Horizontal, chips.mode);
            Assert.IsTrue(chips.ClassListContains("deck-chip-strip"));

            menu.OpenCollection();
            Layout(root);
            root = menu.GetComponent<UIDocument>().rootVisualElement;
            var strip = root.Q("collection-strip");
            if (root.Q("collection-panel").resolvedStyle.width > 2f)
            {
                Assert.GreaterOrEqual(strip.resolvedStyle.paddingTop, 14f);
            }
        }

        [Test]
        public void DestinationPagesIgnorePointerOrbitUntilHome()
        {
            var menu = Mount();
            var orbit = cameraHost.GetComponent<OrbitCamera>();
            var director = directorHost.GetComponent<MatchDirector>();
            orbit.FrameDeskOverview();

            menu.OpenOptions();
            var parked = orbit.transform.position;
            orbit.ApplyPointerOrbit(true, new Vector2(90f, 24f), 240f);
            orbit.Snap();
            Assert.Less(Vector3.Distance(parked, orbit.transform.position), 0.001f);

            menu.GoHome();
            var before = PlanarAngle(orbit.transform.position);
            orbit.Tick(0f, 1f, 1f);
            Assert.AreEqual(8f, Mathf.DeltaAngle(before, PlanarAngle(orbit.transform.position)), 0.2f);

            director.SetStartMenuOpen(false);
            var held = orbit.transform.position;
            orbit.Tick(1f, 1f, 1f);
            Assert.Less(Vector3.Distance(held, orbit.transform.position), 0.001f);

            var boom = Vector3.Distance(orbit.transform.position, Vector3.zero);
            orbit.ApplyPointerOrbit(false, Vector2.zero, 240f);
            orbit.Snap();
            Assert.Greater(Mathf.Abs(boom - Vector3.Distance(orbit.transform.position, Vector3.zero)), 0.02f);
        }

        StartMenuController Mount()
        {
            cameraHost = new GameObject("menu-cam");
            var orbit = cameraHost.AddComponent<OrbitCamera>();
            directorHost = new GameObject("menu-director");
            var director = directorHost.AddComponent<MatchDirector>();
            SetField(director, "orbitCamera", orbit);

            uiHost = new GameObject("menu-ui");
            uiHost.SetActive(false);
            var document = uiHost.AddComponent<UIDocument>();
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            panelSettings.scale = 1f;
            panelTexture = new RenderTexture(1280, 720, 0);
            panelSettings.targetTexture = panelTexture;
            document.panelSettings = panelSettings;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuPath);
            var menu = uiHost.AddComponent<StartMenuController>();
            SetField(menu, "document", document);
            SetField(menu, "director", director);
            uiHost.SetActive(true);
            Assert.NotNull(document.rootVisualElement);
            return menu;
        }

        static void Layout(VisualElement element)
        {
            var panel = element.panel;
            Assert.NotNull(panel);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var validate = panel.GetType().GetMethod("ValidateLayout", flags, null, Type.EmptyTypes, null);
            validate?.Invoke(panel, null);
        }

        static void SetField(UnityEngine.Object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, name);
            field.SetValue(target, value);
        }

        static float PlanarAngle(Vector3 position)
        {
            return Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg;
        }
    }
}
