using EraserWars.Audio;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UiSettings = EraserWars.UI.GameSettings;

namespace EraserWars.Tests.EditMode
{
    public sealed class HotseatRulesTests
    {
        [Test]
        public void StartMenu_ShowsStockAndPoints_BesidePlayerCount()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/StartMenu.uxml");
            Assert.NotNull(tree);
            var root = tree.CloneTree();
            Assert.AreEqual("2人対戦", root.Q<Button>("mode-2p").text);
            Assert.AreEqual("4人対戦", root.Q<Button>("mode-4p").text);
            Assert.AreEqual("ストック戦", root.Q<Button>("mode-stock").text);
            Assert.AreEqual("ポイント戦", root.Q<Button>("mode-points").text);
        }

        [Test]
        public void PointsPref_StartsEightRoundPoints_ForTwoAndFourPlayers()
        {
            var rules = MatchDirector.CreateHotseatRules(winModePref: (int)WinMode.Points, turnSecondsPref: 35);
            Assert.AreEqual(WinMode.Points, rules.WinMode);
            Assert.AreEqual(8, rules.PointRounds);
            Assert.AreEqual(35f, rules.TurnTimeSeconds);
            Assert.AreEqual(3, rules.StockCount);

            var two = MatchFactory.CreateHotseat(2, rules);
            var four = MatchFactory.CreateHotseat(4, rules);
            Assert.AreEqual(2, two.Players.Length);
            Assert.AreEqual(4, four.Players.Length);
            Assert.AreEqual(WinMode.Points, two.Rules.WinMode);
            Assert.AreEqual(8, two.Rules.PointRounds);
            Assert.AreEqual(WinMode.Points, four.Rules.WinMode);
            Assert.AreEqual(8, four.Rules.PointRounds);
            Assert.AreEqual(0, two.Players[0].Points);
            Assert.AreEqual(0, four.Players[3].Points);
        }

        [Test]
        public void StockPref_AndUnknownPref_StayCasualStock()
        {
            var stock = MatchDirector.CreateHotseatRules(0, 20);
            Assert.AreEqual(WinMode.Stock, stock.WinMode);
            Assert.AreEqual(3, stock.StockCount);
            Assert.AreEqual(20f, stock.TurnTimeSeconds);

            var unknown = MatchDirector.CreateHotseatRules(99, 4);
            Assert.AreEqual(WinMode.Stock, unknown.WinMode);
            Assert.AreEqual(3, unknown.StockCount);
            Assert.AreEqual(10f, unknown.TurnTimeSeconds);
        }

        [Test]
        public void MenuSave_IsWhatHotseatReads()
        {
            Assert.AreEqual(UiSettings.Keys.WinMode, MatchDirector.HotseatWinModeKey);
            Assert.AreEqual(UiSettings.Keys.TurnSeconds, MatchDirector.HotseatTurnSecondsKey);

            var prior = Snapshot();
            try
            {
                UiSettings.Load();
                UiSettings.Save(UiSettings.Current with
                {
                    WinMode = WinMode.Points,
                    TurnSeconds = 40,
                });

                var points = ReadSavedRules();
                Assert.AreEqual(WinMode.Points, points.WinMode);
                Assert.AreEqual(8, points.PointRounds);
                Assert.AreEqual(40f, points.TurnTimeSeconds);

                UiSettings.ResetToDefaults();
                var stock = ReadSavedRules();
                Assert.AreEqual(WinMode.Stock, stock.WinMode);
                Assert.AreEqual(3, stock.StockCount);
                Assert.AreEqual(UiSettings.DefaultTurnSeconds, stock.TurnTimeSeconds);
            }
            finally
            {
                prior.Restore();
                UiSettings.Load();
            }
        }

        static RuleSet ReadSavedRules() =>
            MatchDirector.CreateHotseatRules(
                PlayerPrefs.GetInt(MatchDirector.HotseatWinModeKey),
                PlayerPrefs.GetInt(MatchDirector.HotseatTurnSecondsKey));

        static PrefSnapshot Snapshot() => new(
            Pref.Int(UiSettings.Keys.WinMode),
            Pref.Int(UiSettings.Keys.TurnSeconds),
            Pref.Int(UiSettings.Keys.CameraShake),
            Pref.Float(UiSettings.Keys.Master),
            Pref.Float(UiSettings.Keys.Bgm),
            Pref.Float(UiSettings.Keys.Se),
            Pref.Float(UiSettings.Keys.FlickScale),
            Pref.Float(UiSettings.Keys.UiScale),
            Pref.Float(GameSettings.BgmPercentKey),
            Pref.Float(GameSettings.SePercentKey));

        readonly struct PrefSnapshot
        {
            readonly Pref[] prefs;

            public PrefSnapshot(
                Pref winMode,
                Pref turnSeconds,
                Pref cameraShake,
                Pref master,
                Pref bgm,
                Pref se,
                Pref flickScale,
                Pref uiScale,
                Pref bgmPercent,
                Pref sePercent)
            {
                prefs = new[]
                {
                    winMode,
                    turnSeconds,
                    cameraShake,
                    master,
                    bgm,
                    se,
                    flickScale,
                    uiScale,
                    bgmPercent,
                    sePercent,
                };
            }

            public void Restore()
            {
                for (var i = 0; i < prefs.Length; i++)
                {
                    prefs[i].Restore();
                }

                PlayerPrefs.Save();
            }
        }

        readonly struct Pref
        {
            readonly string key;
            readonly bool had;
            readonly bool integer;
            readonly int intValue;
            readonly float floatValue;

            Pref(string key, bool had, bool integer, int intValue, float floatValue)
            {
                this.key = key;
                this.had = had;
                this.integer = integer;
                this.intValue = intValue;
                this.floatValue = floatValue;
            }

            public static Pref Int(string key) =>
                new(key, PlayerPrefs.HasKey(key), true, PlayerPrefs.GetInt(key), 0f);

            public static Pref Float(string key) =>
                new(key, PlayerPrefs.HasKey(key), false, 0, PlayerPrefs.GetFloat(key));

            public void Restore()
            {
                if (!had)
                {
                    PlayerPrefs.DeleteKey(key);
                    return;
                }

                if (integer)
                {
                    PlayerPrefs.SetInt(key, intValue);
                    return;
                }

                PlayerPrefs.SetFloat(key, floatValue);
            }
        }
    }
}
