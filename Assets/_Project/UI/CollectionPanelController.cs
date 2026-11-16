using System.Collections.Generic;
using EraserWars.Cards;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class CollectionPanelController
    {
        readonly ContentCatalog catalog;
        VisualElement panel;
        ScrollView list;
        Label detailTitle;
        Label detailMeta;
        Label detailBody;
        Button tabCharacters;
        Button tabCards;
        string mode = "characters";
        string filter = "all";

        public CollectionPanelController(ContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public void Bind(VisualElement collectionPanel)
        {
            panel = collectionPanel;
            if (panel == null)
            {
                return;
            }

            list = panel.Q<ScrollView>("collection-list");
            detailTitle = panel.Q<Label>("collection-detail-title");
            detailMeta = panel.Q<Label>("collection-detail-meta");
            detailBody = panel.Q<Label>("collection-detail-body");
            tabCharacters = panel.Q<Button>("collection-tab-characters");
            tabCards = panel.Q<Button>("collection-tab-cards");
            var filterRow = panel.Q("collection-filters");

            tabCharacters?.RegisterCallback<ClickEvent>(_ => SetMode("characters"));
            tabCards?.RegisterCallback<ClickEvent>(_ => SetMode("cards"));
            WireFilters(filterRow);
            SetMode(mode);
        }

        public void Refresh()
        {
            if (panel == null)
            {
                return;
            }

            RebuildFilters();
            RebuildList();
        }

        void SetMode(string next)
        {
            mode = next;
            tabCharacters?.EnableInClassList("selected", mode == "characters");
            tabCards?.EnableInClassList("selected", mode == "cards");
            filter = "all";
            RebuildFilters();
            RebuildList();
            ClearDetail(mode == "characters" ? "キャラを選んでください" : "カードを選んでください");
        }

        void WireFilters(VisualElement filterRow)
        {
            if (filterRow == null)
            {
                return;
            }

            filterRow.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is not Button button)
                {
                    return;
                }

                var key = button.userData as string;
                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                filter = key;
                RebuildFilters();
                RebuildList();
            });
        }

        void RebuildFilters()
        {
            var filterRow = panel?.Q("collection-filters");
            if (filterRow == null)
            {
                return;
            }

            filterRow.Clear();
            AddFilterChip(filterRow, "all", "すべて");
            if (mode == "characters")
            {
                AddFilterChip(filterRow, CharacterArchetypes.AllOut, "全力");
                AddFilterChip(filterRow, CharacterArchetypes.SeatTake, "席取り");
                AddFilterChip(filterRow, CharacterArchetypes.Teacher, "先生役");
                AddFilterChip(filterRow, CharacterArchetypes.Afterschool, "放課後");
                AddFilterChip(filterRow, CharacterArchetypes.DeskKing, "机の王");
            }
            else
            {
                AddFilterChip(filterRow, CardFamilies.Kiai, "気合");
                AddFilterChip(filterRow, CardFamilies.Homework, "宿題");
                AddFilterChip(filterRow, CardFamilies.Seat, "席替え");
                AddFilterChip(filterRow, CardFamilies.Stationery, "文房具");
                AddFilterChip(filterRow, CardFamilies.Cover, "カバー");
                AddFilterChip(filterRow, CardFamilies.Signature, "必殺");
            }
        }

        void AddFilterChip(VisualElement row, string key, string label)
        {
            var button = new Button { text = label, userData = key };
            button.AddToClassList("collection-filter");
            button.EnableInClassList("selected", filter == key);
            row.Add(button);
        }

        void RebuildList()
        {
            if (list == null)
            {
                return;
            }

            list.Clear();
            if (catalog == null)
            {
                var missing = new Label("カタログ未読み込み");
                missing.AddToClassList("collection-empty");
                list.Add(missing);
                return;
            }

            if (mode == "characters")
            {
                RebuildCharacterList();
            }
            else
            {
                RebuildCardList();
            }
        }

        void RebuildCharacterList()
        {
            var characters = catalog.Characters ?? new List<CharacterDefinition>();
            var shown = 0;
            foreach (var character in characters)
            {
                if (character == null)
                {
                    continue;
                }

                if (filter != "all" && character.ArchetypeKey != filter)
                {
                    continue;
                }

                var button = new Button();
                button.AddToClassList("collection-item");
                var lockMark = character.UnlockedByDefault ? "" : "🔒 ";
                button.text = $"{lockMark}{character.IdValue:00}  {character.DisplayName}";
                var captured = character;
                button.clicked += () => ShowCharacter(captured);
                list.Add(button);
                shown++;
            }

            if (shown == 0)
            {
                AddEmptyLabel(list);
            }
        }

        void RebuildCardList()
        {
            var cards = catalog.Cards ?? new List<CardDefinition>();
            var shown = 0;
            foreach (var card in cards)
            {
                if (card == null)
                {
                    continue;
                }

                if (filter != "all" && card.FamilyKey != filter)
                {
                    continue;
                }

                var button = new Button();
                button.AddToClassList("collection-item");
                var sig = card.IsSignature ? "★ " : "";
                button.text = $"{sig}{card.IdValue:00}  {card.DisplayName}";
                var captured = card;
                button.clicked += () => ShowCard(captured);
                list.Add(button);
                shown++;
            }

            if (shown == 0)
            {
                AddEmptyLabel(list);
            }
        }

        static void AddEmptyLabel(VisualElement parent)
        {
            var empty = new Label("該当なし");
            empty.AddToClassList("collection-empty");
            parent.Add(empty);
        }

        void ShowCharacter(CharacterDefinition character)
        {
            if (detailTitle == null)
            {
                return;
            }

            detailTitle.text = character.DisplayName;
            var archetype = CharacterArchetypes.DisplayJa(character.ArchetypeKey);
            var unlock = character.UnlockedByDefault ? "初期解放" : "アンロック";
            var eraser = character.Eraser;
            var eraserLine = eraser == null
                ? "消しゴム未設定"
                : $"質量 {eraser.Mass:0.###} / 摩擦 {eraser.Friction:0.##} / パワー {eraser.Power:0.##} / スピード {eraser.Speed:0.##}";
            detailMeta.text = $"{archetype} · {unlock}\n{eraserLine}";
            var sigA = character.SignatureA != null ? character.SignatureA.DisplayName : "—";
            var sigB = character.SignatureB != null ? character.SignatureB.DisplayName : "—";
            var passive = eraser != null ? eraser.PassiveKey : "—";
            detailBody.text =
                $"固有A: {sigA}\n固有B: {sigB}\nパッシブ: {passive}\nデッキ上限: {eraser?.DeckCostCap ?? 0} / 手札: {eraser?.HandLimit ?? 0}";
        }

        void ShowCard(CardDefinition card)
        {
            if (detailTitle == null)
            {
                return;
            }

            detailTitle.text = card.DisplayName;
            var family = CardFamilies.DisplayJa(card.FamilyKey);
            var sig = card.IsSignature ? "必殺" : "コモン";
            detailMeta.text = $"{family} · {sig} · コスト {card.BuildCost} · 枚数上限 {card.MaxCopies}";
            var effects = new List<string>();
            foreach (var kind in card.EffectKinds)
            {
                effects.Add(EffectSpecParser.SummarizeJa(kind));
            }

            detailBody.text = effects.Count == 0
                ? "効果なし"
                : string.Join("\n", effects) +
                  $"\n試合内 {card.UsesPerMatch}回 / CD {card.CooldownTurns} / 持続 {card.DurationTurns}T";
        }

        void ClearDetail(string title)
        {
            if (detailTitle != null)
            {
                detailTitle.text = title;
            }

            if (detailMeta != null)
            {
                detailMeta.text = string.Empty;
            }

            if (detailBody != null)
            {
                detailBody.text = string.Empty;
            }
        }
    }
}
