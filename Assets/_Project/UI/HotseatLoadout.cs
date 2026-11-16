using System.Collections.Generic;
using EraserWars.Cards;

namespace EraserWars.UI
{
    public sealed class HotseatLoadout
    {
        public readonly CharacterDefinition[] Seats = new CharacterDefinition[4];
        public readonly List<CardDefinition>[] Commons =
        {
            new(), new(), new(), new(),
        };

        public void EnsureDefaults(ContentCatalog catalog)
        {
            if (catalog?.Characters == null || catalog.Characters.Count == 0)
            {
                return;
            }

            for (var i = 0; i < Seats.Length; i++)
            {
                if (Seats[i] == null)
                {
                    Seats[i] = catalog.FindCharacter(i) ?? catalog.Characters[i % catalog.Characters.Count];
                }

                if (Commons[i].Count > 0 || catalog.Cards == null)
                {
                    continue;
                }

                // Seed a tiny default common package so hands are non-empty in hotseat.
                TryAddCommon(Commons[i], catalog.FindCard(1));
                TryAddCommon(Commons[i], catalog.FindCard(5));
                TryAddCommon(Commons[i], catalog.FindCard(12));
            }
        }

        static void TryAddCommon(List<CardDefinition> list, CardDefinition card)
        {
            if (card != null && !card.IsSignature)
            {
                list.Add(card);
            }
        }
    }
}
