using System.Collections.Generic;
using EraserWars.Cards;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class DeckPanelController
    {
        readonly ContentCatalog catalog;
        readonly HotseatLoadout loadout;
        ScrollView characterList;
        ScrollView cardList;
        Label status;
        Label detail;
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
            if (catalog?.Characters == null)
            {
                return;
            }

            foreach (var character in catalog.Characters)
            {
                if (character == null)
                {
                    continue;
                }

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
                return;
            }

            var selected = loadout.Commons[activeSeat];
            foreach (var card in catalog.Cards)
            {
                if (card == null || card.IsSignature)
                {
                    continue;
                }

                var button = new Button();
                button.AddToClassList("deck-item");
                var count = CountCopies(selected, card.IdValue);
                button.EnableInClassList("selected", count > 0);
                button.text = count > 0
                    ? $"[{count}] {card.DisplayName}  cost{card.BuildCost}"
                    : $"{card.DisplayName}  cost{card.BuildCost}";
                var captured = card;
                button.clicked += () => ToggleCard(captured);
                cardList.Add(button);
            }
        }

        void ToggleCard(CardDefinition card)
        {
            var selected = loadout.Commons[activeSeat];
            var count = CountCopies(selected, card.IdValue);
            if (count > 0)
            {
                for (var i = selected.Count - 1; i >= 0; i--)
                {
                    if (selected[i] != null && selected[i].IdValue == card.IdValue)
                    {
                        selected.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                if (selected.Count >= 6)
                {
                    RefreshStatus("コモンは6枚まで");
                    return;
                }

                if (count >= card.MaxCopies)
                {
                    RefreshStatus($"枚数上限 {card.DisplayName}");
                    return;
                }

                var character = loadout.Seats[activeSeat];
                var cap = character?.Eraser?.DeckCostCap ?? 8;
                var nextCost = TotalCost(selected) + card.BuildCost;
                if (nextCost > cap)
                {
                    RefreshStatus($"コスト超過 ({nextCost}/{cap})");
                    return;
                }

                selected.Add(card);
            }

            RebuildCardList();
            RefreshStatus();
        }

        void RefreshStatus(string extra = null)
        {
            var character = loadout.Seats[activeSeat];
            var selected = loadout.Commons[activeSeat];
            var cost = TotalCost(selected);
            var cap = character?.Eraser?.DeckCostCap ?? 8;
            var name = character != null ? character.DisplayName : "未選択";
            var sigA = character?.SignatureA != null ? character.SignatureA.DisplayName : "—";
            var sigB = character?.SignatureB != null ? character.SignatureB.DisplayName : "—";
            if (status != null)
            {
                status.text = $"P{activeSeat + 1}: {name}  /  コモン {selected.Count}/6  コスト {cost}/{cap}";
                if (!string.IsNullOrEmpty(extra))
                {
                    status.text += $"  ·  {extra}";
                }
            }

            if (detail != null)
            {
                detail.text = $"固有: {sigA} / {sigB}\n型: {CharacterArchetypes.DisplayJa(character?.ArchetypeKey)}";
            }
        }

        static int CountCopies(List<CardDefinition> cards, int id)
        {
            var n = 0;
            foreach (var card in cards)
            {
                if (card != null && card.IdValue == id)
                {
                    n++;
                }
            }

            return n;
        }

        static int TotalCost(List<CardDefinition> cards)
        {
            var cost = 0;
            foreach (var card in cards)
            {
                if (card != null)
                {
                    cost += card.BuildCost;
                }
            }

            return cost;
        }
    }
}
