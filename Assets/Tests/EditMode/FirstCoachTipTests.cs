using System.Collections.Generic;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class FirstCoachTipTests
    {
        const string MenuPath = "Assets/UI/StartMenu.uxml";
        const string Copy = "カードを1枚出して、ドラッグではじく。";

        readonly List<Object> owned = new();
        GameObject uiHost;
        bool hadPref;
        int prefValue;
        float volume;

        [SetUp]
        public void SetUp()
        {
            hadPref = PlayerPrefs.HasKey(FirstCoachTip.PrefKey);
            prefValue = PlayerPrefs.GetInt(FirstCoachTip.PrefKey);
            volume = AudioListener.volume;
            PlayerPrefs.DeleteKey(FirstCoachTip.PrefKey);
        }

        [TearDown]
        public void TearDown()
        {
            if (uiHost != null)
            {
                Object.DestroyImmediate(uiHost);
                uiHost = null;
            }

            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();

            if (hadPref)
            {
                PlayerPrefs.SetInt(FirstCoachTip.PrefKey, prefValue);
            }
            else
            {
                PlayerPrefs.DeleteKey(FirstCoachTip.PrefKey);
            }

            PlayerPrefs.Save();
            AudioListener.volume = volume;
            GameSettings.Load();
        }

        [Test]
        public void HomeTip_ShowsTheCardAndFlickLine_WhileStartStaysLive()
        {
            var root = MountMenu();
            var start = root.Q<Button>("start-match");
            var tip = root.Q(FirstCoachTip.StripName);

            Assert.AreEqual(Copy, root.Q<Label>("home-tip-copy").text);
            Assert.AreEqual("わかった", root.Q<Button>(FirstCoachTip.DismissName).text);
            Assert.IsFalse(tip.ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.Flex, tip.style.display.value);
            Assert.IsTrue(tip.parent.ClassListContains("action-stack"));
            Assert.Less(tip.parent.IndexOf(tip), tip.parent.IndexOf(start));
            Assert.AreNotEqual(Position.Absolute, tip.resolvedStyle.position);
            Assert.IsTrue(start.enabledSelf);
            Assert.AreEqual("マッチ開始", start.text);
            Assert.IsFalse(start.ClassListContains("hidden"));
            Assert.IsFalse(PlayerPrefs.HasKey(FirstCoachTip.PrefKey));

            var startClicks = 0;
            start.RegisterCallback<ClickEvent>(evt =>
            {
                startClicks++;
                evt.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);
            Click(start);

            Assert.AreEqual(1, startClicks, "Match start still receives a click while the tip is up.");
            Assert.IsFalse(PlayerPrefs.HasKey(FirstCoachTip.PrefKey));
            Assert.IsFalse(tip.ClassListContains("hidden"));
            Assert.IsTrue(start.enabledSelf);
        }

        [Test]
        public void HomeTip_HidesAfterDismiss_AndStaysHiddenOnTheNextVisit()
        {
            var root = MountMenu();
            var start = root.Q<Button>("start-match");

            Click(root.Q<Button>(FirstCoachTip.DismissName));
            Click(root.Q<Button>(FirstCoachTip.DismissName));

            Assert.AreEqual(1, PlayerPrefs.GetInt(FirstCoachTip.PrefKey));
            Assert.IsTrue(root.Q(FirstCoachTip.StripName).ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.None, root.Q(FirstCoachTip.StripName).style.display.value);
            Assert.IsTrue(start.enabledSelf);
            Assert.AreEqual("マッチ開始", start.text);

            Object.DestroyImmediate(uiHost);
            uiHost = null;
            root = MountMenu();

            Assert.IsTrue(root.Q(FirstCoachTip.StripName).ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.None, root.Q(FirstCoachTip.StripName).style.display.value);
            Assert.IsTrue(root.Q<Button>("start-match").enabledSelf);
        }

        VisualElement MountMenu()
        {
            uiHost = new GameObject("home-tip-ui");
            var document = uiHost.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            owned.Add(settings);
            // ApplyUiScale caches any other panel name for the rest of the editor session.
            settings.name = "EraserWarsPanelRuntime";
            document.panelSettings = settings;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuPath);
            Assert.IsNotNull(document.rootVisualElement, "The menu tree has to exist before the controller enables.");
            var menu = uiHost.AddComponent<StartMenuController>();
            menu.ShowMenu();

            var root = document.rootVisualElement;
            Assert.IsNotNull(root);
            Assert.IsNotNull(root.Q(FirstCoachTip.StripName).panel, "The tip needs a panel so a click reaches it.");
            return root;
        }

        static void Click(VisualElement element)
        {
            using var click = ClickEvent.GetPooled();
            click.target = element;
            element.SendEvent(click);
        }
    }
}
