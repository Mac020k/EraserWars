using System.Collections.Generic;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class SecretRoomStubTests
    {
        const string MenuPath = "Assets/UI/StartMenu.uxml";
        const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        readonly List<Object> owned = new();
        GameObject uiHost;
        bool hadLocale;
        string localeValue;
        float volume;
        string clipboard;

        [SetUp]
        public void SetUp()
        {
            hadLocale = PlayerPrefs.HasKey(UiLocale.PrefKey);
            localeValue = PlayerPrefs.GetString(UiLocale.PrefKey, UiLocale.Japanese);
            volume = AudioListener.volume;
            clipboard = GUIUtility.systemCopyBuffer;
            PlayerPrefs.DeleteKey(UiLocale.PrefKey);
            UiLocale.Load();
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

            if (hadLocale)
            {
                PlayerPrefs.SetString(UiLocale.PrefKey, localeValue);
            }
            else
            {
                PlayerPrefs.DeleteKey(UiLocale.PrefKey);
            }

            PlayerPrefs.Save();
            UiLocale.Load();
            AudioListener.volume = volume;
            GameSettings.Load();
            GUIUtility.systemCopyBuffer = clipboard;
        }

        [Test]
        public void JoinCode_IsSixCharactersFromTheReadableAlphabet()
        {
            Assert.AreEqual(6, JoinCode.Length);
            Assert.AreEqual(Alphabet, JoinCode.Alphabet);
            Assert.IsNull(JoinCode.Normalize(null));
            Assert.IsNull(JoinCode.Normalize(""));
            Assert.IsNull(JoinCode.Normalize("ABC23"));
            Assert.IsNull(JoinCode.Normalize("ABC2345"));
            Assert.IsNull(JoinCode.Normalize("IIIIII"));
            Assert.IsNull(JoinCode.Normalize("000000"));
            Assert.IsNull(JoinCode.Normalize("ABC23!"));
            Assert.AreEqual("ABCDEF", JoinCode.Normalize(" abcdef "));
            Assert.AreEqual("ABC234", JoinCode.Normalize("abc234"));

            var random = new System.Random(7);
            for (var n = 0; n < 32; n++)
            {
                var code = JoinCode.Generate(random);
                Assert.AreEqual(code, JoinCode.Normalize(code), code);
                Assert.AreEqual(JoinCode.Length, code.Length, code);
                for (var i = 0; i < code.Length; i++)
                {
                    Assert.IsTrue(Alphabet.IndexOf(code[i]) >= 0, code);
                }
            }
        }

        [Test]
        public void SecretRoom_ShowsTheCodeField_AndCopiesWithoutASession()
        {
            var root = MountMenu();
            var panel = root.Q(SecretRoomPanel.PanelName);
            var home = root.Q("home-stack");
            var start = root.Q<Button>("start-match");

            Assert.IsTrue(panel.ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.None, panel.style.display.value);
            Assert.IsFalse(home.ClassListContains("hidden"));
            Assert.AreEqual("ひみつのへや", root.Q<Button>(SecretRoomPanel.OpenName).text);
            Assert.AreEqual("マッチ開始", start.text);
            Assert.IsTrue(start.enabledSelf);
            Assert.AreEqual(string.Empty, root.Q<TextField>(SecretRoomPanel.FieldName).value);

            Click(root.Q<Button>(SecretRoomPanel.OpenName));

            Assert.IsFalse(panel.ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.Flex, panel.style.display.value);
            Assert.IsTrue(home.ClassListContains("hidden"));
            Assert.AreEqual("ひみつのへや", root.Q<Label>("secret-room-title").text);
            Assert.AreEqual(
                "まだ通信しません。コードはこの端末の中だけです。",
                root.Q<Label>("secret-room-note").text);
            Assert.AreEqual("コードをつくる", root.Q<Button>(SecretRoomPanel.GenerateName).text);
            Assert.AreEqual("コピー", root.Q<Button>(SecretRoomPanel.CopyName).text);

            Click(root.Q<Button>(SecretRoomPanel.GenerateName));
            var code = root.Q<TextField>(SecretRoomPanel.FieldName).value;
            Assert.AreEqual(code, JoinCode.Normalize(code));
            Assert.AreEqual("できました。", root.Q<Label>(SecretRoomPanel.StatusName).text);

            GUIUtility.systemCopyBuffer = "keep-me";
            Click(root.Q<Button>(SecretRoomPanel.CopyName));
            Assert.AreEqual(code, GUIUtility.systemCopyBuffer);
            Assert.AreEqual("コピーしました。", root.Q<Label>(SecretRoomPanel.StatusName).text);
            Assert.AreEqual("マッチ開始", start.text);
            Assert.IsTrue(start.enabledSelf);

            root.Q<TextField>(SecretRoomPanel.FieldName).value = "IIIIII";
            GUIUtility.systemCopyBuffer = "keep-me";
            Click(root.Q<Button>(SecretRoomPanel.CopyName));
            Assert.AreEqual("keep-me", GUIUtility.systemCopyBuffer);
            Assert.AreEqual(
                "6文字にしてください。0、1、I、O は使えません。",
                root.Q<Label>(SecretRoomPanel.StatusName).text);

            Click(root.Q<Button>(SecretRoomPanel.CloseName));
            Assert.IsTrue(panel.ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.None, panel.style.display.value);
            Assert.IsFalse(home.ClassListContains("hidden"));
            Assert.AreEqual(DisplayStyle.Flex, home.style.display.value);
        }

        [Test]
        public void SecretRoom_PaintsEnglish_WhenTheLocaleIsEnglish()
        {
            UiLocale.Set(UiLang.En);
            var root = MountMenu();
            Click(root.Q<Button>(SecretRoomPanel.OpenName));

            Assert.AreEqual("Secret room", root.Q<Button>(SecretRoomPanel.OpenName).text);
            Assert.AreEqual("Secret room", root.Q<Label>("secret-room-title").text);
            Assert.AreEqual(
                "No connection yet. This code stays on this device.",
                root.Q<Label>("secret-room-note").text);
            Assert.AreEqual("Join code", root.Q<Label>("secret-room-code-label").text);
            Assert.AreEqual("Make a code", root.Q<Button>(SecretRoomPanel.GenerateName).text);
            Assert.AreEqual("Copy", root.Q<Button>(SecretRoomPanel.CopyName).text);
            Assert.AreEqual("← Back", root.Q<Button>(SecretRoomPanel.CloseName).text);
        }

        VisualElement MountMenu()
        {
            uiHost = new GameObject("secret-room-ui");
            var document = uiHost.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            owned.Add(settings);
            settings.name = "EraserWarsPanelRuntime";
            document.panelSettings = settings;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuPath);
            Assert.IsNotNull(document.rootVisualElement, "The menu tree has to exist before the controller enables.");
            var menu = uiHost.AddComponent<StartMenuController>();
            menu.ShowMenu();

            var root = document.rootVisualElement;
            Assert.IsNotNull(root.Q(SecretRoomPanel.PanelName).panel, "The secret room page needs a panel so a click reaches it.");
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
