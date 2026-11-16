using System;
using System.Collections.Generic;
using System.Globalization;

namespace EraserWars.Cards
{
    public static class EffectSpecParser
    {
        public static bool TryParse(string raw, out EffectSpec spec)
        {
            spec = null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            var parts = raw.Split(':');
            var head = parts[0].Trim();
            switch (head)
            {
                case "ModifyStat" when parts.Length >= 4:
                    spec = new ModifyStat(parts[1], ParseFloat(parts[2]), ParseInt(parts[3]));
                    return true;
                case "ModifyFlick" when parts.Length >= 4:
                    spec = new ModifyFlick(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3]));
                    return true;
                case "ModifyRule" when parts.Length >= 4:
                    spec = new ModifyRule(parts[1], ParseFloat(parts[2]), ParseInt(parts[3]));
                    return true;
                case "ExtraFlick":
                    spec = new ExtraFlick();
                    return true;
                case "SwapPositions":
                    spec = new SwapPositions();
                    return true;
                case "Teleport" when parts.Length >= 3:
                    spec = new Teleport(ParseFloat(parts[1]), ParseFloat(parts[2]));
                    return true;
                case "PlaceDeskObject" when parts.Length >= 3:
                    spec = new PlaceDeskObject(parts[1], ParseInt(parts[2]));
                    return true;
                case "GrantShield" when parts.Length >= 3:
                    spec = new GrantShield(ParseFloat(parts[1]), ParseInt(parts[2]));
                    return true;
                case "GrantWeapon" when parts.Length >= 3:
                    spec = new GrantWeapon(ParseFloat(parts[1]), ParseInt(parts[2]));
                    return true;
                case "RevealTrajectory" when parts.Length >= 2:
                    spec = new RevealTrajectory(ParseInt(parts[1]));
                    return true;
                case "RestoreStock" when parts.Length >= 2:
                    spec = new RestoreStock(ParseInt(parts[1]));
                    return true;
                default:
                    return false;
            }
        }

        public static IReadOnlyList<EffectSpec> ParseAll(IEnumerable<string> kinds)
        {
            var list = new List<EffectSpec>();
            if (kinds == null)
            {
                return list;
            }

            foreach (var kind in kinds)
            {
                if (TryParse(kind, out var spec))
                {
                    list.Add(spec);
                }
            }

            return list;
        }

        public static string SummarizeJa(string raw)
        {
            if (!TryParse(raw, out var spec))
            {
                return raw ?? string.Empty;
            }

            return spec switch
            {
                ModifyStat s => $"{StatJa(s.Stat)}×{s.Multiplier} ({s.Turns}T)",
                ModifyFlick f => $"フリック 力×{f.PowerMultiplier} / ブレ×{f.SwayAmplitudeMultiplier}",
                ModifyRule r => $"ルール {RuleJa(r.RuleKey)}={r.Value} ({r.Turns}T)",
                ExtraFlick => "追加フリック",
                SwapPositions => "位置入れ替え",
                Teleport t => $"テレポート ({t.DeskNormalizedX:0.##},{t.DeskNormalizedZ:0.##})",
                PlaceDeskObject o => $"机オブジェクト {o.PrefabKey} ({o.Turns}T)",
                GrantShield g => $"シールド 吸収{g.ImpulseAbsorb} / {g.Hits}回",
                GrantWeapon w => $"武器 追加力積{w.BonusImpulseOnHit} / {w.Hits}回",
                RevealTrajectory r => $"軌道予測 深さ{r.CollisionDepth}",
                RestoreStock s => $"ストック回復 +{s.Amount}",
                _ => throw new InvalidOperationException($"Unhandled EffectSpec {spec.GetType().Name}"),
            };
        }

        static string StatJa(string stat) => stat switch
        {
            "Mass" => "質量",
            "Friction" => "摩擦",
            "Bounciness" => "反発",
            "Power" => "パワー",
            "Speed" => "スピード",
            _ => stat,
        };

        static string RuleJa(string rule) => rule switch
        {
            "OpponentTurnTime" => "相手ターン時間",
            "SelfTurnTime" => "自分ターン時間",
            "HideTrajectory" => "予測封じ",
            "PushEdge" => "縁押し",
            "CenterBias" => "中央寄せ",
            "CooldownCut" => "クールダウン短縮",
            _ => rule,
        };

        static float ParseFloat(string raw) =>
            float.Parse(raw, CultureInfo.InvariantCulture);

        static int ParseInt(string raw) =>
            int.Parse(raw, CultureInfo.InvariantCulture);
    }
}
