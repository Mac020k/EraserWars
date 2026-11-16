using System;
using System.Collections.Generic;

namespace EraserWars.Cards
{
    public enum CardSymbol
    {
        Weight,
        Grip,
        Bounce,
        Power,
        Speed,
        Flick,
        Clock,
        Sight,
        Edge,
        Center,
        Extra,
        Swap,
        Pin,
        Desk,
        Shield,
        Weapon,
        Heart,
        Star,
    }

    public static class CardGlance
    {
        public static CardSymbol Primary(string familyKey, IReadOnlyList<string> effectKinds)
        {
            if (effectKinds != null)
            {
                for (var i = 0; i < effectKinds.Count; i++)
                {
                    if (EffectSpecParser.TryParse(effectKinds[i], out var spec))
                    {
                        return SymbolFor(spec);
                    }
                }
            }

            return FamilyEmblem(familyKey);
        }

        public static CardSymbol SymbolFor(EffectSpec spec)
        {
            switch (spec)
            {
                case ModifyStat stat:
                    return StatSymbol(stat.Stat);
                case ModifyFlick:
                    return CardSymbol.Flick;
                case ModifyRule rule:
                    return RuleSymbol(rule.RuleKey);
                case ExtraFlick:
                    return CardSymbol.Extra;
                case SwapPositions:
                    return CardSymbol.Swap;
                case Teleport:
                    return CardSymbol.Pin;
                case PlaceDeskObject:
                    return CardSymbol.Desk;
                case GrantShield:
                    return CardSymbol.Shield;
                case GrantWeapon:
                    return CardSymbol.Weapon;
                case RevealTrajectory:
                    return CardSymbol.Sight;
                case RestoreStock:
                    return CardSymbol.Heart;
                default:
                    throw new InvalidOperationException($"Unhandled EffectSpec {spec.GetType().Name}");
            }
        }

        public static CardSymbol FamilyEmblem(string familyKey)
        {
            switch (familyKey)
            {
                case CardFamilies.Kiai:
                    return CardSymbol.Power;
                case CardFamilies.Homework:
                    return CardSymbol.Clock;
                case CardFamilies.Seat:
                    return CardSymbol.Swap;
                case CardFamilies.Stationery:
                    return CardSymbol.Weapon;
                case CardFamilies.Cover:
                    return CardSymbol.Shield;
                case CardFamilies.Signature:
                    return CardSymbol.Star;
                default:
                    return CardSymbol.Star;
            }
        }

        public static string LabelJa(CardSymbol symbol)
        {
            switch (symbol)
            {
                case CardSymbol.Weight:
                    return "質量";
                case CardSymbol.Grip:
                    return "摩擦";
                case CardSymbol.Bounce:
                    return "反発";
                case CardSymbol.Power:
                    return "パワー";
                case CardSymbol.Speed:
                    return "スピード";
                case CardSymbol.Flick:
                    return "フリック";
                case CardSymbol.Clock:
                    return "時間";
                case CardSymbol.Sight:
                    return "軌道";
                case CardSymbol.Edge:
                    return "端";
                case CardSymbol.Center:
                    return "中央";
                case CardSymbol.Extra:
                    return "もう一回";
                case CardSymbol.Swap:
                    return "入れ替え";
                case CardSymbol.Pin:
                    return "移動";
                case CardSymbol.Desk:
                    return "机";
                case CardSymbol.Shield:
                    return "シールド";
                case CardSymbol.Weapon:
                    return "武器";
                case CardSymbol.Heart:
                    return "ストック";
                case CardSymbol.Star:
                    return "必殺";
                default:
                    throw new InvalidOperationException($"Unhandled CardSymbol {symbol}");
            }
        }

        static CardSymbol StatSymbol(string stat)
        {
            switch (stat)
            {
                case "Mass":
                    return CardSymbol.Weight;
                case "Friction":
                    return CardSymbol.Grip;
                case "Bounciness":
                    return CardSymbol.Bounce;
                case "Power":
                    return CardSymbol.Power;
                case "Speed":
                    return CardSymbol.Speed;
                default:
                    return CardSymbol.Power;
            }
        }

        static CardSymbol RuleSymbol(string ruleKey)
        {
            switch (ruleKey)
            {
                case "OpponentTurnTime":
                case "SelfTurnTime":
                case "CooldownCut":
                    return CardSymbol.Clock;
                case "HideTrajectory":
                    return CardSymbol.Sight;
                case "PushEdge":
                    return CardSymbol.Edge;
                case "CenterBias":
                    return CardSymbol.Center;
                default:
                    return CardSymbol.Clock;
            }
        }
    }
}
