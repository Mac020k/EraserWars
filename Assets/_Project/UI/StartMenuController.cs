using System;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    /// <summary>
    /// Full-screen pre-match menu (ADR-0009 / 6B). High-energy CTA, icon-led modes.
    /// </summary>
    public sealed class StartMenuController : MonoBehaviour
    {
        [SerializeField] MatchDirector director;
        [SerializeField] UIDocument document;
        [SerializeField] ContentCatalog contentCatalog;
        [SerializeField] Texture2D brandIcon;
        [SerializeField] Texture2D icon2P;
        [SerializeField] Texture2D icon4P;
        [SerializeField] Texture2D iconDeck;
        [SerializeField] Texture2D iconCollection;
        [SerializeField] Texture2D iconOptions;

        VisualElement root;
        VisualElement boundPanelRoot;
        VisualElement menuContent;
        VisualElement homeStack;
        VisualElement optionsPanel;
        VisualElement deckPanel;
        VisualElement collectionPanel;
        VisualElement secretRoomPanel;
        CollectionPanelController collection;
        DeckPanelController deck;
        SeatPickController seatPick;
        readonly HotseatLoadout loadout = new();
        Button mode2P;
        Button mode4P;
        Button modeStock;
        Button modePoints;
        int selectedPlayers = 2;
        bool matchRunning;
        bool suspendedMatch;
        MenuScreen currentScreen = MenuScreen.Home;
        bool destinationFrameBound;
        // Dev default ON; hide the options control before release.
        bool debugModeEnabled = true;
        Button debugModeButton;
        Slider masterSlider;
        Slider bgmSlider;
        Slider seSlider;
        Slider flickSlider;
        Slider turnSlider;
        Label masterValue;
        Label bgmValue;
        Label seValue;
        Label flickValue;
        Label turnValue;
        Button shakeButton;
        Button reduceMotionButton;
        Button seatPaletteButton;
        Button resetButton;
        bool pushingOptions;
        bool resetArmed;
        bool optionsWired;

        enum MenuScreen
        {
            Home,
            Collection,
            Options,
            Deck,
            SecretRoom,
        }

        public bool IsMenuVisible =>
            isActiveAndEnabled
            && (document == null || document.enabled)
            && root != null
            && root.resolvedStyle.display != DisplayStyle.None;

        void OnEnable()
        {
            UiLocale.Load();
            BindDocument(force: true);
            if (document == null || document.rootVisualElement == null)
            {
                GameSettings.Load();
                GameSettings.ApplyVolume();
            }

            if (root == null)
            {
                return;
            }

            SelectPlayers(selectedPlayers);
            PaintWinMode();
            if (matchRunning && !suspendedMatch)
            {
                HideMenu();
            }
            else
            {
                ShowMenu(suspendedMatch);
            }
        }

        void BindDocument(bool force = false)
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null)
            {
                return;
            }

            document.sortingOrder = 100;

            var panelRoot = document.rootVisualElement;
            if (panelRoot == null)
            {
                return;
            }

            var treeChanged = force || boundPanelRoot != panelRoot;
            if (boundPanelRoot != panelRoot)
            {
                optionsWired = false;
            }

            if (!treeChanged && root != null)
            {
                RefreshQueries(panelRoot);
                PaintCopy();
                return;
            }

            boundPanelRoot = panelRoot;
            RefreshQueries(panelRoot);
            RegisterMenuCallbacks();
            SecretRoomPanel.Bind(root);
            ApplyMenuIcons();
            EnsureCatalog();
            CharacterAccess.SetDebugUnlockAll(debugModeEnabled);
            if (!debugModeEnabled)
            {
                loadout.ReleaseLockedSeats(contentCatalog);
            }

            EnsureCollectionBound();
            EnsureDeckBound();
            EnsureSeatPickBound();
            LoadOptions();
            PaintCopy();
        }

        void PaintCopy()
        {
            if (root == null)
            {
                return;
            }

            UiText.Apply(root, UiText.StartMenuSlots);
            SecretRoomPanel.RefreshStatus(root);
            RefreshDebugModeButton();
            RefreshOptionLabels();
            RefreshStartButtonLabel();
        }

        void RefreshQueries(VisualElement panelRoot)
        {
            root = panelRoot.Q("root") ?? panelRoot;
            menuContent = root.Q("menu-content");
            homeStack = root.Q("home-stack");
            optionsPanel = root.Q("options-panel");
            deckPanel = root.Q("deck-panel");
            collectionPanel = root.Q("collection-panel");
            secretRoomPanel = root.Q(SecretRoomPanel.PanelName);
            mode2P = root.Q<Button>("mode-2p");
            mode4P = root.Q<Button>("mode-4p");
            modeStock = root.Q<Button>("mode-stock");
            modePoints = root.Q<Button>("mode-points");
            debugModeButton = root.Q<Button>("opt-debug-mode");
            masterSlider = root.Q<Slider>("opt-master");
            bgmSlider = root.Q<Slider>("opt-bgm");
            seSlider = root.Q<Slider>("opt-se");
            flickSlider = root.Q<Slider>("opt-flick");
            turnSlider = root.Q<Slider>("opt-turn");
            masterValue = root.Q<Label>("opt-master-value");
            bgmValue = root.Q<Label>("opt-bgm-value");
            seValue = root.Q<Label>("opt-se-value");
            flickValue = root.Q<Label>("opt-flick-value");
            turnValue = root.Q<Label>("opt-turn-value");
            shakeButton = root.Q<Button>("opt-shake");
            reduceMotionButton = root.Q<Button>("opt-reduce-motion");
            seatPaletteButton = root.Q<Button>("opt-seat-palette");
            resetButton = root.Q<Button>("opt-reset");
        }

        void RegisterMenuCallbacks()
        {
            root.Q<Button>("start-match")?.RegisterCallback<ClickEvent>(_ => StartMatch());
            root.Q<Button>("resume-match")?.RegisterCallback<ClickEvent>(_ => ResumeMatch());
            mode2P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(2));
            mode4P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(4));
            modeStock?.RegisterCallback<ClickEvent>(_ => SelectWinMode(WinMode.Stock));
            modePoints?.RegisterCallback<ClickEvent>(_ => SelectWinMode(WinMode.Points));
            root.Q<Button>("open-options")?.RegisterCallback<ClickEvent>(_ => OpenOptions());
            root.Q<Button>("open-deck")?.RegisterCallback<ClickEvent>(_ => OpenDeck());
            root.Q<Button>("open-collection")?.RegisterCallback<ClickEvent>(_ => OpenCollection());
            root.Q<Button>(SecretRoomPanel.OpenName)?.RegisterCallback<ClickEvent>(_ => OpenSecretRoom());
            root.Q<Button>(SecretRoomPanel.CloseName)?.RegisterCallback<ClickEvent>(_ => GoHome());
            root.Q<Button>("close-options")?.RegisterCallback<ClickEvent>(_ => CloseOptions());
            root.Q<Button>("close-deck")?.RegisterCallback<ClickEvent>(_ => GoHome());
            root.Q<Button>("close-collection")?.RegisterCallback<ClickEvent>(_ => GoHome());
            debugModeButton?.RegisterCallback<ClickEvent>(_ => ToggleDebugMode());
            if (!optionsWired && masterSlider != null)
            {
                optionsWired = true;
                masterSlider?.RegisterValueChangedCallback(OnMasterChanged);
                bgmSlider?.RegisterValueChangedCallback(OnBgmChanged);
                seSlider?.RegisterValueChangedCallback(OnSeChanged);
                flickSlider?.RegisterValueChangedCallback(OnFlickChanged);
                turnSlider?.RegisterValueChangedCallback(OnTurnChanged);
                turnSlider?.RegisterCallback<PointerUpEvent>(_ => SnapTurnSlider(), TrickleDown.TrickleDown);
                shakeButton?.RegisterCallback<ClickEvent>(_ => ToggleCameraShake());
                reduceMotionButton?.RegisterCallback<ClickEvent>(_ => ToggleReduceMotion());
                seatPaletteButton?.RegisterCallback<ClickEvent>(_ => ToggleSeatPalette());
                resetButton?.RegisterCallback<ClickEvent>(_ => ConfirmReset());
            }

            RefreshDebugModeButton();
        }

        void ApplyMenuIcons()
        {
            UiImageUtil.ApplyFit(root.Q("brand-icon"), brandIcon);
            UiImageUtil.ApplyFit(root.Q("icon-deck"), iconDeck);
            UiImageUtil.ApplyFit(root.Q("icon-collection"), iconCollection);
            UiImageUtil.ApplyFit(root.Q("icon-options"), iconOptions);
        }

        void OnDisable()
        {
            CharacterAccess.SetDebugUnlockAll(false);
        }

        void ToggleDebugMode()
        {
            debugModeEnabled = !debugModeEnabled;
            CharacterAccess.SetDebugUnlockAll(debugModeEnabled);
            if (!debugModeEnabled)
            {
                loadout.ReleaseLockedSeats(contentCatalog);
            }

            RefreshDebugModeButton();
            collection?.Refresh();
            seatPick?.Refresh(selectedPlayers);
            if (matchRunning && director != null)
            {
                director.SetDebugChromeVisible(debugModeEnabled);
            }

            Debug.Log($"[EraserWars] debugModeEnabled={debugModeEnabled}");
        }

        void RefreshDebugModeButton()
        {
            if (debugModeButton == null)
            {
                return;
            }

            debugModeButton.text = debugModeEnabled
                ? UiText.Get(UiKey.OptDebugOn)
                : UiText.Get(UiKey.OptDebugOff);
            debugModeButton.EnableInClassList("debug-on", debugModeEnabled);
        }

        void LoadOptions()
        {
            var settings = GameSettings.Load();
            PushOptionsToWidgets();
            ApplyOptionsLive(scaleChanged: true);
            Debug.Log(
                $"[EraserWars] options applied master={settings.Master:0.00} bgm={settings.Bgm:0.00} " +
                $"se={settings.Se:0.00} flick={settings.FlickScale:0.00} turn={settings.TurnSeconds} " +
                $"shake={settings.CameraShake} reduce={settings.ReduceMotion} seats={settings.ColorblindSeats} " +
                $"ui={settings.UiScale:0.00} listener={AudioListener.volume:0.00}");
        }

        void ApplyOptionsLive(bool scaleChanged)
        {
            GameSettings.ApplyVolume();
            if (scaleChanged)
            {
                GameSettings.ApplyUiScale(document);
            }
        }

        void PushOptionsToWidgets()
        {
            var settings = GameSettings.Current;
            pushingOptions = true;
            masterSlider?.SetValueWithoutNotify(settings.Master * 100f);
            bgmSlider?.SetValueWithoutNotify(settings.Bgm * 100f);
            seSlider?.SetValueWithoutNotify(settings.Se * 100f);
            flickSlider?.SetValueWithoutNotify(settings.FlickScale * 100f);
            turnSlider?.SetValueWithoutNotify(settings.TurnSeconds);
            pushingOptions = false;
            RefreshOptionLabels();
        }

        void RefreshOptionLabels()
        {
            var settings = GameSettings.Current;
            SetOptionText(masterValue, UiText.Format(UiKey.OptMaster, Mathf.RoundToInt(settings.Master * 100f)));
            SetOptionText(bgmValue, UiText.Format(UiKey.OptBgm, Mathf.RoundToInt(settings.Bgm * 100f)));
            SetOptionText(seValue, UiText.Format(UiKey.OptSe, Mathf.RoundToInt(settings.Se * 100f)));
            SetOptionText(flickValue, UiText.Format(UiKey.OptFlick, Mathf.RoundToInt(settings.FlickScale * 100f)));
            SetOptionText(turnValue, UiText.Format(UiKey.OptTurn, settings.TurnSeconds));
            if (shakeButton != null)
            {
                shakeButton.text = settings.CameraShake
                    ? UiText.Get(UiKey.OptShakeOn)
                    : UiText.Get(UiKey.OptShakeOff);
                shakeButton.EnableInClassList("on", settings.CameraShake);
            }

            if (reduceMotionButton != null)
            {
                reduceMotionButton.text = settings.ReduceMotion
                    ? UiText.Get(UiKey.OptReduceOn)
                    : UiText.Get(UiKey.OptReduceOff);
                reduceMotionButton.EnableInClassList("on", settings.ReduceMotion);
            }

            if (seatPaletteButton != null)
            {
                seatPaletteButton.text = settings.ColorblindSeats
                    ? UiText.Get(UiKey.OptPaletteOn)
                    : UiText.Get(UiKey.OptPaletteOff);
                seatPaletteButton.EnableInClassList("on", settings.ColorblindSeats);
            }

            PaintSeatSwatches(settings.ColorblindSeats);

            if (resetButton != null)
            {
                resetButton.text = resetArmed
                    ? UiText.Get(UiKey.OptResetConfirm)
                    : UiText.Get(UiKey.OptReset);
                resetButton.EnableInClassList("armed", resetArmed);
            }
        }

        static void SetOptionText(Label label, string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        void OnMasterChanged(ChangeEvent<float> evt)
        {
            if (pushingOptions)
            {
                return;
            }

            CommitOptions(GameSettings.Current with { Master = evt.newValue / 100f });
        }

        void OnBgmChanged(ChangeEvent<float> evt)
        {
            if (pushingOptions)
            {
                return;
            }

            CommitOptions(GameSettings.Current with { Bgm = evt.newValue / 100f });
        }

        void OnSeChanged(ChangeEvent<float> evt)
        {
            if (pushingOptions)
            {
                return;
            }

            CommitOptions(GameSettings.Current with { Se = evt.newValue / 100f });
        }

        void OnFlickChanged(ChangeEvent<float> evt)
        {
            if (pushingOptions)
            {
                return;
            }

            CommitOptions(GameSettings.Current with { FlickScale = evt.newValue / 100f });
        }

        void OnTurnChanged(ChangeEvent<float> evt)
        {
            if (pushingOptions)
            {
                return;
            }

            var seconds = GameSettings.SnapTurnSeconds(evt.newValue);
            CommitOptions(GameSettings.Current with { TurnSeconds = seconds });
        }

        void SnapTurnSlider()
        {
            if (turnSlider == null)
            {
                return;
            }

            pushingOptions = true;
            turnSlider.SetValueWithoutNotify(GameSettings.Current.TurnSeconds);
            pushingOptions = false;
        }

        void ToggleCameraShake()
        {
            CommitOptions(GameSettings.Current with { CameraShake = !GameSettings.Current.CameraShake });
        }

        void ToggleReduceMotion()
        {
            CommitOptions(GameSettings.Current with { ReduceMotion = !GameSettings.Current.ReduceMotion });
        }

        void ToggleSeatPalette()
        {
            CommitOptions(GameSettings.Current with { ColorblindSeats = !GameSettings.Current.ColorblindSeats });
        }

        void PaintSeatSwatches(bool colorblind)
        {
            if (root == null)
            {
                return;
            }

            for (var i = 0; i < SeatPalette.Count; i++)
            {
                var swatch = root.Q($"opt-swatch-{i}");
                if (swatch == null)
                {
                    continue;
                }

                swatch.style.backgroundColor = SeatPalette.Ui(i, colorblind);
            }
        }

        void CommitOptions(GameSettings next)
        {
            var scaleChanged = !Mathf.Approximately(GameSettings.Current.UiScale, next.UiScale);
            DisarmReset();
            GameSettings.Save(next);
            RefreshOptionLabels();
            ApplyOptionsLive(scaleChanged);
        }

        void ConfirmReset()
        {
            if (!resetArmed)
            {
                resetArmed = true;
                RefreshOptionLabels();
                return;
            }

            resetArmed = false;
            GameSettings.ResetToDefaults();
            PushOptionsToWidgets();
            ApplyOptionsLive(scaleChanged: true);
        }

        void DisarmReset()
        {
            if (!resetArmed)
            {
                return;
            }

            resetArmed = false;
            RefreshOptionLabels();
        }

        void EnsureCatalog()
        {
            if (contentCatalog == null)
            {
                contentCatalog = Resources.Load<ContentCatalog>("EraserWars/ContentCatalog");
            }

            loadout.EnsureDefaults(contentCatalog);
        }

        void EnsureCollectionBound()
        {
            EnsureCatalog();
            collection = new CollectionPanelController(contentCatalog);
            collection.Bind(collectionPanel);
        }

        void EnsureDeckBound()
        {
            EnsureCatalog();
            deck = new DeckPanelController(contentCatalog, loadout);
            deck.Bind(deckPanel);
        }

        void EnsureSeatPickBound()
        {
            EnsureCatalog();
            seatPick ??= new SeatPickController(contentCatalog, loadout);
            seatPick.Bind(homeStack ?? root);
            seatPick.Refresh(selectedPlayers);
        }

        public void OpenCollection()
        {
            BindDocument();
            EnsureCollectionBound();
            collection?.Refresh();
            NavigateTo(MenuScreen.Collection);
        }

        public void OpenCollectionCards()
        {
            BindDocument();
            EnsureCollectionBound();
            collection?.ShowCards();
            NavigateTo(MenuScreen.Collection);
        }

        public void OpenOptions()
        {
            BindDocument();
            LoadOptions();
            NavigateTo(MenuScreen.Options);
        }

        public void OpenSecretRoom()
        {
            BindDocument();
            NavigateTo(MenuScreen.SecretRoom);
        }

        void CloseOptions()
        {
            DisarmReset();
            GoHome();
        }

        public void OpenDeck()
        {
            BindDocument();
            EnsureDeckBound();
            deck?.Refresh(selectedPlayers);
            NavigateTo(MenuScreen.Deck);
        }

        public void GoHome()
        {
            if (currentScreen == MenuScreen.Deck)
            {
                deck?.TrySave();
            }

            NavigateTo(MenuScreen.Home);
        }

        public void SelectCollectionCharacter(int characterId)
        {
            EnsureCollectionBound();
            collection?.SelectCharacterById(characterId);
            NavigateTo(MenuScreen.Collection);
        }

        void Update()
        {
            SyncHomeWheelBlock();

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (seatPick != null && seatPick.ClosePicker())
            {
                return;
            }

            if (currentScreen != MenuScreen.Home)
            {
                GoHome();
                return;
            }

            if (IsMenuVisible && suspendedMatch)
            {
                ResumeMatch();
                return;
            }

            if (matchRunning)
            {
                var hud = FindAnyObjectByType<MatchHudController>();
                if (hud != null && hud.isActiveAndEnabled)
                {
                    return;
                }

                ShowMenu(suspendMatch: true);
            }
        }

        void SyncHomeWheelBlock()
        {
            if (currentScreen != MenuScreen.Home || director == null || root?.panel == null)
            {
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            director.SetMenuCamera(
                !GameSettings.Current.ReduceMotion,
                PointerOverScroll(mouse.position.ReadValue()),
                blockDrag: false);
        }

        bool PointerOverScroll(Vector2 screenPosition)
        {
            var picked = root.panel.Pick(RuntimePanelUtils.ScreenToPanel(root.panel, screenPosition));
            for (var node = picked; node != null; node = node.parent)
            {
                if (node is ScrollView)
                {
                    return true;
                }
            }

            return false;
        }

        void SelectPlayers(int count)
        {
            selectedPlayers = count;
            mode2P?.EnableInClassList("selected", count == 2);
            mode4P?.EnableInClassList("selected", count == 4);
            seatPick?.Refresh(count);
        }

        void SelectWinMode(WinMode mode)
        {
            GameSettings.Save(GameSettings.Current with { WinMode = mode });
            PaintWinMode();
        }

        void PaintWinMode()
        {
            var points = GameSettings.Current.WinMode == WinMode.Points;
            modeStock?.EnableInClassList("selected", !points);
            modePoints?.EnableInClassList("selected", points);
        }

        void StartMatch()
        {
            BeginMatch(selectedPlayers);
        }

        public void BeginMatch(int playerCount)
        {
            if (director == null)
            {
                director = FindFirstObjectByType<MatchDirector>();
            }

            if (director == null)
            {
                Debug.LogError("[EraserWars] StartMenu.BeginMatch: MatchDirector missing");
                return;
            }

            selectedPlayers = playerCount;
            EnsureCatalog();
            seatPick?.ClosePicker();
            director.SetSeatRoster(loadout.Seats);
            for (var i = 0; i < playerCount; i++)
            {
                director.SetSeatCommons(i, loadout.Commons[i]);
            }

            director.SetDebugChromeVisible(debugModeEnabled);
            director.StartHotseat(playerCount);
            matchRunning = true;
            suspendedMatch = false;
            HideMenu();
            HidePanels();
            // Play may be paused (agent flags); push HUD without waiting for Update.
            var hud = FindAnyObjectByType<MatchHudController>();
            hud?.RefreshFromState();
            var rules = director.State?.Rules;
            Debug.Log(
                $"[EraserWars] StartMenu.BeginMatch({playerCount}) hid menu, phase={director.State?.Phase} " +
                $"win={rules?.WinMode} rounds={rules?.PointRounds}");
        }

        public void ResumeMatch()
        {
            if (director?.State == null)
            {
                return;
            }

            suspendedMatch = false;
            matchRunning = true;
            director.SetDebugChromeVisible(debugModeEnabled);
            HideMenu();
            HidePanels();
            FindAnyObjectByType<MatchHudController>()?.RefreshFromState();
        }

        public void ShowMenu(bool suspendMatch = false)
        {
            // Settle match state before activating: OnEnable hides the menu while a match runs.
            suspendedMatch = suspendMatch && director?.State != null;
            if (!suspendedMatch)
            {
                matchRunning = false;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document != null && !document.enabled)
            {
                document.enabled = true;
            }

            if (root == null && document != null)
            {
                BindDocument();
            }

            if (root != null)
            {
                root.style.display = DisplayStyle.Flex;
                root.visible = true;
            }

            if (document != null)
            {
                document.rootVisualElement.style.display = DisplayStyle.Flex;
            }

            director?.SetDebugChromeVisible(false);
            director?.SetStartMenuOpen(true);
            GoHome();
            RefreshStartButtonLabel();
        }

        void RefreshStartButtonLabel()
        {
            var start = root?.Q<Button>("start-match");
            var resume = root?.Q<Button>("resume-match");
            if (start != null)
            {
                start.text = suspendedMatch
                    ? UiText.Get(UiKey.MenuNewMatch)
                    : UiText.Get(UiKey.MenuStart);
            }

            resume?.EnableInClassList("hidden", !suspendedMatch);
        }

        void HideMenu()
        {
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
                root.visible = false;
            }

            if (document != null)
            {
                document.rootVisualElement.style.display = DisplayStyle.None;
                document.enabled = false;
            }

            director?.SetStartMenuOpen(false);

            // Whole GameObject off so Game view cannot keep painting the title screen.
            gameObject.SetActive(false);
            Debug.Log($"[EraserWars] StartMenu.HideMenu activeSelf={gameObject.activeSelf} doc={(document != null && document.enabled)}");
        }

        void NavigateTo(MenuScreen screen)
        {
            BindDocument();
            if (screen != MenuScreen.Options)
            {
                DisarmReset();
            }

            currentScreen = screen;
            seatPick?.ClosePicker();
            if (screen == MenuScreen.Home)
            {
                seatPick?.Refresh(selectedPlayers);
            }

            HideDestinationPages();

            var showHome = screen == MenuScreen.Home;
            if (menuContent != null)
            {
                menuContent.RemoveFromClassList("hidden");
                menuContent.RemoveFromClassList("dimmed");
                menuContent.style.display = DisplayStyle.Flex;
                menuContent.visible = true;
            }

            if (homeStack != null)
            {
                homeStack.EnableInClassList("hidden", !showHome);
                homeStack.style.display = showHome ? DisplayStyle.Flex : DisplayStyle.None;
                homeStack.visible = showHome;
                homeStack.style.opacity = showHome ? 1f : 0f;
            }

            if (showHome)
            {
                PaintWinMode();
                FirstCoachTip.Apply(root);
            }

            PresentMenuCamera(showHome);

            var page = PageFor(screen);
            if (page == null)
            {
                Debug.Log("[EraserWars] StartMenu.NavigateTo Home");
                return;
            }

            WatchDestinationFrame();
            page.RemoveFromClassList("hidden");
            page.style.display = DisplayStyle.Flex;
            page.style.position = Position.Absolute;
            page.style.backgroundColor = DestinationBackground(screen);
            page.visible = true;
            page.BringToFront();
            // Left/right stretch on this panel can resolve to 0×0. Measure the host when it has a size.
            ApplyDestinationFrame(page);
            Debug.Log(
                $"[EraserWars] StartMenu.NavigateTo {screen} " +
                $"pageStyle={page.style.width}x{page.style.height} parent={page.parent?.name} homeHidden={!showHome}");
        }

        void PresentMenuCamera(bool homeVisible)
        {
            if (director == null)
            {
                director = FindFirstObjectByType<MatchDirector>();
            }

            if (director == null)
            {
                return;
            }

            var idle = homeVisible && !GameSettings.Current.ReduceMotion;
            director.SetMenuCamera(idle, blockWheel: !homeVisible, blockDrag: !homeVisible);
        }

        void WatchDestinationFrame()
        {
            if (destinationFrameBound || menuContent == null)
            {
                return;
            }

            destinationFrameBound = true;
            menuContent.RegisterCallback<GeometryChangedEvent>(_ => RefitDestination());
        }

        void RefitDestination()
        {
            var page = PageFor(currentScreen);
            if (page == null || page.ClassListContains("hidden"))
            {
                return;
            }

            ApplyDestinationFrame(page);
        }

        void ApplyDestinationFrame(VisualElement page)
        {
            const float margin = 0.04f;
            var host = menuContent ?? page.parent;
            var width = host != null ? host.resolvedStyle.width : 0f;
            var height = host != null ? host.resolvedStyle.height : 0f;
            var hostLaidOut = width > 2f && height > 2f && !float.IsNaN(width) && !float.IsNaN(height);
            if (!hostLaidOut)
            {
                var inset = new Length(margin * 100f, LengthUnit.Percent);
                page.style.left = inset;
                page.style.right = inset;
                page.style.top = inset;
                page.style.bottom = inset;
                page.style.width = StyleKeyword.Auto;
                page.style.height = StyleKeyword.Auto;
                page.style.minWidth = StyleKeyword.Auto;
                page.style.minHeight = StyleKeyword.Auto;
                return;
            }

            var marginX = width * margin;
            var marginY = height * margin;
            var nextWidth = Mathf.Max(1f, width - (marginX * 2f));
            var nextHeight = Mathf.Max(1f, height - (marginY * 2f));
            if (Mathf.Abs(page.resolvedStyle.width - nextWidth) < 1f
                && Mathf.Abs(page.resolvedStyle.height - nextHeight) < 1f
                && Mathf.Abs(page.resolvedStyle.left - marginX) < 1f
                && Mathf.Abs(page.resolvedStyle.top - marginY) < 1f)
            {
                return;
            }

            page.style.left = marginX;
            page.style.top = marginY;
            page.style.right = StyleKeyword.Auto;
            page.style.bottom = StyleKeyword.Auto;
            page.style.width = nextWidth;
            page.style.height = nextHeight;
            page.style.minWidth = StyleKeyword.Auto;
            page.style.minHeight = StyleKeyword.Auto;
        }

        VisualElement PageFor(MenuScreen screen)
        {
            switch (screen)
            {
                case MenuScreen.Home:
                    return null;
                case MenuScreen.Collection:
                    return collectionPanel;
                case MenuScreen.Options:
                    return optionsPanel;
                case MenuScreen.Deck:
                    return deckPanel;
                case MenuScreen.SecretRoom:
                    return secretRoomPanel;
                default:
                    var unexpected = screen;
                    throw new ArgumentOutOfRangeException(nameof(screen), unexpected, null);
            }
        }

        void HideDestinationPages()
        {
            HideDestinationPage(optionsPanel);
            HideDestinationPage(deckPanel);
            HideDestinationPage(collectionPanel);
            HideDestinationPage(secretRoomPanel);
        }

        static Color DestinationBackground(MenuScreen screen) =>
            screen switch
            {
                MenuScreen.Collection => new Color(0.18f, 0.42f, 0.28f, 1f),
                MenuScreen.Options => new Color(0.12f, 0.14f, 0.22f, 1f),
                MenuScreen.Deck => new Color(0.07f, 0.09f, 0.16f, 1f),
                MenuScreen.SecretRoom => new Color(0.16f, 0.10f, 0.28f, 1f),
                MenuScreen.Home => new Color(0.07f, 0.09f, 0.16f, 1f),
                _ => new Color(0.07f, 0.09f, 0.16f, 1f),
            };

        static void HideDestinationPage(VisualElement page)
        {
            if (page == null)
            {
                return;
            }

            page.AddToClassList("hidden");
            page.style.display = DisplayStyle.None;
            page.visible = false;
        }

        void HidePanels() => GoHome();

    }
}
