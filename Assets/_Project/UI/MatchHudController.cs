using System.Collections.Generic;
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
        const int SeatClassCount = 4;

        [SerializeField] MatchDirector director;
        [SerializeField] UIDocument document;
        [SerializeField] StartMenuController startMenu;
        [SerializeField] Texture2D iconStock;
        [SerializeField] Texture2D iconPass;

        readonly List<PlayerPill> pills = new();

        VisualElement root;
        VisualElement playersRow;
        Label phaseLabel;
        Label timerLabel;
        Label turnLabel;
        VisualElement resultBanner;
        Label resultText;

        sealed class PlayerPill
        {
            public VisualElement Root;
            public Label Points;
            public readonly List<VisualElement> Hearts = new();
        }

        void OnEnable()
        {
            if (document == null)
            {
                return;
            }

            if (startMenu == null)
            {
                startMenu = FindAnyObjectByType<StartMenuController>();
            }

            root = document.rootVisualElement.Q("root") ?? document.rootVisualElement;
            playersRow = root.Q("players");
            phaseLabel = root.Q<Label>("phase");
            timerLabel = root.Q<Label>("timer");
            turnLabel = root.Q<Label>("turn");
            resultBanner = root.Q("result-banner");
            resultText = root.Q<Label>("result-text");
            pills.Clear();
            playersRow?.Clear();

            ApplyIcon(root.Q("pass-icon"), iconPass);
            root.Q<Button>("pass")?.RegisterCallback<ClickEvent>(_ => director?.Pass());
            root.Q<Button>("rematch")?.RegisterCallback<ClickEvent>(_ => director?.Rematch());
            root.Q<Button>("menu")?.RegisterCallback<ClickEvent>(_ => startMenu?.ShowMenu());
            SetVisible(false);
        }

        void Update()
        {
            RefreshFromState();
        }

        public void RefreshFromState()
        {
            if (director == null)
            {
                director = FindAnyObjectByType<MatchDirector>();
            }

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

            if (turnLabel != null)
            {
                turnLabel.text = state.Phase == Phase.MatchEnd
                    ? "おわり"
                    : $"ターン P{state.CurrentPlayerId.Value + 1}";
            }

            if (timerLabel != null)
            {
                timerLabel.text = $"のこり {Mathf.CeilToInt(Mathf.Max(0f, state.TurnTimeLeft))}秒";
            }

            RefreshPills(state);
            RefreshResultBanner(state);
        }

        void RefreshResultBanner(MatchState state)
        {
            if (resultBanner == null)
            {
                return;
            }

            var ended = state.Phase == Phase.MatchEnd;
            resultBanner.EnableInClassList("hidden", !ended);
            if (!ended)
            {
                return;
            }

            var winner = state.Winner;
            for (var i = 0; i < SeatClassCount; i++)
            {
                resultBanner.EnableInClassList($"seat-{i}", winner is PlayerId id && id.Value % SeatClassCount == i);
            }

            if (resultText != null)
            {
                resultText.text = winner is PlayerId w
                    ? $"P{w.Value + 1} のかち!"
                    : "ひきわけ!";
            }
        }

        void RefreshPills(MatchState state)
        {
            if (playersRow == null)
            {
                return;
            }

            if (pills.Count != state.Players.Length)
            {
                RebuildPills(state.Players.Length, state.Rules.StockCount);
            }

            var activeSeat = state.Phase == Phase.MatchEnd ? -1 : state.CurrentPlayerId.Value;
            for (var i = 0; i < state.Players.Length; i++)
            {
                var player = state.Players[i];
                var pill = pills[i];
                pill.Root.EnableInClassList(PillActiveClass, player.Id.Value == activeSeat);
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
                // Card play is not wired into hotseat yet; CardSelect is the aiming seat.
                Phase.CardSelect => "ねらえ!",
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

            element.style.backgroundImage = new StyleBackground(icon);
            element.AddToClassList("has-icon");
        }

        void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.EnableInClassList("hidden", !visible);
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
