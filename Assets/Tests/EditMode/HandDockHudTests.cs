using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class HandDockHudTests
    {
        const string HudTreePath = "Assets/UI/MatchHud.uxml";
        const string PanelPath = "Assets/UI/PanelSettings.asset";
        const string SelectedClass = "hand-card-selected";

        [UnityTest]
        public IEnumerator SelectedCardLiftsInsideTheGapAndStaysBelowTheCallout()
        {
            using var rig = new Rig();
            rig.GiveHand();
            rig.StartMatch();
            rig.Hud.RefreshFromState();
            yield return null;

            var chosen = rig.HandCard(0);
            var other = rig.HandCard(1);
            Snap(chosen);
            Snap(other);
            PointAt(chosen);
            yield return null;

            Assert.AreEqual(2, rig.HandCount);
            Assert.IsTrue(chosen.ClassListContains(SelectedClass));
            Assert.IsFalse(other.ClassListContains(SelectedClass));
            Assert.IsTrue(chosen.ClassListContains("hand-card"));
            Assert.IsTrue(chosen.ClassListContains("family-stationery"));
            Assert.AreEqual("ワックス", chosen.Q<Label>(null, "card-face-name").text);
            Assert.IsNotNull(chosen.Q("card-icon"));
            Assert.AreEqual("クリップ", other.Q<Label>(null, "card-face-name").text);

            Assert.AreEqual(8f, chosen.resolvedStyle.marginLeft, 0.01f);
            Assert.AreEqual(8f, other.resolvedStyle.marginLeft, 0.01f);
            Assert.AreEqual(32f, rig.HandRow.resolvedStyle.paddingTop, 0.01f);
            Assert.AreEqual(4f, chosen.resolvedStyle.borderTopWidth, 0.01f);
            Assert.AreEqual(3f, other.resolvedStyle.borderTopWidth, 0.01f);
            Assert.Greater(chosen.resolvedStyle.borderTopColor.r, 0.95f);
            Assert.Greater(chosen.resolvedStyle.borderTopColor.g, 0.95f);
            Assert.Less(other.resolvedStyle.borderTopColor.r, 0.5f);

            var lift = chosen.resolvedStyle.translate.y;
            Assert.AreEqual(-16f, lift, 0.5f);
            Assert.AreEqual(1.1f, chosen.resolvedStyle.scale.value.y, 0.02f);

            var box = chosen.worldBound;
            Assert.Greater(box.height, 140f, $"card height {box.height}");
            var rise = -lift + box.height * (chosen.resolvedStyle.scale.value.y - 1f) * 0.5f;
            var visualTop = box.yMin - rise;
            Assert.Greater(visualTop, rig.Callout.worldBound.yMax, $"card top {visualTop} callout bottom {rig.Callout.worldBound.yMax}");
            Assert.Less(rig.Root.IndexOf(rig.Dock), rig.Root.IndexOf(rig.Callout.parent));
            Assert.Less(rig.Root.IndexOf(rig.Dock), rig.Root.IndexOf(rig.CutIn));

            other.Focus();
            yield return null;
            Assert.IsFalse(chosen.ClassListContains(SelectedClass));
            Assert.IsTrue(other.ClassListContains(SelectedClass));
        }

        [Test]
        public void CutInHidesTheHandAndClearsTheSelection()
        {
            using var rig = new Rig();
            rig.GiveHand();
            rig.StartMatch();
            rig.Hud.RefreshFromState();
            PointAt(rig.HandCard(0));

            Assert.IsTrue(rig.HandCard(0).ClassListContains(SelectedClass));
            Assert.IsFalse(rig.HandHidden);
            Assert.IsFalse(rig.CalloutHidden);

            rig.Director.PlayCard(11);
            rig.Hud.RefreshFromState();

            Assert.IsTrue(rig.HandHidden);
            Assert.IsFalse(rig.CutInHidden);
            Assert.IsTrue(rig.CalloutHidden);
            Assert.IsFalse(rig.AnySelected);
        }

        static void PointAt(VisualElement element)
        {
            using var enter = PointerEnterEvent.GetPooled();
            enter.target = element;
            element.SendEvent(enter);
        }

        static void Snap(VisualElement element)
        {
            element.style.transitionDuration = new List<TimeValue> { new(0f, TimeUnit.Second) };
        }

        sealed class Rig : System.IDisposable
        {
            public MatchDirector Director { get; }
            public MatchHudController Hud { get; }
            readonly GameObject root;
            readonly List<Object> owned = new();

            public Rig()
            {
                root = new GameObject("HandDockRig");
                var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
                surface.name = "PlaySurface";
                surface.transform.SetParent(root.transform, false);

                var template = GameObject.CreatePrimitive(PrimitiveType.Cube);
                template.name = "EraserTemplate";
                template.SetActive(false);
                template.transform.SetParent(root.transform, false);
                template.AddComponent<Rigidbody>();
                template.AddComponent<FalloutDetector>();
                var eraserPrefab = template.AddComponent<EraserBody>();

                var spawns = new Transform[2];
                for (var i = 0; i < spawns.Length; i++)
                {
                    spawns[i] = new GameObject($"Spawn_{i}").transform;
                    spawns[i].SetParent(root.transform, false);
                    spawns[i].position = new Vector3(i * 0.3f, 0.8f, 0f);
                }

                var directorGo = new GameObject("Director");
                directorGo.SetActive(false);
                directorGo.transform.SetParent(root.transform, false);
                var flickApplier = directorGo.AddComponent<FlickApplier>();
                var settleWatcher = directorGo.AddComponent<SettleWatcher>();
                Director = directorGo.AddComponent<MatchDirector>();
                SetField(Director, "flickApplier", flickApplier);
                SetField(Director, "settleWatcher", settleWatcher);
                SetField(Director, "spawnPoints", spawns);
                SetField(Director, "eraserPrefab", eraserPrefab);
                SetField(Director, "showDebug", false);
                directorGo.SetActive(true);
                var physics = directorGo.GetComponent<EffectPhysicsSync>();
                if (physics == null)
                {
                    physics = directorGo.AddComponent<EffectPhysicsSync>();
                }

                SetField(Director, "effectPhysics", physics);

                var source = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudTreePath);
                Assert.NotNull(source, PanelPath);
                Assert.NotNull(tree, HudTreePath);
                var settings = Object.Instantiate(source);
                owned.Add(settings);
                var texture = new RenderTexture(1920, 1080, 0);
                owned.Add(texture);
                settings.targetTexture = texture;
                settings.scaleMode = PanelScaleMode.ConstantPixelSize;
                settings.scale = 1f;

                var menuGo = new GameObject("ClosedMenu");
                menuGo.SetActive(false);
                menuGo.transform.SetParent(root.transform, false);
                var closedMenu = menuGo.AddComponent<StartMenuController>();

                var hudGo = new GameObject("MatchHud");
                hudGo.SetActive(false);
                hudGo.transform.SetParent(root.transform, false);
                var document = hudGo.AddComponent<UIDocument>();
                document.panelSettings = settings;
                document.visualTreeAsset = tree;
                Hud = hudGo.AddComponent<MatchHudController>();
                SetField(Hud, "document", document);
                SetField(Hud, "director", Director);
                SetField(Hud, "startMenu", closedMenu);
                hudGo.SetActive(true);
                Assert.NotNull(Root, "MatchHud did not build a visual tree.");
                var host = document.rootVisualElement;
                host.style.width = 1920;
                host.style.height = 1080;
            }

            public VisualElement Root
            {
                get
                {
                    var document = Hud.GetComponent<UIDocument>();
                    return document.rootVisualElement?.Q("root") ?? document.rootVisualElement;
                }
            }

            public VisualElement Dock => Root.Q("hand-dock");
            public VisualElement HandRow => Root.Q("hand");
            public VisualElement Callout => Root.Q("turn-callout");
            public VisualElement CutIn => Root.Q("card-cutin");
            public int HandCount => HandRow.childCount;
            public bool HandHidden => Dock.ClassListContains("hidden");
            public bool CalloutHidden => Callout.ClassListContains("hidden");
            public bool CutInHidden => CutIn.ClassListContains("hidden");

            public VisualElement HandCard(int index) => HandRow[index];

            public bool AnySelected
            {
                get
                {
                    for (var i = 0; i < HandRow.childCount; i++)
                    {
                        if (HandRow[i].ClassListContains(SelectedClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            public void GiveHand()
            {
                var wax = OwnedCard(11, "ワックス");
                var clip = OwnedCard(12, "クリップ");
                Director.SetSeatRoster(new CharacterDefinition[] { null, null });
                Director.SetSeatCommons(0, new[] { wax, clip });
            }

            public void StartMatch()
            {
                Director.SetStartMenuOpen(false);
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                LogAssert.Expect(LogType.Error, new Regex("Instantiating material"));
                Director.StartHotseat(2);
                Assert.AreEqual(Phase.CardSelect, Director.State.Phase);
            }

            CardDefinition OwnedCard(int id, string name)
            {
                var card = ScriptableObject.CreateInstance<CardDefinition>();
                owned.Add(card);
                card.IdValue = id;
                card.DisplayNameJa = name;
                card.NameKey = name;
                card.FamilyKey = CardFamilies.Stationery;
                card.EffectKinds = new List<string>();
                return card;
            }

            public void Dispose()
            {
                if (Director != null)
                {
                    foreach (var eraser in Director.Erasers)
                    {
                        if (eraser != null)
                        {
                            Object.DestroyImmediate(eraser.gameObject);
                        }
                    }
                }

                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }

                for (var i = 0; i < owned.Count; i++)
                {
                    if (owned[i] != null)
                    {
                        Object.DestroyImmediate(owned[i]);
                    }
                }
            }

            static void SetField(object target, string name, object value)
            {
                var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(field, $"{target.GetType().Name}.{name} was renamed.");
                field.SetValue(target, value);
            }
        }
    }
}
