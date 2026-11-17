using System;
using System.Collections.Generic;
using EraserWars.Core;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class LocalizationTests
    {
        const string MenuPath = "Assets/UI/StartMenu.uxml";
        const string HudPath = "Assets/UI/MatchHud.uxml";

        readonly List<UnityEngine.Object> owned = new();
        GameObject uiHost;
        bool hadPref;
        string prefValue;
        float volume;

        [SetUp]
        public void SetUp()
        {
            hadPref = PlayerPrefs.HasKey(UiLocale.PrefKey);
            prefValue = PlayerPrefs.GetString(UiLocale.PrefKey, UiLocale.Japanese);
            volume = AudioListener.volume;
            PlayerPrefs.DeleteKey(UiLocale.PrefKey);
            UiLocale.Load();
        }

        [TearDown]
        public void TearDown()
        {
            if (uiHost != null)
            {
                UnityEngine.Object.DestroyImmediate(uiHost);
                uiHost = null;
            }

            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();

            if (hadPref)
            {
                PlayerPrefs.SetString(UiLocale.PrefKey, prefValue);
            }
            else
            {
                PlayerPrefs.DeleteKey(UiLocale.PrefKey);
            }

            PlayerPrefs.Save();
            UiLocale.Load();
            AudioListener.volume = volume;
            GameSettings.Load();
        }

        [Test]
        public void EveryKeyHasJapaneseAndEnglish()
        {
            var keys = (UiKey[])Enum.GetValues(typeof(UiKey));
            UiLocale.Set(UiLang.Ja);
            foreach (var key in keys)
            {
                var ja = UiText.Get(key);
                Assert.IsFalse(string.IsNullOrEmpty(ja), key.ToString());
                Assert.AreNotEqual(key.ToString(), ja, key.ToString());
            }

            UiLocale.Set(UiLang.En);
            foreach (var key in keys)
            {
                var en = UiText.Get(key);
                Assert.IsFalse(string.IsNullOrEmpty(en), key.ToString());
                Assert.AreNotEqual(key.ToString(), en, key.ToString());
            }
        }

        [Test]
        public void MissingPrefStaysJapanese()
        {
            Assert.IsFalse(PlayerPrefs.HasKey(UiLocale.PrefKey));
            UiLocale.Load();
            Assert.AreEqual(UiLang.Ja, UiLocale.Current);
            Assert.AreEqual("マッチ開始", UiText.Get(UiKey.MenuStart));
            Assert.AreEqual("P1 のばん", UiText.Format(UiKey.HudSeatTurn, 1));
            Assert.AreEqual("このきを P1 にわたして", UiText.Format(UiKey.HudPassHint, 1));
            Assert.AreEqual("けいけん +10  ·  ごうけい 10", UiText.Format(UiKey.HudXp, 10, 10));
            Assert.AreEqual("P2 のかち!", UiText.Format(UiKey.HudWin, 2));
            Assert.AreEqual("ひきわけ!", UiText.Get(UiKey.HudDraw));
            Assert.AreEqual("P1 ストック 0", UiText.Format(UiKey.HudStockLine, 1, 0));
            Assert.AreEqual("P1 3点", UiText.Format(UiKey.HudPointsLine, 1, 3));
            Assert.AreEqual("P1  とくぎ", UiText.Format(UiKey.HudCutInSpecial, 1));
            Assert.AreEqual("P1  カード", UiText.Format(UiKey.HudCutInCard, 1));
            Assert.AreEqual("パスした", UiText.Get(UiKey.HudYieldPass));
            Assert.AreEqual("じかんぎれ", UiText.Get(UiKey.HudYieldTime));
            Assert.AreEqual("再戦", UiText.Get(UiKey.HudRematch));
            Assert.AreEqual("メニューへ", UiText.Get(UiKey.HudToMenu));
        }

        [Test]
        public void EnglishPrefReturnsEnglishCopy()
        {
            UiLocale.Set(UiLang.En);
            UiLocale.Load();

            Assert.AreEqual(UiLocale.English, PlayerPrefs.GetString(UiLocale.PrefKey));
            Assert.AreEqual(UiLang.En, UiLocale.Current);
            Assert.AreEqual("Start match", UiText.Get(UiKey.MenuStart));
            Assert.AreEqual("New match", UiText.Get(UiKey.MenuNewMatch));
            Assert.AreEqual("P1's turn", UiText.Format(UiKey.HudSeatTurn, 1));
            Assert.AreEqual("Hand the device to P4", UiText.Format(UiKey.HudPassHint, 4));
            Assert.AreEqual("XP +10  ·  total 20", UiText.Format(UiKey.HudXp, 10, 20));
            Assert.AreEqual("P1 wins!", UiText.Format(UiKey.HudWin, 1));
            Assert.AreEqual("Draw!", UiText.Get(UiKey.HudDraw));
            Assert.AreEqual("Master 100", UiText.Format(UiKey.OptMaster, 100));
            Assert.AreEqual("Camera shake: OFF", UiText.Get(UiKey.OptShakeOff));
        }

        [Test]
        public void UnknownPrefFallsBackToJapanese()
        {
            PlayerPrefs.SetString(UiLocale.PrefKey, "fr");
            UiLocale.Load();
            Assert.AreEqual(UiLang.Ja, UiLocale.Current);
            Assert.AreEqual("わかった", UiText.Get(UiKey.MenuTipDismiss));
        }

        [Test]
        public void TurnChrome_UsesTheActiveLanguage()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock) with
            {
                Phase = Phase.CardSelect,
                TurnTimeLeft = 12.2f,
            };

            UiLocale.Set(UiLang.Ja);
            var ja = TurnChrome.From(state);
            Assert.AreEqual("P1 のばん", ja.Headline);
            Assert.AreEqual("このきを P1 にわたして", ja.PassHint);

            UiLocale.Set(UiLang.En);
            var en = TurnChrome.From(state);
            Assert.AreEqual("P1's turn", en.Headline);
            Assert.AreEqual("Hand the device to P1", en.PassHint);

            var ended = TurnChrome.From(state with { Phase = Phase.MatchEnd });
            Assert.AreEqual("End", ended.Headline);
            Assert.AreEqual(string.Empty, ended.PassHint);
        }

        [Test]
        public void StartMenuAssetStaysJapaneseUntilApply()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuPath);
            Assert.NotNull(tree);
            var root = tree.CloneTree();
            Assert.AreEqual("2人対戦", root.Q<Button>("mode-2p").text);
            Assert.AreEqual("マッチ開始", root.Q<Button>("start-match").text);

            UiLocale.Set(UiLang.En);
            UiText.Apply(root, UiText.StartMenuSlots);

            Assert.AreEqual("2 players", root.Q<Button>("mode-2p").text);
            Assert.AreEqual("4 players", root.Q<Button>("mode-4p").text);
            Assert.AreEqual("OK", root.Q<Button>("home-tip-dismiss").text);
            Assert.AreEqual("Deck", root.Q<Label>("nav-deck").text);
            Assert.AreEqual("Options", root.Q<Label>("options-title").text);
            Assert.AreEqual("Save", root.Q<Button>("deck-save").text);
            Assert.AreEqual("Cards", root.Q<Button>("collection-tab-cards").text);
        }

        [Test]
        public void StartMenuController_PaintsTheActiveLanguage()
        {
            UiLocale.Set(UiLang.En);
            var root = Mount(MenuPath, host => host.AddComponent<StartMenuController>());
            var menu = uiHost.GetComponent<StartMenuController>();
            menu.ShowMenu();

            Assert.AreEqual("Start match", root.Q<Button>("start-match").text);
            Assert.AreEqual("2 players", root.Q<Button>("mode-2p").text);
            Assert.AreEqual("Play one card, then drag to flick.", root.Q<Label>("home-tip-copy").text);
            Assert.AreEqual("Reset", root.Q<Button>("opt-reset").text);
            Assert.AreEqual("Debug mode: ON", root.Q<Button>("opt-debug-mode").text);
        }

        [Test]
        public void MatchHudController_PaintsStaticChrome()
        {
            UiLocale.Set(UiLang.En);
            var root = Mount(HudPath, host => host.AddComponent<MatchHudController>());
            uiHost.GetComponent<MatchHudController>().RefreshFromState();

            Assert.AreEqual("Menu", root.Q<Button>("menu").text);
            Assert.AreEqual("Rematch", root.Q<Button>("rematch").text);
            Assert.AreEqual("To menu", root.Q<Button>("match-end-menu").text);
            Assert.AreEqual("Pass", root.Q<Label>("pass-label").text);
            Assert.AreEqual("s", root.Q<Label>("timer-unit").text);
            Assert.AreEqual("Pause", root.Q<Label>("pause-title").text);
            Assert.AreEqual("Resume", root.Q<Button>("pause-continue").text);
            Assert.AreEqual("Main menu", root.Q<Button>("pause-main").text);
            Assert.AreEqual("Decided", root.Q<Label>("result-kicker").text);
        }

        VisualElement Mount(string treePath, Action<GameObject> addController)
        {
            uiHost = new GameObject("locale-ui");
            var document = uiHost.AddComponent<UIDocument>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            owned.Add(settings);
            settings.name = "EraserWarsPanelRuntime";
            document.panelSettings = settings;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);
            Assert.IsNotNull(document.rootVisualElement);
            addController(uiHost);
            var root = document.rootVisualElement;
            Assert.IsNotNull(root);
            return root;
        }
    }
}
