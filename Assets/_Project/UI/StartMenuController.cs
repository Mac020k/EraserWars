using EraserWars.Cards;
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
        readonly HotseatLoadout loadout = new();
        Button mode2P;
        Button mode4P;
        int selectedPlayers = 2;
        bool matchRunning;
        bool suspendedMatch;
        MenuScreen currentScreen = MenuScreen.Home;
        // Dev default ON; hide the options control before release.
        bool debugModeEnabled = true;
        Button debugModeButton;

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
            if (root == null)
            {
                return;
            }

            SelectPlayers(selectedPlayers);
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
            debugModeButton = root.Q<Button>("opt-debug-mode");
        }

        void RegisterMenuCallbacks()
        {
            root.Q<Button>("start-match")?.RegisterCallback<ClickEvent>(_ => StartMatch());
            root.Q<Button>("resume-match")?.RegisterCallback<ClickEvent>(_ => ResumeMatch());
            mode2P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(2));
            mode4P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(4));
            root.Q<Button>("open-options")?.RegisterCallback<ClickEvent>(_ => OpenOptions());
            root.Q<Button>("open-deck")?.RegisterCallback<ClickEvent>(_ => OpenDeck());
            root.Q<Button>("open-collection")?.RegisterCallback<ClickEvent>(_ => OpenCollection());
            root.Q<Button>("close-options")?.RegisterCallback<ClickEvent>(_ => GoHome());
            root.Q<Button>("close-deck")?.RegisterCallback<ClickEvent>(_ => GoHome());
            root.Q<Button>("close-collection")?.RegisterCallback<ClickEvent>(_ => GoHome());
            debugModeButton?.RegisterCallback<ClickEvent>(_ => ToggleDebugMode());
            RefreshDebugModeButton();
        }

        void ApplyMenuIcons()
        {
            UiImageUtil.ApplyFit(root.Q("brand-icon"), brandIcon);
            UiImageUtil.ApplyFit(root.Q("icon-2p"), icon2P);
            UiImageUtil.ApplyFit(root.Q("icon-4p"), icon4P);
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

        public void OpenCollection()
        {
            BindDocument();
            EnsureCollectionBound();
            collection?.Refresh();
            NavigateTo(MenuScreen.Collection);
        }

        public void OpenOptions()
        {
            BindDocument();
            NavigateTo(MenuScreen.Options);
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
            Debug.Log($"[EraserWars] StartMenu.BeginMatch({playerCount}) hid menu, phase={director.State?.Phase}");
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
            currentScreen = screen;

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
            page.style.backgroundColor = new Color(0.07f, 0.09f, 0.16f, 1f);
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
