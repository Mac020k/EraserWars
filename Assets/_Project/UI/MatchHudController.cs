using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class MatchHudController : MonoBehaviour
    {
        [SerializeField] MatchDirector director;
        [SerializeField] UIDocument document;
        [SerializeField] StartMenuController startMenu;

        VisualElement root;
        Label phaseLabel;
        Label stocksLabel;
        Label timerLabel;

        void OnEnable()
        {
            if (document == null)
            {
                return;
            }

            if (startMenu == null)
            {
                startMenu = FindAnyObjectByType<StartMenuController>();
            }

            root = document.rootVisualElement.Q("root") ?? document.rootVisualElement;
            phaseLabel = root.Q<Label>("phase");
            stocksLabel = root.Q<Label>("stocks");
            timerLabel = root.Q<Label>("timer");
            root.Q<Button>("pass")?.RegisterCallback<ClickEvent>(_ => director?.Pass());
            root.Q<Button>("rematch")?.RegisterCallback<ClickEvent>(_ => director?.Rematch());
            root.Q<Button>("menu")?.RegisterCallback<ClickEvent>(_ => startMenu?.ShowMenu());
            SetVisible(false);
        }

        void Update()
        {
            if (director?.State == null)
            {
                SetVisible(false);
                return;
            }

            if (startMenu != null && startMenu.IsMenuVisible)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            var state = director.State;
            if (phaseLabel != null)
            {
                phaseLabel.text = $"{state.Phase}  ROUND {state.Round}";
            }

            if (timerLabel != null)
            {
                timerLabel.text = $"TIME {state.TurnTimeLeft:0.0}";
            }

            if (stocksLabel != null)
            {
                stocksLabel.text = string.Join("\n", state.Players);
            }
        }

        void SetVisible(bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.EnableInClassList("hidden", !visible);
        }
    }
}
