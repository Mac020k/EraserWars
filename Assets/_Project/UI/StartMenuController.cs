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
        [SerializeField] Texture2D iconCollection;
        [SerializeField] Texture2D iconOptions;

        VisualElement root;
        VisualElement optionsPanel;
        VisualElement collectionPanel;
        Button mode2P;
        Button mode4P;
        int selectedPlayers = 2;
        bool matchRunning;

        public bool IsMenuVisible =>
            root != null && root.resolvedStyle.display != DisplayStyle.None;

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
            collectionPanel = root.Q("collection-panel");
            mode2P = root.Q<Button>("mode-2p");
            mode4P = root.Q<Button>("mode-4p");

            root.Q<Button>("start-match")?.RegisterCallback<ClickEvent>(_ => StartMatch());
            mode2P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(2));
            mode4P?.RegisterCallback<ClickEvent>(_ => SelectPlayers(4));
            root.Q<Button>("open-options")?.RegisterCallback<ClickEvent>(_ => ShowPanel(optionsPanel));
            root.Q<Button>("open-collection")?.RegisterCallback<ClickEvent>(_ => ShowPanel(collectionPanel));
            root.Q<Button>("close-options")?.RegisterCallback<ClickEvent>(_ => HidePanels());
            root.Q<Button>("close-collection")?.RegisterCallback<ClickEvent>(_ => HidePanels());

            ApplyIcon(root.Q("brand-icon"), brandIcon);
            ApplyIcon(root.Q("icon-2p"), icon2P);
            ApplyIcon(root.Q("icon-4p"), icon4P);
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
            if (director == null)
            {
                return;
            }

            director.SetDebugChromeVisible(false);
            director.StartHotseat(selectedPlayers);
            matchRunning = true;
            HideMenu();
            HidePanels();
        }

        public void ShowMenu()
        {
            if (root == null)
            {
                return;
            }

            root.style.display = DisplayStyle.Flex;
            matchRunning = false;
            director?.SetDebugChromeVisible(false);
        }

        void HideMenu()
        {
            if (root == null)
            {
                return;
            }

            root.style.display = DisplayStyle.None;
        }

        void ShowPanel(VisualElement panel)
        {
            HidePanels();
            panel?.RemoveFromClassList("hidden");
        }

        void HidePanels()
        {
            optionsPanel?.AddToClassList("hidden");
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
