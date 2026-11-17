using System;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using EraserWars.Persistence;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class MatchHudController : MonoBehaviour
    {
        const string PillActiveClass = "pill-active";
        const string PillOutClass = "pill-out";
        const string HeartLostClass = "heart-lost";
        const string HiddenClass = "hidden";
        const string HandSelectedClass = "hand-card-selected";
        const string TimerUrgentClass = "timer-urgent";
        const string TimerIdleClass = "timer-idle";
        const int SeatClassCount = 4;

        [SerializeField] MatchDirector director;
        [SerializeField] UIDocument document;
        [SerializeField] StartMenuController startMenu;
        [SerializeField] Texture2D iconStock;
        [SerializeField] Texture2D iconPass;

        readonly List<PlayerPill> pills = new();
        KnockPraiseView praiseView;
        MatchDirector praiseDirector;

        VisualElement root;
        VisualElement playersRow;
        VisualElement handDock;
        VisualElement handRow;
        Label handTitle;
        Label phaseLabel;
        Label timerLabel;
        Label timerUnit;
        Label turnLabel;
        Label passHintLabel;
        VisualElement turnCallout;
        VisualElement turnAnnounce;
        Label turnAnnounceLine;
        VisualElement timerBox;
        Label turnYieldLabel;
        VisualElement cardCutIn;
        VisualElement cardCutInDim;
        VisualElement cardCutInBanner;
        VisualElement cardCutInFace;
        Label cardCutInKicker;
        Label cardCutInName;
        int paintedCutInCardId = int.MinValue;
        int paintedCutInSeat = int.MinValue;
        Button passButton;
        Button rematchButton;
        Button matchEndMenuButton;
        VisualElement matchEnd;
        VisualElement matchEndCard;
        Label resultText;
        Label resultSummary;
        Label resultXp;
        bool finishXpGranted;
        int finishXpGained;
        int finishXpTotal;
        string handFingerprint = string.Empty;
        int selectedHandIndex = -1;
        Button boundPass;

        sealed class PlayerPill
        {
            public VisualElement Root;
            public VisualElement HeartsRow;
            public Label Points;
            public Label Now;
            public readonly List<VisualElement> Hearts = new();
        }

        void OnEnable()
        {
            UiLocale.Load();
            EnsureDocument();
            if (document == null)
            {
                return;
            }

            document.sortingOrder = 50;

            if (startMenu == null)
            {
                startMenu = FindAnyObjectByType<StartMenuController>();
            }

            BindTree();
            SetVisible(false);
        }

        void EnsureDocument()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }
        }

        void BindTree()
        {
            EnsureDocument();
            if (document == null)
            {
                return;
            }

            var tree = document.rootVisualElement;
            if (tree == null)
            {
                return;
            }

            var next = tree.Q("root") ?? tree;
            var liveName = next.Q<Label>("card-cutin-name");
            var livePass = next.Q<Button>("pass");
            if (liveName != null
                && ReferenceEquals(liveName, cardCutInName)
                && ReferenceEquals(livePass, passButton))
            {
                return;
            }

            root = next;
            playersRow = root.Q("players");
            handDock = root.Q("hand-dock");
            handRow = root.Q("hand");
            handTitle = root.Q<Label>("hand-title");
            passButton = livePass;
            rematchButton = root.Q<Button>("rematch");
            matchEndMenuButton = root.Q<Button>("match-end-menu");
            phaseLabel = root.Q<Label>("phase");
            timerLabel = root.Q<Label>("timer");
            timerUnit = root.Q<Label>("timer-unit");
            turnLabel = root.Q<Label>("turn");
            passHintLabel = root.Q<Label>("pass-hint");
            turnCallout = root.Q("turn-callout");
            turnAnnounce = root.Q("turn-announce");
            turnAnnounceLine = root.Q<Label>("turn-announce-line");
            timerBox = root.Q("timer-box");
            turnYieldLabel = root.Q<Label>("turn-yield");
            cardCutIn = root.Q("card-cutin");
            cardCutInDim = root.Q("card-cutin-dim");
            cardCutInBanner = root.Q("card-cutin-banner");
            cardCutInFace = root.Q("card-cutin-face");
            cardCutInKicker = root.Q<Label>("card-cutin-kicker");
            cardCutInName = liveName;
            paintedCutInCardId = int.MinValue;
            paintedCutInSeat = int.MinValue;
            matchEnd = root.Q("match-end");
            matchEndCard = root.Q("match-end-card");
            resultText = root.Q<Label>("result-text");
            resultSummary = root.Q<Label>("result-summary");
            resultXp = root.Q<Label>("result-xp");

            if (passButton == null || ReferenceEquals(passButton, boundPass))
            {
                ApplyChrome();
                return;
            }

            boundPass = passButton;
            pills.Clear();
            playersRow?.Clear();
            handRow?.Clear();
            handFingerprint = string.Empty;
            selectedHandIndex = -1;
            ApplyIcon(root.Q("pass-icon"), iconPass);
            EnsurePraiseView();
            HookPraise();
            passButton?.RegisterCallback<ClickEvent>(_ => director?.Pass());
            rematchButton?.RegisterCallback<ClickEvent>(_ => director?.Rematch());
            matchEndMenuButton?.RegisterCallback<ClickEvent>(_ => startMenu?.ShowMenu());
            root.Q<Button>("menu")?.RegisterCallback<ClickEvent>(_ => startMenu?.ShowMenu());
            ApplyChrome();
        }

        void OnDisable()
        {
            UnhookPraise();
        }

        void Update()
        {
            praiseView?.Tick(Time.unscaledDeltaTime);
            RefreshFromState();
        }

        public void RefreshFromState()
        {
            BindTree();
            ApplySeatPalette();
            ApplyChrome();
            if (director == null)
            {
                director = FindAnyObjectByType<MatchDirector>();
            }

            HookPraise();

            if (startMenu == null)
            {
                startMenu = FindAnyObjectByType<StartMenuController>(FindObjectsInactive.Include);
            }

            if (director?.State == null)
            {
                SetVisible(false);
                return;
            }

            var menuUp = startMenu != null
                && startMenu.isActiveAndEnabled
                && startMenu.IsMenuVisible;
            if (menuUp)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            PaintEdgeCue();
            var state = director.State;
            if (phaseLabel != null)
            {
                phaseLabel.text = UiText.Format(UiKey.HudRound, state.Round, PhaseText(state.Phase));
            }

            RefreshTurnChrome(state);
            RefreshTurnAnnounce(state);
            RefreshActions(state);
            RefreshPills(state);
            RefreshHand(state);
            RefreshResultBanner(state);
            RefreshCardCutIn();
        }

        void RefreshTurnChrome(MatchState state)
        {
            var chrome = TurnChrome.From(state);
            var cutInUp = director != null && director.IsCardCutInShowing;
            if (turnCallout != null)
            {
                turnCallout.EnableInClassList(HiddenClass, !chrome.ShowCallout || cutInUp);
                SetSeatClass(turnCallout, chrome.Seat);
            }

            if (turnLabel != null)
            {
                turnLabel.text = chrome.Headline;
            }

            if (passHintLabel != null)
            {
                passHintLabel.text = chrome.PassHint;
                passHintLabel.EnableInClassList(HiddenClass, !chrome.ShowPassHint);
            }

            if (timerLabel != null)
            {
                timerLabel.text = chrome.TimerDigits;
            }

            timerUnit?.EnableInClassList(HiddenClass, !chrome.TimerTicking);
            timerBox?.EnableInClassList(TimerIdleClass, !chrome.TimerTicking);
            timerBox?.EnableInClassList(TimerUrgentClass, chrome.TimerUrgent);
            PaintTurnYield(cutInUp);
        }

        void RefreshTurnAnnounce(MatchState state)
        {
            if (turnAnnounce == null)
            {
                return;
            }

            var covered = (director != null && director.IsCardCutInShowing) || state.Phase == Phase.MatchEnd;
            if (director == null || !director.TryGetTurnAnnounce(out var announce))
            {
                turnAnnounce.EnableInClassList(HiddenClass, true);
                return;
            }

            turnAnnounce.EnableInClassList(HiddenClass, !announce.IsOnScreen(covered));

            SetSeatClass(turnAnnounce, announce.Seat);
            if (turnAnnounceLine != null)
            {
                turnAnnounceLine.text = TurnChrome.SeatLine(announce.Seat);
            }
        }

        void PaintTurnYield(bool suppressed)
        {
            if (turnYieldLabel == null)
            {
                return;
            }

            if (suppressed || director == null || !director.TryGetTurnYield(out var notice))
            {
                turnYieldLabel.EnableInClassList(HiddenClass, true);
                return;
            }

            turnYieldLabel.EnableInClassList(HiddenClass, false);
            SetSeatClass(turnYieldLabel, notice.Seat);
            switch (notice.Kind)
            {
                case TurnYieldKind.Pass:
                    turnYieldLabel.text = UiText.Get(UiKey.HudYieldPass);
                    turnYieldLabel.EnableInClassList("yield-pass", true);
                    turnYieldLabel.EnableInClassList("yield-time", false);
                    turnYieldLabel.EnableInClassList("yield-cutoff", false);
                    break;
                case TurnYieldKind.Time:
                    turnYieldLabel.text = UiText.Get(UiKey.HudYieldTime);
                    turnYieldLabel.EnableInClassList("yield-pass", false);
                    turnYieldLabel.EnableInClassList("yield-time", true);
                    turnYieldLabel.EnableInClassList("yield-cutoff", false);
                    break;
                case TurnYieldKind.Cutoff:
                    turnYieldLabel.text = UiText.Get(UiKey.HudYieldCutoff);
                    turnYieldLabel.EnableInClassList("yield-pass", false);
                    turnYieldLabel.EnableInClassList("yield-time", false);
                    turnYieldLabel.EnableInClassList("yield-cutoff", true);
                    break;
                default:
                    throw UnhandledYield(notice.Kind);
            }
        }

        static ArgumentOutOfRangeException UnhandledYield(TurnYieldKind kind) => kind switch
        {
            TurnYieldKind.Pass => new ArgumentOutOfRangeException(nameof(kind), kind, null),
            TurnYieldKind.Time => new ArgumentOutOfRangeException(nameof(kind), kind, null),
            TurnYieldKind.Cutoff => new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        void RefreshActions(MatchState state)
        {
            var ended = state.Phase == Phase.MatchEnd;
            var cutInUp = director != null && director.IsCardCutInShowing;
            var cpuSeat = director != null && director.IsCpuSeat(state.CurrentPlayerId.Value);
            passButton?.EnableInClassList(HiddenClass, ended || cutInUp);
            passButton?.SetEnabled(
                !cpuSeat && state.Phase is Phase.CardSelect or Phase.Aim or Phase.TurnStart);
        }

        void RefreshHand(MatchState state)
        {
            if (handDock == null || handRow == null)
            {
                return;
            }

            var cpuSeat = director != null && director.IsCpuSeat(state.CurrentPlayerId.Value);
            var canPlay = !cpuSeat && state.Phase is Phase.CardSelect or Phase.Aim;
            var cutInUp = director != null && director.IsCardCutInShowing;
            if (!canPlay || cutInUp)
            {
                selectedHandIndex = -1;
                ApplyHandSelection();
            }

            handDock.EnableInClassList(HiddenClass, !canPlay || cutInUp);
            if (!canPlay)
            {
                return;
            }

            SetSeatClass(handDock, state.CurrentPlayerId.Value);
            if (handTitle != null)
            {
                handTitle.text = UiText.Format(UiKey.HudHand, state.CurrentPlayerId.Value + 1);
            }

            var player = state.Players[state.CurrentPlayerId.Value];
            var fingerprint = $"{state.CurrentPlayerId.Value}:{state.Phase}:{player.Hand.Length}:";
            for (var i = 0; i < player.Hand.Length; i++)
            {
                fingerprint += player.Hand[i].Value + ",";
            }

            if (fingerprint == handFingerprint && handRow.childCount == player.Hand.Length)
            {
                ApplyHandSelection();
                return;
            }

            handFingerprint = fingerprint;
            selectedHandIndex = -1;
            handRow.Clear();
            for (var i = 0; i < player.Hand.Length; i++)
            {
                var cardId = player.Hand[i].Value;
                director.TryGetCard(cardId, out var definition);
                var button = new Button { text = string.Empty };
                if (definition != null)
                {
                    CardFaceBuilder.Paint(button, definition, CardSurface.Hand);
                }
                else
                {
                    CardFaceBuilder.PaintUnknown(button, $"#{cardId}", CardSurface.Hand);
                }

                var captured = cardId;
                var index = i;
                button.clicked += () => director?.PlayCard(captured);
                button.RegisterCallback<PointerEnterEvent>(_ => SelectHandCard(index));
                button.RegisterCallback<PointerLeaveEvent>(_ => ClearHandCard(index));
                button.RegisterCallback<FocusInEvent>(_ => SelectHandCard(index));
                button.RegisterCallback<FocusOutEvent>(_ => ClearHandCard(index));
                handRow.Add(button);
            }

            ApplyHandSelection();
        }

        void SelectHandCard(int index)
        {
            if (handRow == null || index < 0 || index >= handRow.childCount)
            {
                return;
            }

            selectedHandIndex = index;
            ApplyHandSelection();
        }

        void ClearHandCard(int index)
        {
            if (selectedHandIndex != index)
            {
                return;
            }

            selectedHandIndex = -1;
            ApplyHandSelection();
        }

        void ApplyHandSelection()
        {
            if (handRow == null)
            {
                return;
            }

            if (selectedHandIndex < 0 || selectedHandIndex >= handRow.childCount)
            {
                selectedHandIndex = -1;
            }

            for (var i = 0; i < handRow.childCount; i++)
            {
                handRow[i].EnableInClassList(HandSelectedClass, i == selectedHandIndex);
            }
        }

        void RefreshCardCutIn()
        {
            if (cardCutIn == null)
            {
                return;
            }

            if (director == null || !director.TryGetCardCutIn(out var cut))
            {
                cardCutIn.EnableInClassList(HiddenClass, true);
                paintedCutInCardId = int.MinValue;
                paintedCutInSeat = int.MinValue;
                return;
            }

            cardCutIn.EnableInClassList(HiddenClass, false);
            cardCutIn.EnableInClassList("card-cutin-signature", cut.Signature);
            var pose = cut.Pose();
            if (cardCutInBanner != null)
            {
                SetSeatClass(cardCutInBanner, cut.Seat);
                cardCutInBanner.style.translate = new Translate(new Length(pose.SlidePixels), new Length(0f));
                cardCutInBanner.style.opacity = pose.Opacity;
            }

            if (cardCutInDim != null)
            {
                cardCutInDim.style.opacity = pose.Opacity;
            }

            if (cardCutInKicker != null)
            {
                cardCutInKicker.text = cut.Signature
                    ? UiText.Format(UiKey.HudCutInSpecial, cut.Seat + 1)
                    : UiText.Format(UiKey.HudCutInCard, cut.Seat + 1);
            }

            if (cardCutInName != null)
            {
                cardCutInName.text = cut.DisplayName;
            }

            if (cardCutInFace == null || (paintedCutInCardId == cut.CardId && paintedCutInSeat == cut.Seat))
            {
                return;
            }

            paintedCutInCardId = cut.CardId;
            paintedCutInSeat = cut.Seat;
            if (director.TryGetCard(cut.CardId, out var definition) && definition != null)
            {
                CardFaceBuilder.Paint(cardCutInFace, definition, CardSurface.CutIn);
            }
            else
            {
                CardFaceBuilder.PaintUnknown(cardCutInFace, cut.DisplayName, CardSurface.CutIn);
            }
        }

        void RefreshResultBanner(MatchState state)
        {
            if (matchEnd == null)
            {
                return;
            }

            var ended = state.Phase == Phase.MatchEnd;
            matchEnd.EnableInClassList(HiddenClass, !ended);
            if (!ended)
            {
                finishXpGranted = false;
                if (resultXp != null)
                {
                    resultXp.text = string.Empty;
                }

                return;
            }

            if (!finishXpGranted)
            {
                var receipt = CareerXp.GrantFinishedMatch();
                finishXpGained = receipt.Gained;
                finishXpTotal = receipt.Total;
                finishXpGranted = true;
            }

            var copy = DescribeMatchEnd(state);
            if (matchEndCard != null)
            {
                SetSeatClass(matchEndCard, copy.Seat);
            }

            if (resultText != null)
            {
                resultText.text = copy.Title;
            }

            if (resultSummary != null)
            {
                resultSummary.text = copy.Summary;
            }

            if (resultXp != null)
            {
                resultXp.text = FormatXpLine(finishXpGained, finishXpTotal);
            }
        }

        static string FormatXpLine(int gained, int total)
        {
            return UiText.Format(UiKey.HudXp, gained, total);
        }

        readonly struct MatchEndCopy
        {
            public MatchEndCopy(string title, string summary, int seat)
            {
                Title = title;
                Summary = summary;
                Seat = seat;
            }

            public string Title { get; }
            public string Summary { get; }
            public int Seat { get; }
        }

        static MatchEndCopy DescribeMatchEnd(MatchState state)
        {
            var winner = state.Winner;
            var seat = winner is PlayerId id ? id.Value : -1;
            var title = winner is PlayerId won
                ? UiText.Format(UiKey.HudWin, won.Value + 1)
                : UiText.Get(UiKey.HudDraw);
            return new MatchEndCopy(title, SummarizeSeats(state), seat);
        }

        static string SummarizeSeats(MatchState state)
        {
            var mode = state.Rules.WinMode;
            var parts = new string[state.Players.Length];
            for (var i = 0; i < state.Players.Length; i++)
            {
                parts[i] = FormatSeat(mode, state.Players[i]);
            }

            return string.Join("  ·  ", parts);
        }

        static string FormatSeat(WinMode mode, PlayerState player)
        {
            var seat = player.Id.Value + 1;
            switch (mode)
            {
                case WinMode.Stock:
                    return UiText.Format(UiKey.HudStockLine, seat, player.Stocks);
                case WinMode.Points:
                    return UiText.Format(UiKey.HudPointsLine, seat, player.Points);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unhandled win mode.");
            }
        }

        void RefreshPills(MatchState state)
        {
            if (playersRow == null)
            {
                return;
            }

            if (pills.Count != state.Players.Length
                || (pills.Count > 0 && pills[0].Hearts.Count != state.Rules.StockCount))
            {
                RebuildPills(state.Players.Length, state.Rules.StockCount);
            }

            var activeSeat = state.Phase == Phase.MatchEnd ? -1 : state.CurrentPlayerId.Value;
            var showScore = state.Rules.WinMode == WinMode.Points;
            for (var i = 0; i < state.Players.Length; i++)
            {
                var player = state.Players[i];
                var pill = pills[i];
                var active = player.Id.Value == activeSeat;
                pill.Root.EnableInClassList(PillActiveClass, active);
                pill.Now?.EnableInClassList(HiddenClass, !active);
                if (pill.Now != null)
                {
                    pill.Now.text = UiText.Get(UiKey.HudNow);
                }

                pill.Root.EnableInClassList(PillOutClass, !player.IsAlive);
                pill.Points.text = UiText.Format(UiKey.HudPoints, player.Points);
                pill.Points.EnableInClassList(HiddenClass, !showScore);
                pill.HeartsRow.EnableInClassList(HiddenClass, showScore);
                for (var h = 0; h < pill.Hearts.Count; h++)
                {
                    pill.Hearts[h].EnableInClassList(HeartLostClass, h >= player.Stocks);
                }
            }
        }

        void RebuildPills(int playerCount, int stockCount)
        {
            playersRow.Clear();
            pills.Clear();
            for (var i = 0; i < playerCount; i++)
            {
                var pill = new PlayerPill { Root = new VisualElement() };
                pill.Root.AddToClassList("player-pill");
                pill.Root.AddToClassList($"seat-{i % SeatClassCount}");

                pill.Now = new Label(UiText.Get(UiKey.HudNow));
                pill.Now.AddToClassList("pill-now");
                pill.Now.AddToClassList(HiddenClass);
                pill.Root.Add(pill.Now);

                var nameLabel = new Label($"P{i + 1}");
                nameLabel.AddToClassList("pill-name");
                pill.Root.Add(nameLabel);

                var heartRow = new VisualElement();
                heartRow.AddToClassList("pill-hearts");
                pill.HeartsRow = heartRow;
                for (var h = 0; h < stockCount; h++)
                {
                    var heart = new VisualElement();
                    heart.AddToClassList("heart");
                    ApplyIcon(heart, iconStock);
                    heartRow.Add(heart);
                    pill.Hearts.Add(heart);
                }

                pill.Root.Add(heartRow);

                pill.Points = new Label(UiText.Format(UiKey.HudPoints, 0));
                pill.Points.AddToClassList("pill-points");
                pill.Root.Add(pill.Points);

                playersRow.Add(pill.Root);
                pills.Add(pill);
            }
        }

        static string PhaseText(Phase phase)
        {
            return phase switch
            {
                Phase.TurnStart => UiText.Get(UiKey.HudPhaseReady),
                Phase.CardSelect => UiText.Get(UiKey.HudPhaseCard),
                Phase.Aim => UiText.Get(UiKey.HudPhaseAim),
                Phase.Simulating => UiText.Get(UiKey.HudPhaseRoll),
                Phase.Resolve => UiText.Get(UiKey.HudPhaseResult),
                Phase.RoundEnd => UiText.Get(UiKey.HudPhaseRoundEnd),
                Phase.MatchEnd => UiText.Get(UiKey.HudPhaseMatchEnd),
                _ => phase.ToString(),
            };
        }

        void ApplyChrome()
        {
            EnsureDocument();
            var tree = document != null ? document.rootVisualElement : root;
            if (tree == null)
            {
                return;
            }

            UiText.Apply(tree, UiText.MatchHudSlots);
        }

        static void ApplyIcon(VisualElement element, Texture2D icon)
        {
            if (element == null || icon == null)
            {
                return;
            }

            UiImageUtil.ApplyFit(element, icon);
            element.AddToClassList("has-icon");
        }

        void ApplySeatPalette()
        {
            if (root == null)
            {
                return;
            }

            root.EnableInClassList(SeatPalette.HudClass, SeatPalette.ColorblindPreferred());
        }

        static void SetSeatClass(VisualElement element, int seat)
        {
            for (var i = 0; i < SeatClassCount; i++)
            {
                element.EnableInClassList($"seat-{i}", seat >= 0 && seat % SeatClassCount == i);
            }
        }

        void EnsurePraiseView()
        {
            if (praiseView != null || root == null)
            {
                return;
            }

            var layer = new VisualElement
            {
                name = "knock-praise",
                pickingMode = PickingMode.Ignore,
            };
            layer.AddToClassList("knock-praise");
            root.Add(layer);
            praiseView = new KnockPraiseView(layer, PraiseAnchor);
        }

        VisualElement PraiseAnchor(int seat)
        {
            if (seat < 0 || seat >= pills.Count)
            {
                return null;
            }

            return pills[seat].Root;
        }

        void HookPraise()
        {
            if (praiseDirector == director)
            {
                return;
            }

            UnhookPraise();
            praiseDirector = director;
            if (praiseDirector != null)
            {
                praiseDirector.EraserStockLost += OnEraserStockLost;
            }
        }

        void UnhookPraise()
        {
            if (praiseDirector == null)
            {
                return;
            }

            praiseDirector.EraserStockLost -= OnEraserStockLost;
            praiseDirector = null;
        }

        void OnEraserStockLost(EraserStockLoss loss)
        {
            if (praiseView != null
                && KnockPraiseBurst.TryPraiseSeat(loss.VictimSeat, loss.KnockerSeat, out var seat))
            {
                praiseView.Play(seat);
            }
        }

        void PaintEdgeCue()
        {
            if (root == null || director == null)
            {
                return;
            }

            var watcher = director.GetComponent<EdgeSlowmoWatcher>();
            var cue = watcher == null
                ? EdgeSlowmoCue.Idle
                : watcher.CurrentCue(Time.unscaledTime, EdgeSlowmoCue.ReduceMotionPreferred());
            EdgeSlowmoVignette.Paint(root, cue);
        }

        void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.EnableInClassList(HiddenClass, !visible);
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
