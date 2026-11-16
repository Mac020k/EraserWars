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

        public bool IsMenuVisible =>
            isActiveAndEnabled
            && (document == null || document.enabled)
            && root != null
            && root.resolvedStyle.display != DisplayStyle.None;

        void OnEnable()
        {
            BindDocument();
            if (root == null)
            {
                return;
            }

            SelectPlayers(selectedPlayers);
            if (!matchRunning)
            {
                ShowMenu();
            }
            else
            {
                HideMenu();
            }
        }

        void BindDocument()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null)
            {
                return;
            }

            var panelRoot = document.rootVisualElement;
            root = panelRoot.Q("root") ?? panelRoot;
            optionsPanel = root.Q("options-panel");
            deckPanel = root.Q("deck-panel");
            collectionPanel = root.Q("collection-panel");
            mode2P = root.Q<Button>("mode-2p");
            mode4P = root.Q<Button>("mode-4p");

            root.Q<Button>("start-match")?.RegisterCallback<ClickEvent>(_ => StartMatch());
            root.Q<Button>("resume-match")?.RegisterCallback<ClickEvent>(_ => ResumeMatch());
            mode2P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(2));
            mode4P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(4));
            root.Q<Button>("open-options")?.RegisterCallback<ClickEvent>(_ => ShowPanel(optionsPanel));
            root.Q<Button>("open-deck")?.RegisterCallback<ClickEvent>(_ => OpenDeck());
            root.Q<Button>("open-collection")?.RegisterCallback<ClickEvent>(_ => OpenCollection());
            root.Q<Button>("close-options")?.RegisterCallback<ClickEvent>(_ => HidePanels());
            root.Q<Button>("close-deck")?.RegisterCallback<ClickEvent>(_ => HidePanels());
            root.Q<Button>("close-collection")?.RegisterCallback<ClickEvent>(_ => HidePanels());

            ApplyIcon(root.Q("brand-icon"), brandIcon);
            ApplyIcon(root.Q("icon-2p"), icon2P);
            ApplyIcon(root.Q("icon-4p"), icon4P);
            ApplyIcon(root.Q("icon-deck"), iconDeck);
            ApplyIcon(root.Q("icon-collection"), iconCollection);
            ApplyIcon(root.Q("icon-options"), iconOptions);

            EnsureCatalog();
            EnsureCollectionBound();
            EnsureDeckBound();
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
            EnsureCollectionBound();
            collection?.Refresh();
            ShowPanel(collectionPanel);
        }

        public void OpenDeck()
        {
            EnsureDeckBound();
            deck?.Refresh(selectedPlayers);
            ShowPanel(deckPanel);
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
                if (optionsPanel != null && !optionsPanel.ClassListContains("hidden"))
                {
                    HidePanels();
                    return;
                }

                if (deckPanel != null && !deckPanel.ClassListContains("hidden"))
                {
                    HidePanels();
                    return;
                }

                if (collectionPanel != null && !collectionPanel.ClassListContains("hidden"))
                {
                    HidePanels();
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

            director.SetDebugChromeVisible(false);
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
            HideMenu();
            HidePanels();
            FindAnyObjectByType<MatchHudController>()?.RefreshFromState();
        }

        public void ShowMenu(bool suspendMatch = false)
        {
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

            suspendedMatch = suspendMatch && director?.State != null;
            if (!suspendedMatch)
            {
                matchRunning = false;
            }

            director?.SetDebugChromeVisible(false);
            director?.SetStartMenuOpen(true);
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

        void ShowPanel(VisualElement panel)
        {
            HidePanels();
            panel?.RemoveFromClassList("hidden");
        }

        void HidePanels()
        {
            optionsPanel?.AddToClassList("hidden");
            deckPanel?.AddToClassList("hidden");
            collectionPanel?.AddToClassList("hidden");
        }

        static void ApplyIcon(VisualElement element, Texture2D texture)
        {
            if (element == null || texture == null)
            {
                return;
            }

            element.style.backgroundImage = new StyleBackground(texture);
            element.style.backgroundColor = Color.clear;
        }
    }
}
