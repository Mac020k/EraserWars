using System;
using System.Collections.Generic;
using EraserWars.Cards;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
        public enum CardSurface
        {
            Collection,
            Hand,
            CutIn,
            Showcase,
        }

    public static class CardFaceBuilder
    {
        public static void Paint(VisualElement host, CardDefinition card, CardSurface surface)
        {
            if (host == null || card == null)
            {
                return;
            }

            var symbol = CardGlance.Primary(card.FamilyKey, card.EffectKinds);
            var iconKey = CardGlance.IconKey(card.FamilyKey, card.EffectKinds);
            Prepare(host, surface, FamilyClass(card.FamilyKey), card.IsSignature);
            host.tooltip = CardGlance.LabelJa(symbol);

            var pattern = Layer("card-pattern", "card-face-pattern");
            UiImageUtil.ApplyPattern(pattern, CardIconLibrary.Pattern(card.FamilyKey));
            pattern.style.backgroundColor = ArtPaper(card.FamilyKey);

            var icon = Layer("card-icon", "card-effect-icon");
            icon.tooltip = CardGlance.LabelJa(symbol);
            UiImageUtil.ApplyGlyph(icon, CardIconLibrary.Glyph(iconKey, symbol));

            var mat = Layer("card-mat", "card-face-mat");
            var gem = Layer("card-cost", "card-cost-gem");
            var cost = new Label(card.BuildCost.ToString()) { pickingMode = PickingMode.Ignore };
            cost.AddToClassList("card-cost-value");
            gem.Add(cost);

            host.Add(pattern);
            host.Add(icon);
            host.Add(mat);
            host.Add(gem);
            if (card.IsSignature)
            {
                var ribbon = new Label("★ とくぎ") { name = "card-ribbon", pickingMode = PickingMode.Ignore };
                ribbon.AddToClassList("card-ribbon");
                host.Add(ribbon);
            }

            AddPlate(host, card.DisplayName, surface);
        }

        public static void PaintUnknown(VisualElement host, string name, CardSurface surface)
        {
            if (host == null)
            {
                return;
            }

            Prepare(host, surface, "family-unknown", false);
            var pattern = Layer("card-pattern", "card-face-pattern");
            UiImageUtil.ApplyPattern(pattern, CardIconLibrary.Pattern(string.Empty));
            pattern.style.backgroundColor = ArtPaper(string.Empty);
            host.Add(pattern);
            host.Add(Layer("card-mat", "card-face-mat"));
            AddPlate(host, string.IsNullOrEmpty(name) ? "?" : name, surface);
        }

        static void AddPlate(VisualElement host, string title, CardSurface surface)
        {
            var body = Layer("card-body", "card-face-body");
            var name = new Label(title) { pickingMode = PickingMode.Ignore };
            name.AddToClassList("card-face-name");
            name.AddToClassList(NameClass(surface));
            name.style.fontSize = TitleSize(title, surface);
            body.Add(name);
            host.Add(body);
        }

        static int TitleSize(string title, CardSurface surface)
        {
            var length = string.IsNullOrEmpty(title) ? 0 : title.Length;
            if (surface == CardSurface.Showcase)
            {
                return length >= 8 ? 15 : 18;
            }

            var size = length >= 8 ? 11 : length >= 7 ? 12 : 14;
            if (surface == CardSurface.Hand)
            {
                size -= 1;
            }

            return size;
        }

        static Color ArtPaper(string familyKey)
        {
            switch (familyKey)
            {
                case CardFamilies.Kiai:
                    return new Color32(255, 206, 196, 255);
                case CardFamilies.Homework:
                    return new Color32(210, 230, 255, 255);
                case CardFamilies.Seat:
                    return new Color32(196, 238, 204, 255);
                case CardFamilies.Stationery:
                    return new Color32(255, 224, 150, 255);
                case CardFamilies.Cover:
                    return new Color32(255, 196, 214, 255);
                case CardFamilies.Signature:
                    return new Color32(22, 26, 42, 255);
                default:
                    return new Color32(232, 232, 228, 255);
            }
        }

        static void Prepare(VisualElement host, CardSurface surface, string familyClass, bool signature)
        {
            if (host is TextElement text)
            {
                text.text = string.Empty;
            }

            host.Clear();
            host.AddToClassList("card-face");
            switch (surface)
            {
                case CardSurface.Collection:
                    host.EnableInClassList("card-tile", true);
                    host.EnableInClassList("collection-hero-card", false);
                    host.EnableInClassList("hand-card", false);
                    host.EnableInClassList("cutin-card", false);
                    break;
                case CardSurface.Hand:
                    host.EnableInClassList("card-tile", false);
                    host.EnableInClassList("collection-hero-card", false);
                    host.EnableInClassList("hand-card", true);
                    host.EnableInClassList("cutin-card", false);
                    break;
                case CardSurface.CutIn:
                    host.EnableInClassList("card-tile", false);
                    host.EnableInClassList("collection-hero-card", false);
                    host.EnableInClassList("hand-card", false);
                    host.EnableInClassList("cutin-card", true);
                    break;
                case CardSurface.Showcase:
                    host.EnableInClassList("card-tile", false);
                    host.EnableInClassList("collection-hero-card", true);
                    host.EnableInClassList("hand-card", false);
                    host.EnableInClassList("cutin-card", false);
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled CardSurface {surface}");
            }

            var stale = new List<string>();
            foreach (var className in host.GetClasses())
            {
                if (className.StartsWith("family-"))
                {
                    stale.Add(className);
                }
            }

            for (var i = 0; i < stale.Count; i++)
            {
                host.RemoveFromClassList(stale[i]);
            }

            host.AddToClassList(familyClass);
            host.EnableInClassList("signature", signature);
        }

        static string NameClass(CardSurface surface)
        {
            switch (surface)
            {
                case CardSurface.Collection:
                    return "card-tile-name";
                case CardSurface.Hand:
                    return "hand-card-name";
                case CardSurface.CutIn:
                    return "cutin-card-name";
                case CardSurface.Showcase:
                    return "collection-hero-name";
                default:
                    throw new InvalidOperationException($"Unhandled CardSurface {surface}");
            }
        }

        static string FamilyClass(string familyKey)
        {
            switch (familyKey)
            {
                case CardFamilies.Kiai:
                case CardFamilies.Homework:
                case CardFamilies.Seat:
                case CardFamilies.Stationery:
                case CardFamilies.Cover:
                case CardFamilies.Signature:
                    return "family-" + familyKey;
                default:
                    return "family-unknown";
            }
        }

        static VisualElement Layer(string name, string className)
        {
            var element = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            element.AddToClassList(className);
            return element;
        }
    }
}
