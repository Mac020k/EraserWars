using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Core;

namespace EraserWars.UI
{
    static class DeckCardFacts
    {
        public static List<DeckCardFact> From(List<CardDefinition> cards)
        {
            var facts = new List<DeckCardFact>();
            if (cards == null)
            {
                return facts;
            }

            foreach (var card in cards)
            {
                if (card == null)
                {
                    continue;
                }

                facts.Add(new DeckCardFact(
                    card.IdValue,
                    card.BuildCost,
                    card.MaxCopies,
                    card.DisplayName,
                    card.NameKey));
            }

            return facts;
        }
    }
}
