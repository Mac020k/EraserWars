using System;
using System.Collections.Generic;

namespace EraserWars.Cards
{
    public static class CharacterFlavor
    {
        readonly struct Pair
        {
            public readonly string Strong;
            public readonly string Weak;

            public Pair(string strong, string weak)
            {
                Strong = strong;
                Weak = weak;
            }
        }

        static readonly Dictionary<int, Pair> subjects = new()
        {
            { 0, new Pair("体育", "算数") },
            { 1, new Pair("体育", "音楽") },
            { 2, new Pair("算数", "体育") },
            { 3, new Pair("図工", "体育") },
            { 4, new Pair("英語", "国語") },
            { 5, new Pair("体育", "家庭科") },
            { 6, new Pair("理科", "国語") },
            { 7, new Pair("英語", "算数") },
            { 8, new Pair("体育", "音楽") },
            { 9, new Pair("体育", "国語") },
            { 10, new Pair("国語", "体育") },
            { 11, new Pair("体育", "算数") },
            { 12, new Pair("図工", "算数") },
            { 13, new Pair("理科", "国語") },
            { 14, new Pair("家庭科", "英語") },
            { 15, new Pair("音楽", "算数") },
            { 16, new Pair("家庭科", "図工") },
            { 17, new Pair("社会", "体育") },
            { 18, new Pair("体育", "国語") },
            { 19, new Pair("家庭科", "算数") },
        };

        public static bool TrySubjects(int characterId, out string strong, out string weak)
        {
            if (subjects.TryGetValue(characterId, out var pair))
            {
                strong = pair.Strong;
                weak = pair.Weak;
                return true;
            }

            strong = string.Empty;
            weak = string.Empty;
            return false;
        }
    }

    public static class PassiveText
    {
        readonly struct Copy
        {
            public readonly string Name;
            public readonly string Effect;

            public Copy(string name, string effect)
            {
                Name = name;
                Effect = effect;
            }
        }

        static readonly Dictionary<string, Copy> byKey = new(StringComparer.OrdinalIgnoreCase)
        {
            { "passive.none", new Copy("なし", "特別な効果はない。") },
            { "none", new Copy("なし", "特別な効果はない。") },
            { "passive.knock_plus", new Copy("どすこい", "衝突で相手を弾きやすい。") },
            { "passive.sway_minus", new Copy("検算", "フリックのブレが小さい。") },
            { "passive.edge_grip", new Copy("のり止め", "机の縁で滑りにくい。") },
            { "passive.first_slide", new Copy("初日の滑走", "最初の一滑りが伸びる。") },
            { "passive.ally_bounce", new Copy("声かけ", "味方との跳ね返りが整う。") },
            { "passive.desk_object_plus", new Copy("試作カバー", "机に置いた仕掛けが強い。") },
            { "passive.hit_friction", new Copy("ペース奪取", "当たった相手が滑りにくくなる。") },
            { "passive.edge_power", new Copy("縁の火力", "縁から遠いほど力が出る。") },
            { "passive.spin_stable", new Copy("安定スピン", "回転しても進路が安定する。") },
            { "passive.traj_plus", new Copy("予測線", "軌道の読みが長くなる。") },
            { "passive.center_mass", new Copy("中央陣地", "机の中央で止まりやすい。") },
            { "passive.dust_speed", new Copy("カス走り", "消しゴムカスの上で速い。") },
            { "passive.weapon_plus", new Copy("実験増幅", "武器カードの力積が増える。") },
            { "passive.hit_enemy_friction", new Copy("ベタつき", "当たった相手の摩擦が上がる。") },
            { "passive.timer_card_plus", new Copy("秒読み", "制限時間カードが伸びる。") },
            { "passive.dust_self_normal", new Copy("高摩擦", "自分はカスの影響を受けにくい。") },
            { "passive.shield_edge", new Copy("規律", "シールド中は縁を一度だけ許す。") },
            { "passive.power_sway", new Copy("全力ブレ", "パワーが上がり、ブレも大きい。") },
            { "passive.swap_teleport", new Copy("ねこ移動", "入れ替えとテレポートが得意。") },
        };

        public static void Describe(string passiveKey, out string name, out string effect)
        {
            if (string.IsNullOrWhiteSpace(passiveKey))
            {
                name = "なし";
                effect = "特別な効果はない。";
                return;
            }

            if (byKey.TryGetValue(passiveKey.Trim(), out var copy))
            {
                name = copy.Name;
                effect = copy.Effect;
                return;
            }

            name = passiveKey.Trim();
            effect = string.Empty;
        }
    }
}
