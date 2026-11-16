using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    /// <summary>
    /// Full-screen pre-match menu (ADR-0009 / 6B). CoroCoro-energy CTA, icon-led modes.
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
        Button uiScaleSmall;
        Button uiScaleStandard;
        Button uiScaleLarge;
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
        }

        public bool IsMenuVisible =>
            isActiveAndEnabled
            && (document == null || document.enabled)
            && root != null
            && root.resolvedStyle.display != DisplayStyle.None;

        void OnEnable()
        {
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
                return;
            }

            boundPanelRoot = panelRoot;
            RefreshQueries(panelRoot);
            RegisterMenuCallbacks();
            ApplyMenuIcons();
            EnsureCatalog();
            EnsureCollectionBound();
            EnsureDeckBound();
            EnsureSeatPickBound();
            LoadOptions();
        }

        void RefreshQueries(VisualElement panelRoot)
        {
            root = panelRoot.Q("root") ?? panelRoot;
            menuContent = root.Q("menu-content");
            homeStack = root.Q("home-stack");
            optionsPanel = root.Q("options-panel");
            deckPanel = root.Q("deck-panel");
            collectionPanel = root.Q("collection-panel");
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
            uiScaleSmall = root.Q<Button>("opt-ui-80");
            uiScaleStandard = root.Q<Button>("opt-ui-100");
            uiScaleLarge = root.Q<Button>("opt-ui-120");
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
                uiScaleSmall?.RegisterCallback<ClickEvent>(_ => SetUiScale(0.8f));
                uiScaleStandard?.RegisterCallback<ClickEvent>(_ => SetUiScale(1f));
                uiScaleLarge?.RegisterCallback<ClickEvent>(_ => SetUiScale(1.2f));
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

        void ToggleDebugMode()
        {
            debugModeEnabled = !debugModeEnabled;
            RefreshDebugModeButton();
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

            debugModeButton.text = debugModeEnabled ? "デバッグモード: ON" : "デバッグモード: OFF";
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
                $"shake={settings.CameraShake} ui={settings.UiScale:0.00} listener={AudioListener.volume:0.00}");
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
            SetOptionText(masterValue, $"マスター {Mathf.RoundToInt(settings.Master * 100f)}");
            SetOptionText(bgmValue, $"BGM {Mathf.RoundToInt(settings.Bgm * 100f)}");
            SetOptionText(seValue, $"SE {Mathf.RoundToInt(settings.Se * 100f)}");
            SetOptionText(flickValue, $"フリックの強さ {Mathf.RoundToInt(settings.FlickScale * 100f)}");
            SetOptionText(turnValue, $"1ターン {settings.TurnSeconds}秒");
            if (shakeButton != null)
            {
                shakeButton.text = settings.CameraShake ? "カメラ揺れ: ON" : "カメラ揺れ: OFF";
                shakeButton.EnableInClassList("on", settings.CameraShake);
            }

            uiScaleSmall?.EnableInClassList("selected", settings.UiScale < 0.9f);
            uiScaleStandard?.EnableInClassList("selected", settings.UiScale >= 0.9f && settings.UiScale < 1.1f);
            uiScaleLarge?.EnableInClassList("selected", settings.UiScale >= 1.1f);
            if (resetButton != null && !resetArmed)
            {
                resetButton.text = "初期値に戻す";
                resetButton.EnableInClassList("armed", false);
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

        void SetUiScale(float scale)
        {
            CommitOptions(GameSettings.Current with { UiScale = scale });
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
                if (resetButton != null)
                {
                    resetButton.text = "もう一度押すと初期値に戻します";
                    resetButton.EnableInClassList("armed", true);
                }

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
            if (resetButton != null)
            {
                resetButton.text = "初期値に戻す";
                resetButton.EnableInClassList("armed", false);
            }
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
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
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
                    ShowMenu(suspendMatch: true);
                }
            }
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
                start.text = suspendedMatch ? "あたらしいマッチ" : "マッチ開始";
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

            var page = screen switch
            {
                MenuScreen.Collection => collectionPanel,
                MenuScreen.Options => optionsPanel,
                MenuScreen.Deck => deckPanel,
                MenuScreen.Home => null,
                _ => null,
            };

            if (page == null)
            {
                Debug.Log("[EraserWars] StartMenu.NavigateTo Home");
                return;
            }

            // Fixed pixel box — left/right/top/bottom stretch resolves to 0x0 on this panel.
            // ~900x700 is the size that lays out reliably in Editor Game view.
            const float pageW = 900f;
            const float pageH = 700f;

            page.RemoveFromClassList("hidden");
            page.style.display = DisplayStyle.Flex;
            page.style.position = Position.Absolute;
            page.style.left = 40;
            page.style.top = 40;
            page.style.right = StyleKeyword.Auto;
            page.style.bottom = StyleKeyword.Auto;
            page.style.width = pageW;
            page.style.height = pageH;
            page.style.minWidth = pageW;
            page.style.minHeight = pageH;
            page.style.backgroundColor = DestinationBackground(screen);
            page.visible = true;
            page.BringToFront();
            Debug.Log(
                $"[EraserWars] StartMenu.NavigateTo {screen} " +
                $"pageBox={pageW:0}x{pageH:0} parent={page.parent?.name} homeHidden={!showHome}");
        }

        void HideDestinationPages()
        {
            HideDestinationPage(optionsPanel);
            HideDestinationPage(deckPanel);
            HideDestinationPage(collectionPanel);
        }

        static Color DestinationBackground(MenuScreen screen) =>
            screen switch
            {
                MenuScreen.Collection => new Color(0.18f, 0.42f, 0.28f, 1f),
                MenuScreen.Options => new Color(0.12f, 0.14f, 0.22f, 1f),
                MenuScreen.Deck => new Color(0.07f, 0.09f, 0.16f, 1f),
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
