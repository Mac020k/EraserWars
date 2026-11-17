using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Persistence;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public enum CollectionPreviewMode
    {
        Still,
        Eraser3D,
        EraserDesign,
    }

    public sealed class CollectionPanelController
    {
        readonly ContentCatalog catalog;
        VisualElement panel;
        VisualElement strip;
        Label detailTitle;
        Label detailMeta;
        Label detailBody;
        Label detailUnlock;
        Label passiveLabel;
        VisualElement subjects;
        VisualElement signatures;
        VisualElement archRow;
        VisualElement stage;
        VisualElement portrait;
        VisualElement preview3d;
        VisualElement previewDesign;
        Label slot3d;
        Label slotDesign;
        Button modeStill;
        Button mode3d;
        Button modeDesign;
        Button tabCharacters;
        Button tabCards;
        VisualElement cardSlots;
        Label grantCaption;
        Label playCaption;
        public const string UnlockFlashClass = "unlock-flash";
        const long UnlockFlashMs = 650;
        const string PreviewWiredClass = "preview-wired";
        readonly HashSet<int> visitFlashes = new();
        string mode = "characters";
        string filter = "all";
        int selectedCharacterId = -1;
        int selectedCardId = -1;
        CollectionPreviewMode previewMode = CollectionPreviewMode.Still;

        public CollectionPanelController(ContentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public CollectionPreviewMode PreviewMode => previewMode;

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
            tabCharacters = panel.Q<Button>("collection-tab-characters");
            tabCards = panel.Q<Button>("collection-tab-cards");
            EnsureDetailChrome();
            EnsurePreviewChrome();

            tabCharacters?.RegisterCallback<ClickEvent>(_ => SetMode("characters"));
            tabCards?.RegisterCallback<ClickEvent>(_ => SetMode("cards"));
            WirePreviewModes();
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

        public void ShowCards()
        {
            SetMode("cards");
        }

        public void SetPreviewMode(CollectionPreviewMode next)
        {
            previewMode = next;
            ApplyPreviewVisibility();
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
            filter = "all";
            selectedCharacterId = character.IdValue;
            tabCharacters?.EnableInClassList("selected", true);
            tabCards?.EnableInClassList("selected", false);
            SetPreviewMode(CollectionPreviewMode.Still);
            RebuildFilters();
            RebuildList();
            PaintCharacterDetail(character);
        }

        void EnsureDetailChrome()
        {
            var detail = panel.Q(className: "collection-detail") ?? detailBody?.parent ?? panel;
            archRow = EnsureElement(detail, "collection-arch-row", "collection-arch-row");
            subjects = EnsureElement(detail, "collection-detail-subjects", "collection-subjects");
            passiveLabel = EnsureLabel(detail, "collection-passive", "collection-passive");
            signatures = EnsureElement(detail, "collection-signatures", "collection-signatures");
        }

        void EnsurePreviewChrome()
        {
            portrait = panel.Q("collection-portrait");
            stage = panel.Q("collection-stage");
            if (stage == null && portrait != null && portrait.parent != null)
            {
                stage = new VisualElement { name = "collection-stage" };
                stage.AddToClassList("collection-stage");
                var parent = portrait.parent;
                var index = parent.IndexOf(portrait);
                portrait.RemoveFromHierarchy();
                stage.Add(portrait);
                parent.Insert(index, stage);
            }

            var preview = panel.Q("collection-preview");
            if (preview == null && stage != null && stage.parent != null)
            {
                preview = new VisualElement { name = "collection-preview" };
                preview.AddToClassList("collection-preview");
                var parent = stage.parent;
                var index = parent.IndexOf(stage);
                stage.RemoveFromHierarchy();
                preview.Add(stage);
                parent.Insert(index, preview);
            }

            preview3d = EnsureElement(stage, "collection-eraser-3d", "collection-preview-panel", "hidden");
            previewDesign = EnsureElement(stage, "collection-eraser-design", "collection-preview-panel", "hidden");
            EnsureLabel(preview3d, "collection-eraser-3d-kicker", "collection-preview-kicker", "3D消しゴム");
            EnsureLabel(previewDesign, "collection-eraser-design-kicker", "collection-preview-kicker", "消しゴムデザイン");
            slot3d = EnsureLabel(preview3d, "collection-eraser-3d-slot", "collection-slot-caption", "モデル枠");
            slotDesign = EnsureLabel(previewDesign, "collection-eraser-design-slot", "collection-slot-caption", "デザイン枠");

            var modes = EnsureElement(preview ?? stage?.parent, "collection-preview-modes", "collection-preview-modes");
            modeStill = EnsureButton(modes, "collection-mode-still", "collection-preview-mode", "立ち絵");
            mode3d = EnsureButton(modes, "collection-mode-3d", "collection-preview-mode", "3D");
            modeDesign = EnsureButton(modes, "collection-mode-design", "collection-preview-mode", "デザイン");
            EnsureCardSlots(preview ?? stage?.parent);
            previewMode = ReadPreviewMode();
            ApplyPreviewVisibility();
        }

        void EnsureCardSlots(VisualElement preview)
        {
            cardSlots = EnsureElement(preview, "collection-card-slots", "collection-card-slots", "hidden");
            var grant = EnsureElement(cardSlots, "collection-grant-slot", "collection-card-slot");
            EnsureLabel(grant, "collection-grant-kicker", "collection-preview-kicker", "付与演出");
            grantCaption = EnsureLabel(grant, "collection-grant-caption", "collection-slot-caption", "準備中");
            var play = EnsureElement(cardSlots, "collection-play-slot", "collection-card-slot");
            EnsureLabel(play, "collection-play-kicker", "collection-preview-kicker", "プレイ演出");
            playCaption = EnsureLabel(play, "collection-play-caption", "collection-slot-caption", "準備中");
        }

        void WirePreviewModes()
        {
            WirePreviewButton(modeStill, CollectionPreviewMode.Still);
            WirePreviewButton(mode3d, CollectionPreviewMode.Eraser3D);
            WirePreviewButton(modeDesign, CollectionPreviewMode.EraserDesign);
        }

        void WirePreviewButton(Button button, CollectionPreviewMode next)
        {
            if (button == null || button.ClassListContains(PreviewWiredClass))
            {
                return;
            }

            button.AddToClassList(PreviewWiredClass);
            button.clicked += () => SetPreviewMode(next);
        }

        CollectionPreviewMode ReadPreviewMode()
        {
            if (preview3d != null && !preview3d.ClassListContains("hidden"))
            {
                return CollectionPreviewMode.Eraser3D;
            }

            if (previewDesign != null && !previewDesign.ClassListContains("hidden"))
            {
                return CollectionPreviewMode.EraserDesign;
            }

            return CollectionPreviewMode.Still;
        }

        void ApplyPreviewVisibility()
        {
            preview3d?.EnableInClassList("hidden", previewMode != CollectionPreviewMode.Eraser3D);
            previewDesign?.EnableInClassList("hidden", previewMode != CollectionPreviewMode.EraserDesign);
            modeStill?.EnableInClassList("selected", previewMode == CollectionPreviewMode.Still);
            mode3d?.EnableInClassList("selected", previewMode == CollectionPreviewMode.Eraser3D);
            modeDesign?.EnableInClassList("selected", previewMode == CollectionPreviewMode.EraserDesign);
        }

        static VisualElement EnsureElement(VisualElement parent, string name, params string[] classes)
        {
            var found = FindNamed<VisualElement>(parent, name);
            if (found != null)
            {
                return found;
            }

            if (parent == null)
            {
                return null;
            }

            var created = new VisualElement { name = name };
            for (var i = 0; i < classes.Length; i++)
            {
                created.AddToClassList(classes[i]);
            }

            parent.Add(created);
            return created;
        }

        static Label EnsureLabel(VisualElement parent, string name, string className, string text = "")
        {
            var existing = FindNamed<Label>(parent, name);
            if (existing != null)
            {
                return existing;
            }

            if (parent == null)
            {
                return null;
            }

            var label = new Label(text) { name = name };
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        static Button EnsureButton(VisualElement parent, string name, string className, string text)
        {
            var existing = FindNamed<Button>(parent, name);
            if (existing != null)
            {
                return existing;
            }

            if (parent == null)
            {
                return null;
            }

            var button = new Button { name = name, text = text };
            button.AddToClassList(className);
            parent.Add(button);
            return button;
        }

        static T FindNamed<T>(VisualElement origin, string name) where T : VisualElement
        {
            if (origin == null)
            {
                return null;
            }

            var root = origin;
            while (root.parent != null)
            {
                root = root.parent;
            }

            return root.Q<T>(name);
        }

        void SetMode(string next)
        {
            mode = next;
            tabCharacters?.EnableInClassList("selected", mode == "characters");
            tabCards?.EnableInClassList("selected", mode == "cards");
            filter = "all";
            selectedCharacterId = -1;
            selectedCardId = -1;
            SetPreviewMode(CollectionPreviewMode.Still);
            ApplyCollectionChrome();
            if (mode == "cards")
            {
                CollectionDisplay.FitCardFrame(stage);
            }
            else
            {
                CollectionDisplay.FitStillFrame(stage, CollectionDisplay.StillLockedSize, CollectionDisplay.StillLockedSize);
            }

            RebuildFilters();
            RebuildList();
            ClearDetail(mode == "characters" ? "キャラを選んでください" : "カードを選んでください");
            if (mode == "cards")
            {
                var first = FirstListedCard();
                if (first != null)
                {
                    ShowCard(first);
                }
            }
        }

        void ApplyCollectionChrome()
        {
            var cards = mode == "cards";
            cardSlots?.EnableInClassList("hidden", !cards);
            panel?.Q("collection-preview-modes")?.EnableInClassList("hidden", cards);
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
            var button = new Button { text = string.Empty };
            button.AddToClassList("collection-filter");
            button.AddToClassList(FilterMotifClass(key));
            button.EnableInClassList("selected", filter == key);
            if (mode == "characters" && key != "all")
            {
                var icon = ArchetypeMark.Create(key);
                icon.AddToClassList("collection-filter-icon");
                button.Add(icon);
            }

            if (mode == "cards" && key != "all")
            {
                button.AddToClassList("filter-" + key);
            }

            var caption = new Label(label) { pickingMode = PickingMode.Ignore };
            caption.AddToClassList("collection-filter-label");
            button.Add(caption);
            var captured = key;
            button.clicked += () => ApplyFilter(captured);
            row.Add(button);
        }

        void ApplyFilter(string key)
        {
            filter = key;
            RebuildFilters();
            RebuildList();
            if (mode != "cards")
            {
                return;
            }

            if (IsCardListed(selectedCardId))
            {
                return;
            }

            var first = FirstListedCard();
            if (first != null)
            {
                ShowCard(first);
                return;
            }

            selectedCardId = -1;
            ClearCardHero();
        }

        static string FilterMotifClass(string key)
        {
            switch (key)
            {
                case "all":
                    return "filter-all";
                case CharacterArchetypes.AllOut:
                    return "filter-all-out";
                case CharacterArchetypes.SeatTake:
                    return "filter-seat-take";
                case CharacterArchetypes.Teacher:
                    return "filter-teacher";
                case CharacterArchetypes.Afterschool:
                    return "filter-afterschool";
                case CharacterArchetypes.DeskKing:
                    return "filter-desk-king";
                case CardFamilies.Kiai:
                    return "filter-kiai";
                case CardFamilies.Homework:
                    return "filter-homework";
                case CardFamilies.Seat:
                    return "filter-seat";
                case CardFamilies.Stationery:
                    return "filter-stationery";
                case CardFamilies.Cover:
                    return "filter-cover";
                case CardFamilies.Signature:
                    return "filter-signature";
                default:
                    return "filter-all";
            }
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
            strip.Clear();
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

                var unlocked = IsUnlocked(character);
                var flash = BeginFaceFlash(character.IdValue);
                var tile = new Button();
                tile.AddToClassList("char-tile");
                tile.EnableInClassList("locked", !unlocked);
                tile.EnableInClassList(UnlockFlashClass, flash);
                tile.EnableInClassList("selected", character.IdValue == selectedCharacterId);

                var art = new VisualElement();
                art.AddToClassList("char-tile-art");
                PaintCircle(art, character, unlocked);

                var name = new Label(unlocked ? character.DisplayName : character.MaskedDisplayName);
                name.AddToClassList("char-tile-name");
                var place = new Label(unlocked ? CharacterFlavor.TileCaption(character.IdValue) : "？？？");
                place.AddToClassList("char-tile-place");

                tile.Add(art);
                if (unlocked)
                {
                    var badge = ArchetypeMark.Create(character.ArchetypeKey);
                    badge.AddToClassList("char-tile-badge");
                    tile.Add(badge);
                }

                tile.Add(name);
                tile.Add(place);
                tile.userData = character.IdValue;
                var captured = character;
                tile.clicked += () => ShowCharacter(captured);
                strip.Add(tile);
                if (flash)
                {
                    ScheduleFlashEnd(tile, character.IdValue);
                }

                shown++;
            }

            if (shown == 0)
            {
                AddEmptyLabel(strip);
            }
        }

        static void PaintCircle(VisualElement art, CharacterDefinition character, bool unlocked)
        {
            art.Clear();
            if (!unlocked)
            {
                UiImageUtil.Clear(art);
                art.Add(LockMark.Create());
                return;
            }

            if (character.Portrait != null)
            {
                UiImageUtil.ApplyFaceCrop(art, character.Portrait);
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

                var tile = new Button { text = string.Empty };
                CardFaceBuilder.Paint(tile, card, CardSurface.Collection);
                tile.EnableInClassList("selected", card.IdValue == selectedCardId);
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
            previewMode = ReadPreviewMode();
            SyncStripSelection();
            PaintCharacterDetail(character);
        }

        void SyncStripSelection()
        {
            if (strip == null)
            {
                return;
            }

            foreach (var child in strip.Children())
            {
                if (child is not Button tile || !tile.ClassListContains("char-tile"))
                {
                    continue;
                }

                var id = tile.userData is int value ? value : -1;
                tile.EnableInClassList("selected", id == selectedCharacterId);
            }
        }

        void PaintCharacterDetail(CharacterDefinition character)
        {
            if (detailTitle == null)
            {
                return;
            }

            var unlocked = IsUnlocked(character);
            detailTitle.text = unlocked ? character.DisplayName : character.MaskedDisplayName;
            PaintArchRow(character.ArchetypeKey, unlocked);
            var eraser = character.Eraser;
            if (!unlocked)
            {
                if (detailMeta != null)
                {
                    detailMeta.text = "詳細はロック中";
                }

                if (detailBody != null)
                {
                    detailBody.text = "？？？？？？？？\n自己紹介は解放後に読めます。";
                }

                if (detailUnlock != null)
                {
                    detailUnlock.text = $"解放条件: {character.UnlockHint}";
                }
            }
            else
            {
                if (detailMeta != null)
                {
                    detailMeta.text = eraser != null
                        ? $"質量 {CollectionDisplay.DisplayedMass(eraser.Mass)} / 摩擦 {eraser.Friction:0.##} / パワー {eraser.Power:0.##} / スピード {eraser.Speed:0.##}"
                        : "質量 —";
                }

                if (detailBody != null)
                {
                    detailBody.text = character.Bio;
                }

                if (detailUnlock != null)
                {
                    detailUnlock.text = "解放済み";
                }
            }

            PaintSubjects(character.IdValue, unlocked);
            PaintPassive(eraser != null ? eraser.PassiveKey : null, unlocked);
            PaintSignatures(character, unlocked);
            PaintStubs(character, unlocked);
            PaintStill(character, unlocked);
            ApplyPreviewVisibility();
        }

        void PaintArchRow(string archetypeKey, bool unlocked)
        {
            if (archRow == null)
            {
                return;
            }

            archRow.Clear();
            if (unlocked)
            {
                archRow.Add(ArchetypeMark.Create(archetypeKey));
            }

            var label = new Label(unlocked ? CharacterArchetypes.DisplayJa(archetypeKey) : "？？？型");
            label.AddToClassList("collection-arch-label");
            archRow.Add(label);
        }

        void PaintSubjects(int characterId, bool unlocked)
        {
            if (subjects == null)
            {
                return;
            }

            subjects.Clear();
            if (!unlocked)
            {
                return;
            }

            CharacterFlavor.TrySubjects(characterId, out var strong, out var weak);
            if (string.IsNullOrEmpty(strong))
            {
                strong = "—";
            }

            if (string.IsNullOrEmpty(weak))
            {
                weak = "—";
            }

            var strongLabel = new Label("得意 " + strong);
            strongLabel.AddToClassList("collection-subject-strong");
            var weakLabel = new Label("苦手 " + weak);
            weakLabel.AddToClassList("collection-subject-weak");
            subjects.Add(strongLabel);
            subjects.Add(weakLabel);
        }

        void PaintPassive(string passiveKey, bool unlocked)
        {
            if (passiveLabel == null)
            {
                return;
            }

            if (!unlocked)
            {
                passiveLabel.text = string.Empty;
                return;
            }

            PassiveText.Describe(passiveKey, out var name, out var effect);
            passiveLabel.text = string.IsNullOrEmpty(effect) ? name : name + "\n" + effect;
        }

        void PaintSignatures(CharacterDefinition character, bool unlocked)
        {
            if (signatures == null)
            {
                return;
            }

            signatures.Clear();
            if (!unlocked)
            {
                return;
            }

            AddSignature(character.SignatureA);
            AddSignature(character.SignatureB);
        }

        void AddSignature(CardDefinition card)
        {
            if (card == null || signatures == null)
            {
                return;
            }

            var column = new VisualElement();
            column.AddToClassList("collection-signature");
            var face = new VisualElement();
            CardFaceBuilder.Paint(face, card, CardSurface.Collection);
            var effect = new Label(CardEffectText(card));
            effect.AddToClassList("collection-signature-effect");
            column.Add(face);
            column.Add(effect);
            signatures.Add(column);
        }

        static string CardEffectText(CardDefinition card)
        {
            if (card.EffectKinds == null || card.EffectKinds.Count == 0)
            {
                return "効果なし";
            }

            var lines = new List<string>();
            foreach (var kind in card.EffectKinds)
            {
                lines.Add(EffectSpecParser.SummarizeJa(kind));
            }

            return string.Join("\n", lines);
        }

        void PaintStubs(CharacterDefinition character, bool unlocked)
        {
            var caption = unlocked ? character.DisplayName : "解放後に見られます";
            if (slot3d != null)
            {
                slot3d.text = "モデル枠\n" + caption;
            }

            if (slotDesign != null)
            {
                slotDesign.text = "デザイン枠\n" + caption;
            }
        }

        void PaintStill(CharacterDefinition character, bool unlocked)
        {
            if (portrait == null)
            {
                return;
            }

            portrait.Clear();
            portrait.EnableInClassList("showing-card", false);
            portrait.EnableInClassList("locked", !unlocked);
            stage?.EnableInClassList("locked", !unlocked);
            if (!unlocked || character.Portrait == null)
            {
                UiImageUtil.Clear(portrait);
                if (!unlocked)
                {
                    portrait.Add(LockMark.Create());
                }

                CollectionDisplay.FitStillFrame(stage, CollectionDisplay.StillLockedSize, CollectionDisplay.StillLockedSize);
                return;
            }

            UiImageUtil.ApplyFit(portrait, character.Portrait);
            var rect = character.Portrait.rect;
            CollectionDisplay.FitStillFrame(stage, rect.width, rect.height);
        }

        void ShowCard(CardDefinition card)
        {
            if (card == null)
            {
                return;
            }

            selectedCardId = card.IdValue;
            SetPreviewMode(CollectionPreviewMode.Still);
            PaintHero(card);
            if (grantCaption != null)
            {
                grantCaption.text = card.DisplayName;
            }

            if (playCaption != null)
            {
                playCaption.text = card.DisplayName;
            }

            RebuildList();
            if (detailTitle == null)
            {
                return;
            }

            detailTitle.text = card.DisplayName;
            archRow?.Clear();
            subjects?.Clear();
            signatures?.Clear();
            if (passiveLabel != null)
            {
                passiveLabel.text = string.Empty;
            }

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
        }

        void PaintHero(CardDefinition card)
        {
            if (portrait == null)
            {
                return;
            }

            portrait.Clear();
            UiImageUtil.Clear(portrait);
            portrait.EnableInClassList("locked", false);
            portrait.EnableInClassList("showing-card", true);
            stage?.EnableInClassList("locked", false);
            var hero = new VisualElement { name = "collection-hero-card", pickingMode = PickingMode.Ignore };
            CardFaceBuilder.Paint(hero, card, CardSurface.Showcase);
            portrait.Add(hero);
            CollectionDisplay.FitCardFrame(stage);
        }

        void ClearCardHero()
        {
            portrait?.Clear();
            portrait?.EnableInClassList("showing-card", false);
            portrait?.EnableInClassList("locked", false);
            UiImageUtil.Clear(portrait);
            if (mode == "cards")
            {
                CollectionDisplay.FitCardFrame(stage);
            }

            if (grantCaption != null)
            {
                grantCaption.text = "準備中";
            }

            if (playCaption != null)
            {
                playCaption.text = "準備中";
            }
        }

        CardDefinition FirstListedCard()
        {
            var cards = catalog?.Cards;
            if (cards == null)
            {
                return null;
            }

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null)
                {
                    continue;
                }

                if (filter != "all" && card.FamilyKey != filter)
                {
                    continue;
                }

                return card;
            }

            return null;
        }

        bool IsCardListed(int id)
        {
            var cards = catalog?.Cards;
            if (cards == null)
            {
                return false;
            }

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null || card.IdValue != id)
                {
                    continue;
                }

                return filter == "all" || card.FamilyKey == filter;
            }

            return false;
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

            if (passiveLabel != null)
            {
                passiveLabel.text = string.Empty;
            }

            archRow?.Clear();
            subjects?.Clear();
            signatures?.Clear();
            portrait?.Clear();
            portrait?.EnableInClassList("showing-card", false);
            portrait?.EnableInClassList("locked", false);
            UiImageUtil.Clear(portrait);
            if (grantCaption != null)
            {
                grantCaption.text = "準備中";
            }

            if (playCaption != null)
            {
                playCaption.text = "準備中";
            }

            if (slot3d != null)
            {
                slot3d.text = "モデル枠";
            }

            if (slotDesign != null)
            {
                slotDesign.text = "デザイン枠";
            }
        }

        bool IsUnlocked(CharacterDefinition character)
        {
            return CharacterAccess.IsUnlocked(character);
        }

        bool BeginFaceFlash(int characterId)
        {
            if (visitFlashes.Contains(characterId))
            {
                return true;
            }

            if (!CharacterUnlocks.ConsumeFlash(characterId))
            {
                return false;
            }

            visitFlashes.Add(characterId);
            return true;
        }

        void ScheduleFlashEnd(VisualElement tile, int characterId)
        {
            if (tile.panel == null)
            {
                return;
            }

            tile.schedule.Execute(() =>
            {
                tile.RemoveFromClassList(UnlockFlashClass);
                if (tile.parent == null)
                {
                    return;
                }

                visitFlashes.Remove(characterId);
            }).ExecuteLater(UnlockFlashMs);
        }
    }
}
