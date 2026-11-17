using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public enum UiLang
    {
        Ja,
        En,
    }

    /// <summary>
    /// PlayerPrefs language for start menu and match HUD copy.
    /// A missing or unknown value stays Japanese. Options has no language control yet.
    /// </summary>
    public static class UiLocale
    {
        public const string PrefKey = "eraserwars.locale";
        public const string Japanese = "ja";
        public const string English = "en";

        public static UiLang Current { get; private set; } = UiLang.Ja;

        public static void Load()
        {
            var raw = PlayerPrefs.GetString(PrefKey, Japanese);
            Current = raw == English ? UiLang.En : UiLang.Ja;
        }

        public static void Set(UiLang lang)
        {
            Current = lang == UiLang.En ? UiLang.En : UiLang.Ja;
            PlayerPrefs.SetString(PrefKey, Current == UiLang.En ? English : Japanese);
            PlayerPrefs.Save();
        }
    }

    public enum UiKey
    {
        MenuTitleJa,
        MenuTitleEn,
        MenuTwoPlayers,
        MenuFourPlayers,
        MenuStock,
        MenuPoints,
        MenuTip,
        MenuTipDismiss,
        MenuResume,
        MenuStart,
        MenuNewMatch,
        MenuNavDeck,
        MenuNavCollection,
        MenuNavOptions,
        MenuSecretRoom,
        SecretRoomTitle,
        SecretRoomNote,
        SecretRoomCodeLabel,
        SecretRoomGenerate,
        SecretRoomCopy,
        SecretRoomMade,
        SecretRoomCopied,
        SecretRoomBadCode,
        MenuBack,
        OptTitle,
        OptSectionVolume,
        OptMaster,
        OptBgm,
        OptSe,
        OptNoteVolume,
        OptSectionMatch,
        OptFlick,
        OptTurn,
        OptShakeOn,
        OptShakeOff,
        OptNoteMatch,
        OptSectionDisplay,
        OptUiSize,
        OptUiLarge,
        OptSectionDev,
        OptDebugOn,
        OptDebugOff,
        OptNoteDebug,
        OptReset,
        OptResetConfirm,
        DeckTitle,
        DeckSave,
        DeckColCharacter,
        DeckColCommon,
        DeckEmpty,
        CollectionTitle,
        CollectionTabCharacters,
        CollectionTabCards,
        HudTitle,
        HudMenu,
        HudTimerUnit,
        HudResultKicker,
        HudRematch,
        HudToMenu,
        HudPass,
        HudRound,
        HudPhaseReady,
        HudPhaseCard,
        HudPhaseAim,
        HudPhaseRoll,
        HudPhaseResult,
        HudPhaseRoundEnd,
        HudPhaseMatchEnd,
        HudSeatTurn,
        HudTurnOver,
        HudPassHint,
        HudHand,
        HudCutInSpecial,
        HudCutInCard,
        HudXp,
        HudWin,
        HudDraw,
        HudStockLine,
        HudPointsLine,
        HudPoints,
        HudNow,
        HudYieldPass,
        HudYieldTime,
        HudYieldCutoff,
        OptSectionAccess,
        OptReduceOn,
        OptReduceOff,
        OptPaletteOn,
        OptPaletteOff,
        OptNoteAccess,
    }

    public static class UiText
    {
        public readonly struct Slot
        {
            public Slot(string elementName, UiKey key)
            {
                ElementName = elementName;
                Key = key;
            }

            public string ElementName { get; }
            public UiKey Key { get; }
        }

        readonly struct Line
        {
            public Line(string ja, string en)
            {
                Ja = ja;
                En = en;
            }

            public string Ja { get; }
            public string En { get; }
        }

        static readonly Dictionary<UiKey, Line> Lines = new Dictionary<UiKey, Line>
        {
            { UiKey.MenuTitleJa, new Line("消しピンウォーズ", "Eraser Wars") },
            { UiKey.MenuTitleEn, new Line("ERASER WARS", "ERASER WARS") },
            { UiKey.MenuTwoPlayers, new Line("2人対戦", "2 players") },
            { UiKey.MenuFourPlayers, new Line("4人対戦", "4 players") },
            { UiKey.MenuStock, new Line("ストック戦", "Stock") },
            { UiKey.MenuPoints, new Line("ポイント戦", "Points") },
            { UiKey.MenuTip, new Line("カードを1枚出して、ドラッグではじく。", "Play one card, then drag to flick.") },
            { UiKey.MenuTipDismiss, new Line("わかった", "OK") },
            { UiKey.MenuResume, new Line("つづける", "Resume") },
            { UiKey.MenuStart, new Line("マッチ開始", "Start match") },
            { UiKey.MenuNewMatch, new Line("あたらしいマッチ", "New match") },
            { UiKey.MenuNavDeck, new Line("デッキ", "Deck") },
            { UiKey.MenuNavCollection, new Line("コレクション", "Collection") },
            { UiKey.MenuNavOptions, new Line("オプション", "Options") },
            { UiKey.MenuSecretRoom, new Line("ひみつのへや", "Secret room") },
            { UiKey.SecretRoomTitle, new Line("ひみつのへや", "Secret room") },
            { UiKey.SecretRoomNote, new Line("まだ通信しません。コードはこの端末の中だけです。", "No connection yet. This code stays on this device.") },
            { UiKey.SecretRoomCodeLabel, new Line("参加コード", "Join code") },
            { UiKey.SecretRoomGenerate, new Line("コードをつくる", "Make a code") },
            { UiKey.SecretRoomCopy, new Line("コピー", "Copy") },
            { UiKey.SecretRoomMade, new Line("できました。", "Code ready.") },
            { UiKey.SecretRoomCopied, new Line("コピーしました。", "Copied.") },
            { UiKey.SecretRoomBadCode, new Line("6文字にしてください。0、1、I、O は使えません。", "Use 6 characters. Skip 0, 1, I, and O.") },
            { UiKey.MenuBack, new Line("← もどる", "← Back") },
            { UiKey.OptTitle, new Line("オプション", "Options") },
            { UiKey.OptSectionVolume, new Line("音量", "Volume") },
            { UiKey.OptMaster, new Line("マスター {0}", "Master {0}") },
            { UiKey.OptBgm, new Line("BGM {0}", "BGM {0}") },
            { UiKey.OptSe, new Line("SE {0}", "SE {0}") },
            { UiKey.OptNoteVolume, new Line("マスターは今すぐ変わります。BGM と SE は音が入ると、この割合で鳴ります。", "Master changes now. BGM and SE use this mix once those sounds play.") },
            { UiKey.OptSectionMatch, new Line("対戦", "Match") },
            { UiKey.OptFlick, new Line("フリックの強さ {0}", "Flick power {0}") },
            { UiKey.OptTurn, new Line("1ターン {0}秒", "1 turn {0}s") },
            { UiKey.OptShakeOn, new Line("カメラ揺れ: ON", "Camera shake: ON") },
            { UiKey.OptShakeOff, new Line("カメラ揺れ: OFF", "Camera shake: OFF") },
            { UiKey.OptNoteMatch, new Line("フリックの強さ、ターン時間、カメラ揺れは保存されます。試合側がこの値を読むと効きます。", "Flick power, turn time, and camera shake are saved. A match applies them when it reads these values.") },
            { UiKey.OptSectionDisplay, new Line("表示", "Display") },
            { UiKey.OptUiSize, new Line("画面の大きさ", "Screen size") },
            { UiKey.OptUiLarge, new Line("大", "Large") },
            { UiKey.OptSectionAccess, new Line("見やすさ", "Readability") },
            { UiKey.OptReduceOn, new Line("動きを減らす: ON", "Reduce motion: ON") },
            { UiKey.OptReduceOff, new Line("動きを減らす: OFF", "Reduce motion: OFF") },
            { UiKey.OptPaletteOn, new Line("座席の色: 色覚サポート", "Seat colors: distinct") },
            { UiKey.OptPaletteOff, new Line("座席の色: 標準", "Seat colors: standard") },
            { UiKey.OptNoteAccess, new Line("動きを減らすと、画面端の光は点滅せず、うすく固定されます。座席の色は次の試合のコマと表示に使います。", "Reduce motion holds the edge glow still and dim. Seat colors apply to the erasers and the HUD on the next match.") },
            { UiKey.OptSectionDev, new Line("開発", "Dev") },
            { UiKey.OptDebugOn, new Line("デバッグモード: ON", "Debug mode: ON") },
            { UiKey.OptDebugOff, new Line("デバッグモード: OFF", "Debug mode: OFF") },
            { UiKey.OptNoteDebug, new Line("リリース前にこのボタンを非表示にします。", "Hide this button before release.") },
            { UiKey.OptReset, new Line("初期値に戻す", "Reset") },
            { UiKey.OptResetConfirm, new Line("もう一度押すと初期値に戻します", "Press again to reset") },
            { UiKey.DeckTitle, new Line("デッキ", "Deck") },
            { UiKey.DeckSave, new Line("保存", "Save") },
            { UiKey.DeckColCharacter, new Line("キャラ", "Characters") },
            { UiKey.DeckColCommon, new Line("コモンカード", "Common cards") },
            { UiKey.DeckEmpty, new Line("デッキは空です。カードの「追加」で入れてください。", "The deck is empty. Use Add on a card.") },
            { UiKey.CollectionTitle, new Line("コレクション", "Collection") },
            { UiKey.CollectionTabCharacters, new Line("キャラ", "Characters") },
            { UiKey.CollectionTabCards, new Line("カード", "Cards") },
            { UiKey.HudTitle, new Line("ERASER WARS", "ERASER WARS") },
            { UiKey.HudMenu, new Line("メニュー", "Menu") },
            { UiKey.HudTimerUnit, new Line("秒", "s") },
            { UiKey.HudResultKicker, new Line("しょうぶあり", "Decided") },
            { UiKey.HudRematch, new Line("再戦", "Rematch") },
            { UiKey.HudToMenu, new Line("メニューへ", "To menu") },
            { UiKey.HudPass, new Line("パス", "Pass") },
            { UiKey.HudRound, new Line("ROUND {0}  {1}", "ROUND {0}  {1}") },
            { UiKey.HudPhaseReady, new Line("じゅんびちゅう", "Get ready") },
            { UiKey.HudPhaseCard, new Line("カード / ねらえ!", "Card / Aim!") },
            { UiKey.HudPhaseAim, new Line("ねらえ!", "Aim!") },
            { UiKey.HudPhaseRoll, new Line("ころがり中...", "Rolling...") },
            { UiKey.HudPhaseResult, new Line("けっか", "Result") },
            { UiKey.HudPhaseRoundEnd, new Line("ラウンド終了", "Round over") },
            { UiKey.HudPhaseMatchEnd, new Line("しょうぶあり!", "Decided!") },
            { UiKey.HudSeatTurn, new Line("P{0} のばん", "P{0}'s turn") },
            { UiKey.HudTurnOver, new Line("おわり", "End") },
            { UiKey.HudPassHint, new Line("このきを P{0} にわたして", "Hand the device to P{0}") },
            { UiKey.HudHand, new Line("P{0} のカード", "P{0}'s cards") },
            { UiKey.HudCutInSpecial, new Line("P{0}  とくぎ", "P{0}  Special") },
            { UiKey.HudCutInCard, new Line("P{0}  カード", "P{0}  Card") },
            { UiKey.HudXp, new Line("けいけん +{0}  ·  ごうけい {1}", "XP +{0}  ·  total {1}") },
            { UiKey.HudWin, new Line("P{0} のかち!", "P{0} wins!") },
            { UiKey.HudDraw, new Line("ひきわけ!", "Draw!") },
            { UiKey.HudStockLine, new Line("P{0} ストック {1}", "P{0} stock {1}") },
            { UiKey.HudPointsLine, new Line("P{0} {1}点", "P{0} {1} pts") },
            { UiKey.HudPoints, new Line("{0}点", "{0} pts") },
            { UiKey.HudNow, new Line("いま", "Now") },
            { UiKey.HudYieldPass, new Line("パスした", "Passed") },
            { UiKey.HudYieldTime, new Line("じかんぎれ", "Time's up") },
            { UiKey.HudYieldCutoff, new Line("うちきり", "Cutoff") },
        };

        public static readonly Slot[] StartMenuSlots =
        {
            new Slot("title-ja", UiKey.MenuTitleJa),
            new Slot("title-en", UiKey.MenuTitleEn),
            new Slot("mode-2p", UiKey.MenuTwoPlayers),
            new Slot("mode-4p", UiKey.MenuFourPlayers),
            new Slot("mode-stock", UiKey.MenuStock),
            new Slot("mode-points", UiKey.MenuPoints),
            new Slot("home-tip-copy", UiKey.MenuTip),
            new Slot("home-tip-dismiss", UiKey.MenuTipDismiss),
            new Slot("resume-match", UiKey.MenuResume),
            new Slot("nav-deck", UiKey.MenuNavDeck),
            new Slot("nav-collection", UiKey.MenuNavCollection),
            new Slot("nav-options", UiKey.MenuNavOptions),
            new Slot("open-secret-room", UiKey.MenuSecretRoom),
            new Slot("secret-room-title", UiKey.SecretRoomTitle),
            new Slot("secret-room-note", UiKey.SecretRoomNote),
            new Slot("secret-room-code-label", UiKey.SecretRoomCodeLabel),
            new Slot("secret-generate", UiKey.SecretRoomGenerate),
            new Slot("secret-copy", UiKey.SecretRoomCopy),
            new Slot("close-secret-room", UiKey.MenuBack),
            new Slot("close-options", UiKey.MenuBack),
            new Slot("close-deck", UiKey.MenuBack),
            new Slot("close-collection", UiKey.MenuBack),
            new Slot("options-title", UiKey.OptTitle),
            new Slot("opt-section-volume", UiKey.OptSectionVolume),
            new Slot("opt-note-volume", UiKey.OptNoteVolume),
            new Slot("opt-section-match", UiKey.OptSectionMatch),
            new Slot("opt-note-match", UiKey.OptNoteMatch),
            new Slot("opt-section-display", UiKey.OptSectionDisplay),
            new Slot("opt-ui-size", UiKey.OptUiSize),
            new Slot("opt-ui-120", UiKey.OptUiLarge),
            new Slot("opt-section-access", UiKey.OptSectionAccess),
            new Slot("opt-note-access", UiKey.OptNoteAccess),
            new Slot("opt-section-dev", UiKey.OptSectionDev),
            new Slot("opt-note-debug", UiKey.OptNoteDebug),
            new Slot("deck-title", UiKey.DeckTitle),
            new Slot("deck-save", UiKey.DeckSave),
            new Slot("deck-col-character", UiKey.DeckColCharacter),
            new Slot("deck-col-common", UiKey.DeckColCommon),
            new Slot("deck-empty", UiKey.DeckEmpty),
            new Slot("collection-title", UiKey.CollectionTitle),
            new Slot("collection-tab-characters", UiKey.CollectionTabCharacters),
            new Slot("collection-tab-cards", UiKey.CollectionTabCards),
        };

        public static readonly Slot[] MatchHudSlots =
        {
            new Slot("title", UiKey.HudTitle),
            new Slot("menu", UiKey.HudMenu),
            new Slot("timer-unit", UiKey.HudTimerUnit),
            new Slot("result-kicker", UiKey.HudResultKicker),
            new Slot("rematch", UiKey.HudRematch),
            new Slot("match-end-menu", UiKey.HudToMenu),
            new Slot("pass-label", UiKey.HudPass),
        };

        public static string Get(UiKey key)
        {
            if (!Lines.TryGetValue(key, out var line))
            {
                return key.ToString();
            }

            return UiLocale.Current == UiLang.En ? line.En : line.Ja;
        }

        public static string Format(UiKey key, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, Get(key), args);
        }

        public static void Apply(VisualElement root, Slot[] slots)
        {
            if (root == null)
            {
                return;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                var element = root.Q<TextElement>(slots[i].ElementName);
                if (element == null)
                {
                    continue;
                }

                element.text = Get(slots[i].Key);
            }
        }
    }
}
