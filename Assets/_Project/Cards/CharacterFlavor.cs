using System;
using System.Collections.Generic;

namespace EraserWars.Cards
{
    public static class CharacterFlavor
    {
        readonly struct Profile
        {
            public readonly string Strong;
            public readonly string Weak;
            public readonly string SchoolJa;
            public readonly string ClassJa;

            public Profile(string strong, string weak, string schoolJa, string classJa)
            {
                Strong = strong;
                Weak = weak;
                SchoolJa = schoolJa;
                ClassJa = classJa;
            }
        }

        const string Himawari = "ひまわり小学校";
        const string Aoba = "青葉小学校";

        static readonly Dictionary<int, Profile> subjects = new()
        {
            { 0, new Profile("体育", "算数", Himawari, "3年2組") },
            { 1, new Profile("体育", "音楽", Himawari, "3年2組") },
            { 2, new Profile("算数", "体育", Himawari, "3年1組") },
            { 3, new Profile("図工", "体育", Himawari, "3年2組") },
            { 4, new Profile("英語", "国語", Himawari, "3年2組") },
            { 5, new Profile("体育", "家庭科", Himawari, "6年1組") },
            { 6, new Profile("理科", "国語", Aoba, "4年3組") },
            { 7, new Profile("英語", "算数", "", "留学生の先生役") },
            { 8, new Profile("体育", "音楽", Himawari, "5年1組") },
            { 9, new Profile("体育", "国語", Aoba, "4年1組") },
            { 10, new Profile("国語", "体育", Himawari, "3年1組") },
            { 11, new Profile("体育", "算数", Himawari, "6年2組") },
            { 12, new Profile("図工", "算数", Aoba, "5年2組") },
            { 13, new Profile("理科", "国語", Aoba, "6年3組") },
            { 14, new Profile("家庭科", "英語", Himawari, "4年2組") },
            { 15, new Profile("音楽", "算数", "", "先生役") },
            { 16, new Profile("家庭科", "図工", "", "先生役") },
            { 17, new Profile("社会", "体育", "", "先生役") },
            { 18, new Profile("体育", "国語", Himawari, "3年2組") },
            { 19, new Profile("家庭科", "算数", Aoba, "3年3組") },
        };

        public static bool TrySubjects(int characterId, out string strong, out string weak)
        {
            if (subjects.TryGetValue(characterId, out var profile))
            {
                strong = profile.Strong;
                weak = profile.Weak;
                return true;
            }

            strong = string.Empty;
            weak = string.Empty;
            return false;
        }

        public static bool TryPlace(int characterId, out string schoolJa, out string classJa)
        {
            if (subjects.TryGetValue(characterId, out var profile))
            {
                schoolJa = profile.SchoolJa;
                classJa = profile.ClassJa;
                return true;
            }

            schoolJa = string.Empty;
            classJa = string.Empty;
            return false;
        }

        public static string TileCaption(int characterId)
        {
            return TryPlace(characterId, out _, out var classJa) ? classJa : string.Empty;
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
