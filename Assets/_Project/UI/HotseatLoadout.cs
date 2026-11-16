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
                if (Seats[i] != null)
                {
                    continue;
                }

                Seats[i] = catalog.FindCharacter(i) ?? catalog.Characters[i % catalog.Characters.Count];
            }
        }
    }
}
