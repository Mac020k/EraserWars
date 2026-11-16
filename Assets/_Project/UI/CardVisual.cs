using System;
using System.Collections.Generic;
using EraserWars.Cards;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public enum CardSurface
    {
        Collection,
        Hand,
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
            Prepare(host, surface, FamilyClass(card.FamilyKey), card.IsSignature);
            host.tooltip = CardGlance.LabelJa(symbol);

            var pattern = Layer("card-pattern", "card-face-pattern");
            UiImageUtil.ApplyPattern(pattern, CardIconLibrary.Pattern(card.FamilyKey));
            var mat = Layer("card-mat", "card-face-mat");
            var gem = Layer("card-cost", "card-cost-gem");
            var cost = new Label(card.BuildCost.ToString()) { pickingMode = PickingMode.Ignore };
            cost.AddToClassList("card-cost-value");
            gem.Add(cost);

            var icon = Layer("card-icon", "card-effect-icon");
            icon.tooltip = CardGlance.LabelJa(symbol);
            UiImageUtil.ApplyGlyph(icon, CardIconLibrary.Icon(symbol));

            host.Add(pattern);
            host.Add(mat);
            host.Add(gem);
            host.Add(icon);
            if (card.IsSignature)
            {
                var ribbon = new Label("★ とくぎ") { pickingMode = PickingMode.Ignore };
                ribbon.AddToClassList("card-ribbon");
                host.Add(ribbon);
            }

            var body = Layer("card-body", "card-face-body");
            var family = new Label(CardFamilies.DisplayJa(card.FamilyKey)) { pickingMode = PickingMode.Ignore };
            family.AddToClassList("card-face-family");
            var name = new Label(card.DisplayName) { pickingMode = PickingMode.Ignore };
            name.AddToClassList("card-face-name");
            name.AddToClassList(NameClass(surface));

            body.Add(family);
            body.Add(name);
            host.Add(body);
        }

        public static void PaintUnknown(VisualElement host, string name, CardSurface surface)
        {
            if (host == null)
            {
                return;
            }

            Prepare(host, surface, "family-unknown", false);
            var label = new Label(string.IsNullOrEmpty(name) ? "?" : name) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("card-face-name");
            host.Add(label);
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
                    host.EnableInClassList("hand-card", false);
                    break;
                case CardSurface.Hand:
                    host.EnableInClassList("card-tile", false);
                    host.EnableInClassList("hand-card", true);
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
