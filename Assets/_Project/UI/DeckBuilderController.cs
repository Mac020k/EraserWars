using System;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Core;
using UnityEngine;

namespace EraserWars.UI
{
    public sealed class DeckBuilderController : MonoBehaviour
    {
        [SerializeField] CharacterDefinition character;
        [SerializeField] List<CardDefinition> selectedCommons = new();

        public bool TryValidate(out string error)
        {
            var eraser = character != null ? character.Eraser : null;
            var verdict = DeckRules.Evaluate(
                eraser != null,
                eraser != null ? eraser.DeckCostCap : 0,
                DeckCardFacts.From(selectedCommons));
            switch (verdict.Fault)
            {
                case DeckFault.None:
                    error = null;
                    return true;
                case DeckFault.NoCharacter:
                    error = "character-missing";
                    return false;
                case DeckFault.SameName:
                    error = $"max-copies:{verdict.NameKey}";
                    return false;
                case DeckFault.Slots:
                    error = "too-many-commons";
                    return false;
                case DeckFault.OverCost:
                    error = "over-cost";
                    return false;
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(DeckFault)} '{verdict.Fault}'.");
            }
        }
    }
}
