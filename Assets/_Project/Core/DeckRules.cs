using System;
using System.Collections.Generic;

namespace EraserWars.Core
{
    public enum DeckFault
    {
        None,
        NoCharacter,
        Slots,
        SameName,
        OverCost,
    }

    public readonly record struct DeckCardFact(
        int Id,
        int BuildCost,
        int MaxCopies,
        string DisplayName,
        string NameKey);

    public readonly record struct DeckVerdict(
        DeckFault Fault,
        string Message,
        string NameKey,
        string DisplayName,
        int MaxCopies);

    public static class DeckRules
    {
        public const int SlotCap = 6;

        public static DeckVerdict Evaluate(bool hasCharacter, int costCap, IReadOnlyList<DeckCardFact> cards)
        {
            if (!hasCharacter)
            {
                return Make(DeckFault.NoCharacter, "", "", 0);
            }

            var counts = new Dictionary<int, int>();
            var used = 0;
            var count = 0;
            if (cards != null)
            {
                for (var i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    used += card.BuildCost;
                    count++;
                    counts.TryGetValue(card.Id, out var copies);
                    copies++;
                    counts[card.Id] = copies;
                    if (copies > card.MaxCopies)
                    {
                        return Make(
                            DeckFault.SameName,
                            card.DisplayName,
                            card.NameKey,
                            card.MaxCopies);
                    }
                }
            }

            if (count > SlotCap)
            {
                return Make(DeckFault.Slots, "", "", 0);
            }

            if (used > costCap)
            {
                return Make(DeckFault.OverCost, "", "", 0);
            }

            return Make(DeckFault.None, "", "", 0);
        }

        static DeckVerdict Make(DeckFault fault, string displayName, string nameKey, int maxCopies)
        {
            var name = displayName ?? "";
            var key = nameKey ?? "";
            return new DeckVerdict(fault, Message(fault, name, maxCopies), key, name, maxCopies);
        }

        static string Message(DeckFault fault, string name, int maxCopies) => fault switch
        {
            DeckFault.None => "",
            DeckFault.NoCharacter => "キャラを選んでください。",
            DeckFault.Slots => $"コモンは{SlotCap}枚までです。",
            DeckFault.SameName => $"{name} は{maxCopies}枚までです。",
            DeckFault.OverCost => "上限を超えています。カードを外してください。",
            _ => throw new InvalidOperationException($"Unhandled {nameof(DeckFault)} '{fault}'."),
        };
    }
}
