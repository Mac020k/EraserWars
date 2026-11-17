using System;
using System;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Persistence;

namespace EraserWars.UI
{
    public sealed class HotseatLoadout
    {
        const int DefaultCap = 8;

        public readonly CharacterDefinition[] Seats = new CharacterDefinition[4];
        public readonly List<CardDefinition>[] Commons =
        {
            new(), new(), new(), new(),
        };

        public bool TryAssignUnlocked(int seat, CharacterDefinition character)
        {
            if (seat < 0 || seat >= Seats.Length || !CharacterAccess.IsUnlocked(character))
            {
                return false;
            }

            Seats[seat] = character;
            return true;
        }

        public void ApplyActivePreset(int seat, ContentCatalog catalog)
        {
            if (seat < 0 || seat >= Seats.Length || catalog == null)
            {
                return;
            }

            var character = Seats[seat];
            if (character == null)
            {
                return;
            }

            var slot = DeckPresets.GetActiveSlot(character.IdValue);
            if (!DeckPresets.HasSaved(character.IdValue, slot))
            {
                Commons[seat].Clear();
                SeedStarter(Commons[seat], catalog);
                return;
            }

            var resolved = ResolveCommons(catalog, DeckPresets.LoadCards(character.IdValue, slot));
            var cap = character.Eraser != null ? character.Eraser.DeckCostCap : DefaultCap;
            var fault = DeckRules.Evaluate(true, cap, DeckCardFacts.From(resolved)).Fault;
            switch (fault)
            {
                case DeckFault.None:
                    Commons[seat].Clear();
                    Commons[seat].AddRange(resolved);
                    return;
                case DeckFault.NoCharacter:
                case DeckFault.Slots:
                case DeckFault.SameName:
                case DeckFault.OverCost:
                    return;
                default:
                    throw new InvalidOperationException($"Unhandled {nameof(DeckFault)} '{fault}'.");
            }
        }

        public void ReleaseLockedSeats(ContentCatalog catalog)
        {
            for (var i = 0; i < Seats.Length; i++)
            {
                if (Seats[i] != null && !CharacterAccess.IsUnlocked(Seats[i]))
                {
                    Seats[i] = null;
                }
            }

            EnsureDefaults(catalog);
        }

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

                var character = Seats[i];
                if (character != null
                    && DeckPresets.HasSaved(character.IdValue, DeckPresets.GetActiveSlot(character.IdValue)))
                {
                    ApplyActivePreset(i, catalog);
                    continue;
                }

                SeedStarter(Commons[i], catalog);
            }
        }

        public static List<CardDefinition> ResolveCommons(ContentCatalog catalog, IReadOnlyList<int> ids)
        {
            var list = new List<CardDefinition>();
            if (ids == null || catalog?.Cards == null)
            {
                return list;
            }

            for (var i = 0; i < ids.Count; i++)
            {
                var card = catalog.FindCard(ids[i]);
                if (card != null && !card.IsSignature)
                {
                    list.Add(card);
                }
            }

            return list;
        }

        public static void SeedStarter(List<CardDefinition> list, ContentCatalog catalog)
        {
            if (list == null || catalog?.Cards == null)
            {
                return;
            }

            TryAddCommon(list, catalog.FindCard(1));
            TryAddCommon(list, catalog.FindCard(5));
            TryAddCommon(list, catalog.FindCard(12));
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
