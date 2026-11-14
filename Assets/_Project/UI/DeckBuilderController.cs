using System.Collections.Generic;
using Keshipin.Cards;
using UnityEngine;

namespace Keshipin.UI
{
    public sealed class DeckBuilderController : MonoBehaviour
    {
        [SerializeField] CharacterDefinition character;
        [SerializeField] List<CardDefinition> selectedCommons = new();

        public bool TryValidate(out string error)
        {
            if (character == null || character.Eraser == null)
            {
                error = "character-missing";
                return false;
            }

            var cost = 0;
            var counts = new Dictionary<int, int>();
            foreach (var card in selectedCommons)
            {
                if (card == null)
                {
                    continue;
                }

                cost += card.BuildCost;
                counts.TryGetValue(card.IdValue, out var n);
                counts[card.IdValue] = n + 1;
                if (counts[card.IdValue] > card.MaxCopies)
                {
                    error = $"max-copies:{card.NameKey}";
                    return false;
                }
            }

            if (selectedCommons.Count > 6)
            {
                error = "too-many-commons";
                return false;
            }

            if (cost > character.Eraser.DeckCostCap)
            {
                error = "over-cost";
                return false;
            }

            error = null;
            return true;
        }
    }
}
