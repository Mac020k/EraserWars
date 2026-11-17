using System;
using System.Collections;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EraserWars.Tests.EditMode
{
    public sealed class CardFaceBuilderTests
    {
        const string FaceSheetPath = "Assets/UI/CardFace.uss";

        [Test]
        public void CommonFaceStacksHatchIconStrokeGemAndNamePlate()
        {
            var card = Card("重い消しゴム", CardFamilies.Kiai, 1, false, "ModifyStat:Mass:1.5:1");
            try
            {
                var host = new VisualElement();
                CardFaceBuilder.Paint(host, card, CardSurface.Hand);

                Assert.IsTrue(host.ClassListContains("card-face"));
                Assert.IsTrue(host.ClassListContains("hand-card"));
                Assert.IsTrue(host.ClassListContains("family-kiai"));
                Assert.IsFalse(host.ClassListContains("signature"));
                CollectionAssert.AreEqual(
                    new[] { "card-pattern", "card-icon", "card-mat", "card-cost", "card-body" },
                    Names(host));
                Assert.IsNull(host.Q(className: "card-ribbon"));
                Assert.IsNull(host.Q(className: "card-face-family"));
                Assert.AreEqual("1", host.Q<Label>(className: "card-cost-value").text);
                Assert.AreEqual("重い消しゴム", host.Q<Label>(className: "card-face-name").text);
                Assert.IsTrue(host.Q("card-body").ClassListContains("card-face-body"));
                Assert.AreEqual("質量", host.tooltip);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(card);
            }
        }

        [Test]
        public void SignatureFaceKeepsTheGoldRibbonAndTheTitlePlate()
        {
            var card = Card("もう一押し", CardFamilies.Signature, 4, true, "ExtraFlick");
            try
            {
                var host = new VisualElement();
                CardFaceBuilder.Paint(host, card, CardSurface.Collection);

                Assert.IsTrue(host.ClassListContains("card-tile"));
                Assert.IsTrue(host.ClassListContains("signature"));
                Assert.IsTrue(host.ClassListContains("family-signature"));
                CollectionAssert.AreEqual(
                    new[] { "card-pattern", "card-icon", "card-mat", "card-cost", "card-ribbon", "card-body" },
                    Names(host));
                Assert.AreEqual("★ とくぎ", host.Q<Label>(className: "card-ribbon").text);
                Assert.AreEqual("もう一押し", host.Q<Label>(className: "card-face-name").text);
                Assert.AreEqual("4", host.Q<Label>(className: "card-cost-value").text);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(card);
            }
        }

        [Test]
        public void CutInKeepsTheIconAndThePlateInTheTree()
        {
            var card = Card("ワックス", CardFamilies.Cover, 1, false, "ModifyStat:Friction:0.5:1");
            try
            {
                var host = new VisualElement();
                CardFaceBuilder.Paint(host, card, CardSurface.CutIn);

                Assert.IsTrue(host.ClassListContains("cutin-card"));
                Assert.IsFalse(host.ClassListContains("hand-card"));
                Assert.IsFalse(host.ClassListContains("card-tile"));
                Assert.IsNotNull(host.Q("card-icon"));
                Assert.IsNotNull(host.Q(className: "card-face-body"));
                Assert.AreEqual("ワックス", host.Q<Label>(className: "card-face-name").text);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(card);
            }
        }

        [Test]
        public void UnknownFaceKeepsThePlateWithoutACostGem()
        {
            var host = new VisualElement();
            CardFaceBuilder.PaintUnknown(host, "", CardSurface.Hand);

            Assert.IsTrue(host.ClassListContains("family-unknown"));
            Assert.AreEqual("?", host.Q<Label>(className: "card-face-name").text);
            Assert.IsNotNull(host.Q("card-pattern"));
            Assert.IsNotNull(host.Q("card-mat"));
            Assert.IsNull(host.Q("card-cost"));
            Assert.IsNull(host.Q("card-icon"));
        }

        [Test]
        public void AbilityGlyphsStayDistinct()
        {
            var seen = new HashSet<string>();
            foreach (CardSymbol symbol in Enum.GetValues(typeof(CardSymbol)))
            {
                var markup = CardIconSvg.Markup(symbol);
                Assert.IsTrue(seen.Add(markup), symbol.ToString());
                StringAssert.Contains("fill=\"#24180F\"", markup);
            }
        }

        [UnityTest]
        public IEnumerator KiaiHandFrameIsVividAndThePlateIsWhite()
        {
            var hostGo = new GameObject("card-face-ui");
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            var card = Card("重い消しゴム", CardFamilies.Kiai, 1, false, "ModifyStat:Mass:1.5:1");
            try
            {
                var document = hostGo.AddComponent<UIDocument>();
                document.panelSettings = settings;
                var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(FaceSheetPath);
                Assert.IsNotNull(sheet);
                var root = document.rootVisualElement;
                root.styleSheets.Add(sheet);
                var face = new VisualElement();
                face.style.width = 112;
                face.style.height = 158;
                root.Add(face);
                CardFaceBuilder.Paint(face, card, CardSurface.Hand);
                yield return null;

                var frame = face.resolvedStyle.backgroundColor;
                Assert.Greater(frame.r, 0.75f, $"frame r {frame.r}");
                Assert.Less(frame.g, 0.2f, $"frame g {frame.g}");
                Assert.Less(frame.b, 0.25f, $"frame b {frame.b}");
                Assert.AreEqual(3f, face.resolvedStyle.borderTopWidth, 0.01f);
                Assert.Less(face.resolvedStyle.borderTopColor.r, 0.2f);

                var plate = face.Q(className: "card-face-body").resolvedStyle.backgroundColor;
                Assert.Greater(plate.r, 0.95f);
                Assert.Greater(plate.g, 0.95f);
                Assert.Greater(plate.b, 0.95f);

                var stroke = face.Q("card-mat").resolvedStyle.borderTopColor;
                Assert.Greater(stroke.r, 0.95f);
                Assert.Greater(stroke.g, 0.95f);
                Assert.Greater(stroke.b, 0.95f);

                var icon = face.Q("card-icon").resolvedStyle;
                Assert.Greater(icon.width, 60f);
                Assert.Greater(icon.height, 60f);
                Assert.Less(icon.backgroundColor.a, 0.05f);

                var gem = face.Q("card-cost").resolvedStyle;
                Assert.Less(gem.backgroundColor.r, 0.15f);
                Assert.Greater(gem.backgroundColor.a, 0.9f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(card);
                UnityEngine.Object.DestroyImmediate(hostGo);
                UnityEngine.Object.DestroyImmediate(settings);
            }
        }

        static CardDefinition Card(string name, string family, int cost, bool signature, string effect)
        {
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            card.DisplayNameJa = name;
            card.FamilyKey = family;
            card.BuildCost = cost;
            card.IsSignature = signature;
            card.EffectKinds = new List<string> { effect };
            return card;
        }

        static List<string> Names(VisualElement host)
        {
            var names = new List<string>();
            foreach (var child in host.Children())
            {
                names.Add(child.name);
            }

            return names;
        }
    }
}
