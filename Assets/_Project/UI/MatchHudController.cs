using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class MatchHudController : MonoBehaviour
    {
        [SerializeField] MatchDirector director;
        [SerializeField] UIDocument document;

        Label phaseLabel;
        Label stocksLabel;

        void OnEnable()
        {
            if (document == null)
            {
                return;
            }

            var root = document.rootVisualElement;
            phaseLabel = root.Q<Label>("phase");
            stocksLabel = root.Q<Label>("stocks");
            root.Q<Button>("pass")?.RegisterCallback<ClickEvent>(_ => director?.Pass());
            root.Q<Button>("rematch")?.RegisterCallback<ClickEvent>(_ => director?.Rematch());
        }

        void Update()
        {
            if (director?.State == null)
            {
                return;
            }

            var state = director.State;
            if (phaseLabel != null)
            {
                phaseLabel.text = $"{state.Phase} R{state.Round}";
            }

            if (stocksLabel != null)
            {
                stocksLabel.text = string.Join(" | ", state.Players);
            }
        }
    }
}
