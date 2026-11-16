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
        VisualElement strip;
        Label detailTitle;
        Label detailMeta;
        Label detailBody;
        Label detailUnlock;
        VisualElement portrait;
        Button tabCharacters;
        Button tabCards;
        string mode = "characters";
        string filter = "all";
        int selectedCharacterId = -1;

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

            strip = panel.Q("collection-strip");
            detailTitle = panel.Q<Label>("collection-detail-title");
            detailMeta = panel.Q<Label>("collection-detail-meta");
            detailBody = panel.Q<Label>("collection-detail-body");
            detailUnlock = panel.Q<Label>("collection-detail-unlock");
            portrait = panel.Q("collection-portrait");
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

        public void SelectCharacterById(int id)
        {
            if (catalog == null)
            {
                return;
            }

            var character = catalog.FindCharacter(id);
            if (character == null)
            {
                return;
            }

            mode = "characters";
            SetMode("characters");
            ShowCharacter(character);
        }

        void SetMode(string next)
        {
            mode = next;
            tabCharacters?.EnableInClassList("selected", mode == "characters");
            tabCards?.EnableInClassList("selected", mode == "cards");
            filter = "all";
            selectedCharacterId = -1;
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
            if (strip == null)
            {
                return;
            }

            strip.Clear();
            if (catalog == null)
            {
                var missing = new Label("カタログ未読み込み");
                missing.AddToClassList("collection-empty");
                strip.Add(missing);
                return;
            }

            if (mode == "characters")
            {
                RebuildCharacterStrip();
            }
            else
            {
                RebuildCardStrip();
            }
        }

        void RebuildCharacterStrip()
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

                var unlocked = character.UnlockedByDefault;
                var tile = new Button();
                tile.AddToClassList("char-tile");
                tile.EnableInClassList("locked", !unlocked);
                tile.EnableInClassList("selected", character.IdValue == selectedCharacterId);

                var art = new VisualElement();
                art.AddToClassList("char-tile-art");
                if (character.Portrait != null)
                {
                    art.style.backgroundImage = new StyleBackground(character.Portrait.texture);
                }

                var name = new Label(unlocked ? character.DisplayName : character.MaskedDisplayName);
                name.AddToClassList("char-tile-name");
                var archetype = new Label(unlocked
                    ? CharacterArchetypes.DisplayJa(character.ArchetypeKey)
                    : "？？？型");
                archetype.AddToClassList("char-tile-arch");

                tile.Add(art);
                tile.Add(name);
                tile.Add(archetype);
                var captured = character;
                tile.clicked += () => ShowCharacter(captured);
                strip.Add(tile);
                shown++;
            }

            if (shown == 0)
            {
                AddEmptyLabel(strip);
            }
        }

        void RebuildCardStrip()
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

                var tile = new Button();
                tile.AddToClassList("card-tile");
                tile.EnableInClassList("signature", card.IsSignature);
                var title = new Label(card.DisplayName);
                title.AddToClassList("card-tile-name");
                var cost = new Label($"コスト {card.BuildCost}");
                cost.AddToClassList("card-tile-cost");
                tile.Add(title);
                tile.Add(cost);
                var captured = card;
                tile.clicked += () => ShowCard(captured);
                strip.Add(tile);
                shown++;
            }

            if (shown == 0)
            {
                AddEmptyLabel(strip);
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
            selectedCharacterId = character.IdValue;
            RebuildCharacterStrip();
            if (detailTitle == null)
            {
                return;
            }

            var unlocked = character.UnlockedByDefault;
            detailTitle.text = unlocked ? character.DisplayName : character.MaskedDisplayName;
            var archetype = unlocked
                ? CharacterArchetypes.DisplayJa(character.ArchetypeKey)
                : "？？？型";
            var eraser = character.Eraser;
            if (unlocked && eraser != null)
            {
                detailMeta.text =
                    $"{archetype}\n質量 {eraser.Mass:0.###} / 摩擦 {eraser.Friction:0.##} / パワー {eraser.Power:0.##} / スピード {eraser.Speed:0.##}";
                var sigA = character.SignatureA != null ? character.SignatureA.DisplayName : "—";
                var sigB = character.SignatureB != null ? character.SignatureB.DisplayName : "—";
                detailBody.text =
                    $"{character.Bio}\n\n固有A: {sigA}\n固有B: {sigB}\nパッシブ: {eraser.PassiveKey}\nデッキコスト上限: {eraser.DeckCostCap} / 手札: {eraser.HandLimit}";
                if (detailUnlock != null)
                {
                    detailUnlock.text = "解放済み";
                }
            }
            else
            {
                detailMeta.text = $"{archetype}\n詳細はロック中";
                detailBody.text = "？？？？？？？？\n自己紹介は解放後に読めます。";
                if (detailUnlock != null)
                {
                    detailUnlock.text = $"解放条件: {character.UnlockHint}";
                }
            }

            if (portrait != null)
            {
                portrait.EnableInClassList("locked", !unlocked);
                if (unlocked && character.Portrait != null)
                {
                    portrait.style.backgroundImage = new StyleBackground(character.Portrait.texture);
                }
                else
                {
                    portrait.style.backgroundImage = StyleKeyword.None;
                }
            }
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
            if (detailUnlock != null)
            {
                detailUnlock.text = card.IsSignature ? "キャラ固有" : "コモンプール";
            }

            portrait?.EnableInClassList("locked", false);
            if (portrait != null)
            {
                portrait.style.backgroundImage = StyleKeyword.None;
            }
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

            if (detailUnlock != null)
            {
                detailUnlock.text = string.Empty;
            }
        }
    }
}
