using System;
using System.Collections.Generic;
using EraserWars.Cards;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class DeckPanelController
    {
        const int DefaultCap = 8;

        readonly ContentCatalog catalog;
        readonly HotseatLoadout loadout;
        ScrollView characterList;
        ScrollView cardList;
        Label status;
        Label detail;
        Label budgetLabel;
        Label budgetRemain;
        VisualElement budgetFill;
        Label budgetWarning;
        Label emptyDeck;
        readonly Button[] seatTabs = new Button[4];
        int activeSeat;
        int playerCount = 2;

        public DeckPanelController(ContentCatalog catalog, HotseatLoadout loadout)
        {
            this.catalog = catalog;
            this.loadout = loadout;
        }

        public void Bind(VisualElement deckPanel)
        {
            if (deckPanel == null)
            {
                return;
            }

            characterList = deckPanel.Q<ScrollView>("deck-character-list");
            cardList = deckPanel.Q<ScrollView>("deck-card-list");
            status = deckPanel.Q<Label>("deck-status");
            detail = deckPanel.Q<Label>("deck-detail");
            budgetLabel = deckPanel.Q<Label>("deck-budget-label");
            budgetRemain = deckPanel.Q<Label>("deck-budget-remain");
            budgetFill = deckPanel.Q("deck-budget-fill");
            budgetWarning = deckPanel.Q<Label>("deck-budget-warning");
            emptyDeck = deckPanel.Q<Label>("deck-empty");
            for (var i = 0; i < seatTabs.Length; i++)
            {
                var seat = i;
                seatTabs[i] = deckPanel.Q<Button>($"deck-seat-{i}");
                seatTabs[i]?.RegisterCallback<ClickEvent>(_ => SetSeat(seat));
            }
        }

        public void Refresh(int matchPlayerCount)
        {
            playerCount = matchPlayerCount < 2 ? 2 : matchPlayerCount;
            loadout.EnsureDefaults(catalog);
            if (activeSeat >= playerCount)
            {
                activeSeat = 0;
            }

            for (var i = 0; i < seatTabs.Length; i++)
            {
                seatTabs[i]?.EnableInClassList("hidden", i >= playerCount);
                seatTabs[i]?.EnableInClassList("selected", i == activeSeat);
            }

            RebuildCharacterList();
            RebuildCardList();
            RefreshStatus();
        }

        void SetSeat(int seat)
        {
            if (seat < 0 || seat >= playerCount)
            {
                return;
            }

            activeSeat = seat;
            for (var i = 0; i < seatTabs.Length; i++)
            {
                seatTabs[i]?.EnableInClassList("selected", i == activeSeat);
            }

            RebuildCharacterList();
            RebuildCardList();
            RefreshStatus();
        }

        void RebuildCharacterList()
        {
            if (characterList == null)
            {
                return;
            }

            characterList.Clear();
            if (catalog?.Characters == null || catalog.Characters.Count == 0)
            {
                characterList.Add(EmptyCopy("キャラがいません。"));
                return;
            }

            var shown = 0;
            foreach (var character in catalog.Characters)
            {
                if (character == null)
                {
                    continue;
                }

                shown++;
                var button = new Button();
                button.AddToClassList("deck-item");
                var selected = loadout.Seats[activeSeat] == character;
                button.EnableInClassList("selected", selected);
                var lockMark = character.UnlockedByDefault ? "" : "🔒 ";
                button.text =
                    $"{lockMark}{character.DisplayName}  ({CharacterArchetypes.DisplayJa(character.ArchetypeKey)})";
                var captured = character;
                button.clicked += () => SelectCharacter(captured);
                characterList.Add(button);
            }

            if (shown == 0)
            {
                characterList.Add(EmptyCopy("キャラがいません。"));
            }
        }

        void SelectCharacter(CharacterDefinition character)
        {
            loadout.Seats[activeSeat] = character;
            RebuildCharacterList();
            RefreshStatus();
        }

        void RebuildCardList()
        {
            if (cardList == null)
            {
                return;
            }

            cardList.Clear();
            if (catalog?.Cards == null)
            {
                cardList.Add(EmptyCopy("追加できるコモンカードがありません。"));
                return;
            }

            var selected = loadout.Commons[activeSeat];
            var shown = 0;
            foreach (var card in catalog.Cards)
            {
                if (card == null || card.IsSignature)
                {
                    continue;
                }

                shown++;
                cardList.Add(BuildCardRow(card, CountCopies(selected, card.IdValue)));
            }

            if (shown == 0)
            {
                cardList.Add(EmptyCopy("追加できるコモンカードがありません。"));
            }
        }

        VisualElement BuildCardRow(CardDefinition card, int count)
        {
            var row = new VisualElement();
            row.AddToClassList("deck-row");
            row.EnableInClassList("deck-row-in", count > 0);

            var face = new VisualElement();
            CardFaceBuilder.Paint(face, card, CardSurface.Collection);

            var copies = new Label(count > 0 ? $"{count}枚" : "");
            copies.AddToClassList("deck-row-count");
            copies.EnableInClassList("hidden", count <= 0);

            var remove = new Button { text = "外す" };
            remove.AddToClassList("deck-action");
            remove.AddToClassList("deck-action-remove");
            remove.EnableInClassList("hidden", count <= 0);
            var add = new Button { text = "追加" };
            add.AddToClassList("deck-action");
            add.AddToClassList("deck-action-add");

            var captured = card;
            remove.clicked += () => RemoveOne(captured);
            add.clicked += () => AddOne(captured);

            var actions = new VisualElement();
            actions.AddToClassList("deck-row-actions");
            actions.Add(copies);
            actions.Add(remove);
            actions.Add(add);

            row.Add(face);
            row.Add(actions);
            return row;
        }

        void AddOne(CardDefinition card)
        {
            var selected = loadout.Commons[activeSeat];
            var budget = DeckBudget.From(selected, ActiveCap());
            var block = budget.BlockAdd(CountCopies(selected, card.IdValue), card.MaxCopies, card.BuildCost);
            switch (block)
            {
                case AddBlock.None:
                    selected.Add(card);
                    RebuildCardList();
                    RefreshStatus();
                    return;
                case AddBlock.Slots:
                    RefreshStatus("コモンは6枚までです。");
                    return;
                case AddBlock.Copies:
                    RefreshStatus($"{card.DisplayName} は{card.MaxCopies}枚までです。");
                    return;
                case AddBlock.Cost:
                    RefreshStatus(
                        $"{card.DisplayName} を足すと {budget.Used + card.BuildCost}/{budget.Cap} になります。");
                    return;
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(AddBlock)} '{block}'.");
            }
        }

        void RemoveOne(CardDefinition card)
        {
            var selected = loadout.Commons[activeSeat];
            for (var i = selected.Count - 1; i >= 0; i--)
            {
                if (selected[i] != null && selected[i].IdValue == card.IdValue)
                {
                    selected.RemoveAt(i);
                    break;
                }
            }

            RebuildCardList();
            RefreshStatus();
        }

        void RefreshStatus(string reject = null)
        {
            var character = loadout.Seats[activeSeat];
            var selected = loadout.Commons[activeSeat];
            var budget = DeckBudget.From(selected, ActiveCap());
            var name = character != null ? character.DisplayName : "未選択";
            var sigA = character?.SignatureA != null ? character.SignatureA.DisplayName : "なし";
            var sigB = character?.SignatureB != null ? character.SignatureB.DisplayName : "なし";
            PaintBudget(budget, reject);

            if (status != null)
            {
                status.text = $"P{activeSeat + 1}: {name}  /  コモン {budget.Count}/{DeckBudget.SlotCap}";
            }

            if (detail != null)
            {
                detail.text =
                    $"固有: {sigA} / {sigB}\n型: {CharacterArchetypes.DisplayJa(character?.ArchetypeKey)}";
            }
        }

        void PaintBudget(DeckBudget budget, string reject)
        {
            if (budgetLabel != null)
            {
                budgetLabel.text = $"デッキコスト {budget.Used}/{budget.Cap}";
            }

            if (budgetRemain != null)
            {
                budgetRemain.text = budget.OverCap
                    ? "上限オーバー"
                    : budget.AtCap
                        ? "上限ぴったり"
                        : $"残り {budget.Remaining}";
                budgetRemain.EnableInClassList("over", budget.OverCap);
                budgetRemain.EnableInClassList("full", budget.AtCap);
            }

            if (budgetFill != null)
            {
                budgetFill.style.width = Length.Percent(budget.FillPercent);
                budgetFill.EnableInClassList("over", budget.OverCap);
                budgetFill.EnableInClassList("full", budget.AtCap);
            }

            var showWarning = budget.OverCap || !string.IsNullOrEmpty(reject);
            if (budgetWarning != null)
            {
                budgetWarning.EnableInClassList("hidden", !showWarning);
                if (showWarning)
                {
                    budgetWarning.text = budget.OverCap
                        ? "上限を超えています。カードを外してください。"
                        : reject;
                }
            }

            emptyDeck?.EnableInClassList("hidden", budget.Count > 0);
        }

        int ActiveCap()
        {
            var character = loadout.Seats[activeSeat];
            return character?.Eraser?.DeckCostCap ?? DefaultCap;
        }

        static Label EmptyCopy(string text)
        {
            var label = new Label(text);
            label.AddToClassList("deck-empty");
            return label;
        }

        static int CountCopies(List<CardDefinition> cards, int id)
        {
            var n = 0;
            if (cards == null)
            {
                return n;
            }

            foreach (var card in cards)
            {
                if (card != null && card.IdValue == id)
                {
                    n++;
                }
            }

            return n;
        }

        enum AddBlock
        {
            None,
            Slots,
            Copies,
            Cost,
        }

        readonly struct DeckBudget
        {
            public const int SlotCap = 6;

            public readonly int Used;
            public readonly int Cap;
            public readonly int Count;

            DeckBudget(int used, int cap, int count)
            {
                Used = used;
                Cap = cap;
                Count = count;
            }

            public bool OverCap => Used > Cap;

            public bool AtCap => Used == Cap;

            public int Remaining => Cap > Used ? Cap - Used : 0;

            public float FillPercent
            {
                get
                {
                    if (Cap <= 0)
                    {
                        return Used > 0 ? 100f : 0f;
                    }

                    var raw = 100f * Used / Cap;
                    if (raw < 0f)
                    {
                        return 0f;
                    }

                    return raw > 100f ? 100f : raw;
                }
            }

            public static DeckBudget From(List<CardDefinition> cards, int cap)
            {
                var used = 0;
                var count = 0;
                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        if (card == null)
                        {
                            continue;
                        }

                        used += card.BuildCost;
                        count++;
                    }
                }

                return new DeckBudget(used, cap, count);
            }

            public AddBlock BlockAdd(int copies, int maxCopies, int buildCost)
            {
                if (Count >= SlotCap)
                {
                    return AddBlock.Slots;
                }

                if (copies >= maxCopies)
                {
                    return AddBlock.Copies;
                }

                if (Used + buildCost > Cap)
                {
                    return AddBlock.Cost;
                }

                return AddBlock.None;
            }
        }
    }
}
