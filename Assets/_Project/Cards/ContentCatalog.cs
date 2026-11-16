using System.Collections.Generic;
using UnityEngine;

namespace EraserWars.Cards
{
    [CreateAssetMenu(menuName = "EraserWars/Content Catalog", fileName = "ContentCatalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        public List<CardDefinition> Cards = new();
        public List<CharacterDefinition> Characters = new();
        public List<EraserDefinition> Erasers = new();

        public CardDefinition FindCard(int id)
        {
            for (var i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];
                if (card != null && card.IdValue == id)
                {
                    return card;
                }
            }

            return null;
        }

        public CharacterDefinition FindCharacter(int id)
        {
            for (var i = 0; i < Characters.Count; i++)
            {
                var character = Characters[i];
                if (character != null && character.IdValue == id)
                {
                    return character;
                }
            }

            return null;
        }
    }
}
