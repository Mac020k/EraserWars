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
        Button mode2P;
        Button mode4P;
        int selectedPlayers = 2;
        bool matchRunning;

        public bool IsMenuVisible =>
            isActiveAndEnabled
            && (document == null || document.enabled)
            && root != null
            && root.resolvedStyle.display != DisplayStyle.None;

        void OnEnable()
        {
            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null)
            {
                return;
            }

            root = document.rootVisualElement;
            optionsPanel = root.Q("options-panel");
            deckPanel = root.Q("deck-panel");
            collectionPanel = root.Q("collection-panel");
            mode2P = root.Q<Button>("mode-2p");
            mode4P = root.Q<Button>("mode-4p");

            root.Q<Button>("start-match")?.RegisterCallback<ClickEvent>(_ => StartMatch());
            mode2P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(2));
            mode4P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(4));
            root.Q<Button>("open-options")?.RegisterCallback<ClickEvent>(_ => ShowPanel(optionsPanel));
            root.Q<Button>("open-deck")?.RegisterCallback<ClickEvent>(_ => ShowPanel(deckPanel));
            root.Q<Button>("open-collection")?.RegisterCallback<ClickEvent>(_ => ShowPanel(collectionPanel));
            root.Q<Button>("close-options")?.RegisterCallback<ClickEvent>(_ => HidePanels());
            root.Q<Button>("close-deck")?.RegisterCallback<ClickEvent>(_ => HidePanels());
            root.Q<Button>("close-collection")?.RegisterCallback<ClickEvent>(_ => HidePanels());

            ApplyIcon(root.Q("brand-icon"), brandIcon);
            ApplyIcon(root.Q("icon-2p"), icon2P);
            ApplyIcon(root.Q("icon-4p"), icon4P);
            ApplyIcon(root.Q("icon-deck"), iconDeck);
            ApplyIcon(root.Q("icon-collection"), iconCollection);
            ApplyIcon(root.Q("icon-options"), iconOptions);
            SelectPlayers(selectedPlayers);
            ShowMenu();
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

                if (matchRunning)
                {
                    ShowMenu();
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
            director.SetDebugChromeVisible(false);
            director.StartHotseat(playerCount);
            matchRunning = true;
            HideMenu();
            HidePanels();
            // Play may be paused (agent flags); push HUD without waiting for Update.
            var hud = FindAnyObjectByType<MatchHudController>();
            hud?.RefreshFromState();
            Debug.Log($"[EraserWars] StartMenu.BeginMatch({playerCount}) hid menu, phase={director.State?.Phase}");
        }

        public void ShowMenu()
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
                root = document.rootVisualElement;
            }

            if (root != null)
            {
                root.style.display = DisplayStyle.Flex;
            }

            matchRunning = false;
            director?.SetDebugChromeVisible(false);
        }

        void HideMenu()
        {
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
            }

            if (document != null)
            {
                document.enabled = false;
            }

            // Whole GameObject off so Game view cannot keep painting the title screen.
            gameObject.SetActive(false);
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
