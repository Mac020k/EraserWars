using System;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Persistence;
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
        Button saveButton;
        VisualElement portrait;
        Label profileName;
        Label archetypeLabel;
        Label statsLabel;
        Label flavorLabel;
        VisualElement signatures;
        Label presetName;
        readonly Button[] presetTabs = new Button[DeckPresets.SlotsPerCharacter];
        readonly Button[] seatTabs = new Button[4];
        readonly CharacterDefinition[] draftSeats = new CharacterDefinition[4];
        readonly List<CardDefinition>[] draftCommons =
        {
            new(), new(), new(), new(),
        };
        bool draftReady;
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
            saveButton = deckPanel.Q<Button>("deck-save");
            if (saveButton != null)
            {
                saveButton.clicked += () => TrySave();
            }

            portrait = deckPanel.Q("deck-portrait");
            profileName = deckPanel.Q<Label>("deck-profile-name");
            archetypeLabel = deckPanel.Q<Label>("deck-archetype");
            statsLabel = deckPanel.Q<Label>("deck-stats");
            flavorLabel = deckPanel.Q<Label>("deck-flavor");
            signatures = deckPanel.Q("deck-signatures");
            presetName = deckPanel.Q<Label>("deck-preset-name");
            for (var i = 0; i < presetTabs.Length; i++)
            {
                var slot = i;
                presetTabs[i] = deckPanel.Q<Button>($"deck-preset-{i}");
                if (presetTabs[i] != null)
                {
                    presetTabs[i].clicked += () => SelectPreset(slot);
                }
            }

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
            CaptureSaved();
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
            if (seat < 0 || seat >= playerCount || seat == activeSeat)
            {
                return;
            }

            CommitIfLegal();
            activeSeat = seat;
            ReplaceDraft(activeSeat, DraftSource.SeatCommons);
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
                var selected = draftSeats[activeSeat] == character;
                button.EnableInClassList("selected", selected);
                var lockMark = CharacterAccess.IsUnlocked(character) ? "" : "🔒 ";
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
            if (draftSeats[activeSeat] == character)
            {
                RebuildCharacterList();
                RefreshStatus();
                return;
            }

            CommitIfLegal();
            draftSeats[activeSeat] = character;
            ReplaceDraft(activeSeat, DraftSource.Starter);
            CommitIfLegal();
            RebuildCharacterList();
            RebuildCardList();
            RefreshStatus();
        }

        void SelectPreset(int slot)
        {
            var character = draftSeats[activeSeat];
            if (character == null || slot < 0 || slot >= DeckPresets.SlotsPerCharacter)
            {
                return;
            }

            if (slot == DeckPresets.GetActiveSlot(character.IdValue))
            {
                return;
            }

            CommitIfLegal();
            DeckPresets.SetActiveSlot(character.IdValue, slot);
            ReplaceDraft(activeSeat, DraftSource.Empty);
            CommitIfLegal();
            RebuildCardList();
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

            var selected = draftCommons[activeSeat];
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
            var selected = draftCommons[activeSeat];
            var budget = DeckBudget.From(selected, ActiveCap());
            var block = budget.BlockAdd(CountCopies(selected, card.IdValue), card.MaxCopies, card.BuildCost);
            switch (block)
            {
                case AddBlock.None:
                    selected.Add(card);
                    CommitIfLegal();
                    RebuildCardList();
                    RefreshStatus();
                    return;
                case AddBlock.Slots:
                    RefreshStatus($"コモンは{DeckBudget.SlotCap}枚までです。");
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
            var selected = draftCommons[activeSeat];
            for (var i = selected.Count - 1; i >= 0; i--)
            {
                if (selected[i] != null && selected[i].IdValue == card.IdValue)
                {
                    selected.RemoveAt(i);
                    break;
                }
            }

            CommitIfLegal();
            RebuildCardList();
            RefreshStatus();
        }

        public bool TrySave()
        {
            var saved = CommitIfLegal();
            RefreshStatus();
            return saved;
        }

        void RefreshStatus(string reject = null)
        {
            var character = draftSeats[activeSeat];
            var selected = draftCommons[activeSeat];
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
                detail.text = $"固有: {sigA} / {sigB}";
            }

            PaintProfile(character);
            PaintPresets(character);
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

            var verdict = VerdictFor(activeSeat);
            var illegal = verdict.Fault != DeckFault.None;
            var showWarning = illegal || !string.IsNullOrEmpty(reject);
            if (budgetWarning != null)
            {
                budgetWarning.EnableInClassList("hidden", !showWarning);
                if (showWarning)
                {
                    budgetWarning.text = illegal ? verdict.Message : reject;
                }
            }

            saveButton?.SetEnabled(!illegal);
            emptyDeck?.EnableInClassList("hidden", budget.Count > 0);
        }

        void CaptureSaved()
        {
            if (draftReady)
            {
                return;
            }

            draftReady = true;
            for (var i = 0; i < draftSeats.Length; i++)
            {
                draftSeats[i] = loadout.Seats[i];
                ReplaceDraft(i, DraftSource.SeatCommons);
                var fault = VerdictFor(i).Fault;
                switch (fault)
                {
                    case DeckFault.SameName:
                    case DeckFault.Slots:
                    case DeckFault.OverCost:
                        loadout.Commons[i].Clear();
                        break;
                    case DeckFault.None:
                        CopyDraftToSeat(i);
                        break;
                    case DeckFault.NoCharacter:
                        break;
                    default:
                        throw new InvalidOperationException($"Unhandled {nameof(DeckFault)} '{fault}'.");
                }
            }
        }

        bool CommitIfLegal()
        {
            if (!draftReady || VerdictFor(activeSeat).Fault != DeckFault.None)
            {
                return false;
            }

            loadout.Seats[activeSeat] = draftSeats[activeSeat];
            CopyDraftToSeat(activeSeat);
            var character = draftSeats[activeSeat];
            if (character != null)
            {
                DeckPresets.SaveCards(
                    character.IdValue,
                    DeckPresets.GetActiveSlot(character.IdValue),
                    IdsOf(draftCommons[activeSeat]));
            }

            return true;
        }

        void CopyDraftToSeat(int seat)
        {
            var saved = loadout.Commons[seat];
            saved.Clear();
            saved.AddRange(draftCommons[seat]);
        }

        DeckVerdict VerdictFor(int seat)
        {
            var character = draftSeats[seat];
            var eraser = character != null ? character.Eraser : null;
            var cap = eraser != null ? eraser.DeckCostCap : DefaultCap;
            return DeckRules.Evaluate(eraser != null, cap, DeckCardFacts.From(draftCommons[seat]));
        }

        int ActiveCap()
        {
            var character = draftSeats[activeSeat];
            return character?.Eraser?.DeckCostCap ?? DefaultCap;
        }

        enum DraftSource
        {
            SeatCommons,
            Starter,
            Empty,
        }

        void ReplaceDraft(int seat, DraftSource missing)
        {
            draftCommons[seat].Clear();
            var character = draftSeats[seat];
            if (character == null)
            {
                return;
            }

            var slot = DeckPresets.GetActiveSlot(character.IdValue);
            if (DeckPresets.HasSaved(character.IdValue, slot))
            {
                draftCommons[seat].AddRange(
                    HotseatLoadout.ResolveCommons(catalog, DeckPresets.LoadCards(character.IdValue, slot)));
                return;
            }

            switch (missing)
            {
                case DraftSource.SeatCommons:
                    if (loadout.Commons[seat] != null)
                    {
                        draftCommons[seat].AddRange(loadout.Commons[seat]);
                    }

                    return;
                case DraftSource.Starter:
                    HotseatLoadout.SeedStarter(draftCommons[seat], catalog);
                    return;
                case DraftSource.Empty:
                    return;
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(DraftSource)} '{missing}'.");
            }
        }

        void PaintProfile(CharacterDefinition character)
        {
            if (portrait != null)
            {
                if (character?.FaceSprite != null)
                {
                    UiImageUtil.ApplyFaceCrop(portrait, character);
                }
                else
                {
                    UiImageUtil.Clear(portrait);
                }
            }

            if (profileName != null)
            {
                profileName.text = character != null ? character.DisplayName : "未選択";
            }

            if (archetypeLabel != null)
            {
                archetypeLabel.text = character != null
                    ? CharacterArchetypes.DisplayJa(character.ArchetypeKey)
                    : string.Empty;
            }

            if (statsLabel != null)
            {
                statsLabel.text = StatsLine(character);
            }

            if (flavorLabel != null)
            {
                flavorLabel.text = FlavorLine(character);
            }

            PaintSignatures(character);
        }

        void PaintSignatures(CharacterDefinition character)
        {
            if (signatures == null)
            {
                return;
            }

            signatures.Clear();
            if (character == null)
            {
                return;
            }

            AddSignatureFace(character.SignatureA);
            AddSignatureFace(character.SignatureB);
        }

        void AddSignatureFace(CardDefinition card)
        {
            if (card == null || signatures == null)
            {
                return;
            }

            var face = new VisualElement();
            CardFaceBuilder.Paint(face, card, CardSurface.Collection);
            signatures.Add(face);
        }

        void PaintPresets(CharacterDefinition character)
        {
            var slot = character != null ? DeckPresets.GetActiveSlot(character.IdValue) : 0;
            for (var i = 0; i < presetTabs.Length; i++)
            {
                presetTabs[i]?.EnableInClassList("selected", character != null && i == slot);
                presetTabs[i]?.SetEnabled(character != null);
            }

            if (presetName != null)
            {
                presetName.text = character != null ? DeckPresets.GetName(character.IdValue, slot) : string.Empty;
            }
        }

        static string StatsLine(CharacterDefinition character)
        {
            var eraser = character != null ? character.Eraser : null;
            if (eraser == null)
            {
                return "消しゴム未設定";
            }

            return $"質量 {CollectionDisplay.DisplayedMass(eraser.Mass)} / コスト上限 {eraser.DeckCostCap}";
        }

        static string FlavorLine(CharacterDefinition character)
        {
            if (character == null)
            {
                return string.Empty;
            }

            var subjects = CharacterFlavor.TrySubjects(character.IdValue, out var strong, out var weak)
                ? $"得意 {strong} / 苦手 {weak}"
                : null;
            var hasBio = !string.IsNullOrWhiteSpace(character.BioJa);
            if (subjects != null && hasBio)
            {
                return subjects + "\n" + character.Bio;
            }

            if (subjects != null)
            {
                return subjects;
            }

            return character.Bio;
        }

        static int[] IdsOf(List<CardDefinition> cards)
        {
            if (cards == null || cards.Count == 0)
            {
                return Array.Empty<int>();
            }

            var ids = new List<int>(cards.Count);
            foreach (var card in cards)
            {
                if (card != null)
                {
                    ids.Add(card.IdValue);
                }
            }

            return ids.ToArray();
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
            public const int SlotCap = DeckRules.SlotCap;

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
