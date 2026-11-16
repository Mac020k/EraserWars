using System;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
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
        VisualElement timerBox;
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
        string handFingerprint = string.Empty;
        Button boundPass;

        sealed class PlayerPill
        {
            public VisualElement Root;
            public Label Points;
            public Label Now;
            public readonly List<VisualElement> Hearts = new();
        }

        void OnEnable()
        {
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

        void BindTree()
        {
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
            timerBox = root.Q("timer-box");
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

            if (passButton == null || ReferenceEquals(passButton, boundPass))
            {
                return;
            }

            boundPass = passButton;
            pills.Clear();
            playersRow?.Clear();
            handRow?.Clear();
            handFingerprint = string.Empty;
            ApplyIcon(root.Q("pass-icon"), iconPass);
            EnsurePraiseView();
            HookPraise();
            passButton?.RegisterCallback<ClickEvent>(_ => director?.Pass());
            rematchButton?.RegisterCallback<ClickEvent>(_ => director?.Rematch());
            matchEndMenuButton?.RegisterCallback<ClickEvent>(_ => startMenu?.ShowMenu());
            root.Q<Button>("menu")?.RegisterCallback<ClickEvent>(_ => startMenu?.ShowMenu());
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
            var state = director.State;
            if (phaseLabel != null)
            {
                phaseLabel.text = $"ROUND {state.Round}  {PhaseText(state.Phase)}";
            }

            RefreshTurnChrome(state);
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
        }

        void RefreshActions(MatchState state)
        {
            var ended = state.Phase == Phase.MatchEnd;
            var cutInUp = director != null && director.IsCardCutInShowing;
            passButton?.EnableInClassList(HiddenClass, ended || cutInUp);
            passButton?.SetEnabled(state.Phase is Phase.CardSelect or Phase.Aim or Phase.TurnStart);
        }

        void RefreshHand(MatchState state)
        {
            if (handDock == null || handRow == null)
            {
                return;
            }

            var canPlay = state.Phase is Phase.CardSelect or Phase.Aim;
            var cutInUp = director != null && director.IsCardCutInShowing;
            handDock.EnableInClassList(HiddenClass, !canPlay || cutInUp);
            if (!canPlay)
            {
                return;
            }

            SetSeatClass(handDock, state.CurrentPlayerId.Value);
            if (handTitle != null)
            {
                handTitle.text = $"P{state.CurrentPlayerId.Value + 1} のカード";
            }

            var player = state.Players[state.CurrentPlayerId.Value];
            var fingerprint = $"{state.CurrentPlayerId.Value}:{state.Phase}:{player.Hand.Length}:";
            for (var i = 0; i < player.Hand.Length; i++)
            {
                fingerprint += player.Hand[i].Value + ",";
            }

            if (fingerprint == handFingerprint && handRow.childCount == player.Hand.Length)
            {
                return;
            }

            handFingerprint = fingerprint;
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
                button.clicked += () => director?.PlayCard(captured);
                handRow.Add(button);
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
                    ? $"P{cut.Seat + 1}  とくぎ"
                    : $"P{cut.Seat + 1}  カード";
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
                return;
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
                ? $"P{won.Value + 1} のかち!"
                : "ひきわけ!";
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
            var seat = $"P{player.Id.Value + 1}";
            switch (mode)
            {
                case WinMode.Stock:
                    return $"{seat} ストック {player.Stocks}";
                case WinMode.Points:
                    return $"{seat} {player.Points}点";
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
            for (var i = 0; i < state.Players.Length; i++)
            {
                var player = state.Players[i];
                var pill = pills[i];
                var active = player.Id.Value == activeSeat;
                pill.Root.EnableInClassList(PillActiveClass, active);
                pill.Now?.EnableInClassList(HiddenClass, !active);
                pill.Root.EnableInClassList(PillOutClass, !player.IsAlive);
                pill.Points.text = $"{player.Points}点";
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

                pill.Now = new Label("いま");
                pill.Now.AddToClassList("pill-now");
                pill.Now.AddToClassList(HiddenClass);
                pill.Root.Add(pill.Now);

                var nameLabel = new Label($"P{i + 1}");
                nameLabel.AddToClassList("pill-name");
                pill.Root.Add(nameLabel);

                var heartRow = new VisualElement();
                heartRow.AddToClassList("pill-hearts");
                for (var h = 0; h < stockCount; h++)
                {
                    var heart = new VisualElement();
                    heart.AddToClassList("heart");
                    ApplyIcon(heart, iconStock);
                    heartRow.Add(heart);
                    pill.Hearts.Add(heart);
                }

                pill.Root.Add(heartRow);

                pill.Points = new Label("0点");
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
                Phase.TurnStart => "じゅんびちゅう",
                Phase.CardSelect => "カード / ねらえ!",
                Phase.Aim => "ねらえ!",
                Phase.Simulating => "ころがり中...",
                Phase.Resolve => "けっか",
                Phase.RoundEnd => "ラウンド終了",
                Phase.MatchEnd => "しょうぶあり!",
                _ => phase.ToString(),
            };
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
