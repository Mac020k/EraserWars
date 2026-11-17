using System.Collections;
using System.Collections.Generic;
using EraserWars.Gameplay;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class OptionsAccessibilityTests
    {
        const string MenuPath = "Assets/UI/StartMenu.uxml";
        const string HudPath = "Assets/UI/MatchHud.uxml";

        readonly List<Object> owned = new();
        GameObject uiHost;

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
        }

        [Test]
        public void SaveRoundTripsReduceMotionAndTheSeatPalette()
        {
            using var prior = new PrefGuard();
            PlayerPrefs.DeleteKey(GameSettings.Keys.ReduceMotion);
            PlayerPrefs.DeleteKey(GameSettings.Keys.ColorblindSeats);

            var loaded = GameSettings.Load();
            Assert.IsFalse(loaded.ReduceMotion);
            Assert.IsFalse(loaded.ColorblindSeats);
            Assert.IsFalse(EdgeSlowmoCue.ReduceMotionPreferred());
            Assert.IsFalse(SeatPalette.ColorblindPreferred());
            Assert.AreEqual("red", SeatPalette.DebugName(0));
            AssertRedGreenPair(colorblind: false);

            GameSettings.Save(loaded with { ReduceMotion = true, ColorblindSeats = true });

            Assert.AreEqual(1, PlayerPrefs.GetInt(EdgeSlowmoCue.ReduceMotionPref));
            Assert.AreEqual(EdgeSlowmoCue.ReduceMotionPref, GameSettings.Keys.ReduceMotion);
            Assert.AreEqual(1, PlayerPrefs.GetInt(SeatPalette.PrefKey));
            Assert.IsTrue(EdgeSlowmoCue.ReduceMotionPreferred());
            Assert.IsTrue(SeatPalette.ColorblindPreferred());

            var again = GameSettings.Load();
            Assert.IsTrue(again.ReduceMotion);
            Assert.IsTrue(again.ColorblindSeats);
            Assert.AreEqual("blue", SeatPalette.DebugName(0));
            Assert.AreEqual("magenta", SeatPalette.DebugName(1));
            Assert.AreEqual("orange", SeatPalette.DebugName(2));
            Assert.AreEqual("gold", SeatPalette.DebugName(3));
            var accessible = SeatPalette.Body(0);
            Assert.AreEqual(SeatPalette.Ui(0, true), accessible);
            Assert.AreEqual(SeatPalette.Praise(0), accessible);
            Assert.Greater(accessible.b, accessible.r);
            AssertRedGreenPair(colorblind: true);
            AssertSeatsStayApart();

            var layer = new VisualElement();
            var praise = new KnockPraiseView(layer, _ => null);
            praise.Play(0, 1);
            var mark = layer.Q<Label>("praise-mark");
            Assert.AreEqual(accessible, mark.style.unityTextOutlineColor.value);

            GameSettings.ResetToDefaults();
            Assert.IsFalse(GameSettings.Load().ReduceMotion);
            Assert.IsFalse(GameSettings.Current.ColorblindSeats);
            Assert.IsFalse(EdgeSlowmoCue.ReduceMotionPreferred());
            Assert.Greater(SeatPalette.Body(0).r, SeatPalette.Body(0).b);
            Assert.AreEqual("red", SeatPalette.DebugName(0));
        }

        [Test]
        public void UiScaleSnapsToLargeAndTheMenuOffersOnlyThatSize()
        {
            using var prior = new PrefGuard();
            PlayerPrefs.SetFloat(GameSettings.Keys.UiScale, 0.8f);

            var loaded = GameSettings.Load();
            Assert.AreEqual(1.2f, loaded.UiScale, 0.001f);
            Assert.AreEqual(1.2f, GameSettings.SnapUiScale(0.8f), 0.001f);
            Assert.AreEqual(1.2f, GameSettings.SnapUiScale(1f), 0.001f);
            Assert.AreEqual(GameSettings.LargeUiScale, GameSettings.DefaultUiScale);

            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/StartMenu.uxml");
            var root = tree.CloneTree();
            Assert.IsNull(root.Q("opt-ui-80"));
            Assert.IsNull(root.Q("opt-ui-100"));
            Assert.IsNull(root.Q<Button>("opt-ui-120"));
            Assert.AreEqual("大", root.Q<Label>("opt-ui-120").text);
        }

        [Test]
        public void OptionsTogglesWriteThePrefsAndRepaintTheSwatches()
        {
            using var prior = new PrefGuard();
            PlayerPrefs.DeleteKey(GameSettings.Keys.ReduceMotion);
            PlayerPrefs.DeleteKey(GameSettings.Keys.ColorblindSeats);
            UiLocale.Set(UiLang.Ja);

            var root = Mount(MenuPath, host => host.AddComponent<StartMenuController>());
            var menu = uiHost.GetComponent<StartMenuController>();
            menu.OpenOptions();

            var reduce = root.Q<Button>("opt-reduce-motion");
            var palette = root.Q<Button>("opt-seat-palette");
            Assert.AreEqual("見やすさ", root.Q<Label>("opt-section-access").text);
            Assert.AreEqual("動きを減らす: OFF", reduce.text);
            Assert.AreEqual("座席の色: 標準", palette.text);
            Assert.IsFalse(reduce.ClassListContains("on"));
            Assert.IsFalse(palette.ClassListContains("on"));
            var standardSwatch = Swatch(root, 0);
            Assert.Greater(standardSwatch.r, standardSwatch.b);

            Click(reduce);
            Click(palette);

            Assert.AreEqual(1, PlayerPrefs.GetInt(EdgeSlowmoCue.ReduceMotionPref));
            Assert.AreEqual(1, PlayerPrefs.GetInt(SeatPalette.PrefKey));
            Assert.IsTrue(EdgeSlowmoCue.ReduceMotionPreferred());
            Assert.IsTrue(GameSettings.Current.ReduceMotion);
            Assert.IsTrue(GameSettings.Current.ColorblindSeats);
            Assert.AreEqual("動きを減らす: ON", reduce.text);
            Assert.AreEqual("座席の色: 色覚サポート", palette.text);
            Assert.IsTrue(reduce.ClassListContains("on"));
            Assert.IsTrue(palette.ClassListContains("on"));
            var accessibleSwatch = Swatch(root, 0);
            Assert.Greater(accessibleSwatch.b, accessibleSwatch.r);
            Assert.AreEqual(SeatPalette.Ui(0, true).r, accessibleSwatch.r, 0.001f);
            Assert.AreEqual(SeatPalette.Ui(2, true).r, Swatch(root, 2).r, 0.001f);
            Assert.Greater(Swatch(root, 2).r, Swatch(root, 2).g);

            Click(reduce);
            Click(palette);

            Assert.IsFalse(EdgeSlowmoCue.ReduceMotionPreferred());
            Assert.IsFalse(SeatPalette.ColorblindPreferred());
            Assert.AreEqual("動きを減らす: OFF", reduce.text);
            Assert.AreEqual("座席の色: 標準", palette.text);
            Assert.Greater(Swatch(root, 0).r, Swatch(root, 0).b);
        }

        [UnityTest]
        public IEnumerator HudSeatBorderSwapsToTheAccessibleBlue()
        {
            using var prior = new PrefGuard();
            PlayerPrefs.DeleteKey(SeatPalette.PrefKey);
            var root = Mount(HudPath, host => host.AddComponent<MatchHudController>());
            var hud = uiHost.GetComponent<MatchHudController>();
            hud.RefreshFromState();

            var hudRoot = root.Q("root") ?? root;
            var callout = hudRoot.Q("turn-callout");
            callout.AddToClassList("seat-0");
            Show(hudRoot);
            Show(callout);
            yield return null;

            var standard = callout.resolvedStyle.borderTopColor;
            Assert.IsFalse(hudRoot.ClassListContains(SeatPalette.HudClass));
            Assert.Greater(standard.r, 0.9f, $"standard border {standard}");
            Assert.Greater(standard.r, standard.b, $"standard border {standard}");

            PlayerPrefs.SetInt(SeatPalette.PrefKey, 1);
            hud.RefreshFromState();
            Show(hudRoot);
            Show(callout);
            yield return null;

            Assert.IsTrue(hudRoot.ClassListContains(SeatPalette.HudClass));
            var swapped = callout.resolvedStyle.borderTopColor;
            var expected = SeatPalette.Ui(0, true);
            Assert.AreEqual(expected.r, swapped.r, 0.02f, $"swapped border {swapped}");
            Assert.AreEqual(expected.g, swapped.g, 0.02f, $"swapped border {swapped}");
            Assert.AreEqual(expected.b, swapped.b, 0.02f, $"swapped border {swapped}");
            Assert.Greater(swapped.b, swapped.r);
        }

        static void AssertRedGreenPair(bool colorblind)
        {
            var first = SeatPalette.Body(0);
            var third = SeatPalette.Body(2);
            if (colorblind)
            {
                Assert.Greater(first.b, first.r);
                Assert.Greater(third.r, third.g);
                return;
            }

            Assert.Greater(first.r, first.b);
            Assert.Greater(third.g, third.r);
        }

        static void AssertSeatsStayApart()
        {
            for (var i = 0; i < SeatPalette.Count; i++)
            {
                for (var j = i + 1; j < SeatPalette.Count; j++)
                {
                    var distance = Distance(SeatPalette.Ui(i, true), SeatPalette.Ui(j, true));
                    Assert.Greater(distance, 70f, $"seats {i} and {j} distance {distance}");
                }
            }
        }

        static float Distance(Color a, Color b)
        {
            var dr = (a.r - b.r) * 255f;
            var dg = (a.g - b.g) * 255f;
            var db = (a.b - b.b) * 255f;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db);
        }

        static Color Swatch(VisualElement root, int seat)
        {
            var swatch = root.Q($"opt-swatch-{seat}");
            Assert.NotNull(swatch, $"opt-swatch-{seat}");
            return swatch.style.backgroundColor.value;
        }

        static void Show(VisualElement element)
        {
            element.RemoveFromClassList("hidden");
            element.style.display = DisplayStyle.Flex;
        }

        static void Click(VisualElement element)
        {
            using var click = ClickEvent.GetPooled();
            click.target = element;
            element.SendEvent(click);
        }

        VisualElement Mount(string treePath, System.Action<GameObject> addController)
        {
            uiHost = new GameObject("access-ui");
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
            Assert.IsNotNull(root.panel);
            return root;
        }

        sealed class PrefGuard : System.IDisposable
        {
            readonly Snap[] snaps;
            readonly bool hadLocale;
            readonly string locale;
            readonly float volume;

            public PrefGuard()
            {
                snaps = new[]
                {
                    Snap.Int(GameSettings.Keys.WinMode),
                    Snap.Int(GameSettings.Keys.TurnSeconds),
                    Snap.Int(GameSettings.Keys.CameraShake),
                    Snap.Int(GameSettings.Keys.ReduceMotion),
                    Snap.Int(GameSettings.Keys.ColorblindSeats),
                    Snap.Float(GameSettings.Keys.Master),
                    Snap.Float(GameSettings.Keys.Bgm),
                    Snap.Float(GameSettings.Keys.Se),
                    Snap.Float(GameSettings.Keys.FlickScale),
                    Snap.Float(GameSettings.Keys.UiScale),
                    Snap.Float(EraserWars.Audio.GameSettings.BgmPercentKey),
                    Snap.Float(EraserWars.Audio.GameSettings.SePercentKey),
                };
                hadLocale = PlayerPrefs.HasKey(UiLocale.PrefKey);
                locale = PlayerPrefs.GetString(UiLocale.PrefKey, UiLocale.Japanese);
                volume = AudioListener.volume;
            }

            public void Dispose()
            {
                for (var i = 0; i < snaps.Length; i++)
                {
                    snaps[i].Restore();
                }

                if (hadLocale)
                {
                    PlayerPrefs.SetString(UiLocale.PrefKey, locale);
                }
                else
                {
                    PlayerPrefs.DeleteKey(UiLocale.PrefKey);
                }

                PlayerPrefs.Save();
                UiLocale.Load();
                GameSettings.Load();
                AudioListener.volume = volume;
            }

            readonly struct Snap
            {
                readonly string key;
                readonly bool had;
                readonly bool integer;
                readonly int intValue;
                readonly float floatValue;

                Snap(string key, bool had, bool integer, int intValue, float floatValue)
                {
                    this.key = key;
                    this.had = had;
                    this.integer = integer;
                    this.intValue = intValue;
                    this.floatValue = floatValue;
                }

                public static Snap Int(string key) =>
                    new(key, PlayerPrefs.HasKey(key), true, PlayerPrefs.GetInt(key), 0f);

                public static Snap Float(string key) =>
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
}
