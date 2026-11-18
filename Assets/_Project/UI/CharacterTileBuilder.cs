using EraserWars.Cards;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    /// <summary>
    /// Builds the character name card: face circle, class line, then the large name,
    /// with the archetype diamond pinned to the top-right corner.
    /// </summary>
    public static class CharacterTileBuilder
    {
        public const string LockedPlaceText = "？？？";
        const float MaxNameFontSize = 24f;
        const float MinNameFontSize = 14f;
        const float NameWidth = 112f;

        public static Button Create(CharacterDefinition character, bool unlocked)
        {
            var tile = new Button();
            tile.AddToClassList("char-tile");
            tile.EnableInClassList("locked", !unlocked);

            var art = new VisualElement();
            art.AddToClassList("char-tile-art");
            PaintFace(art, character, unlocked);
            tile.Add(art);

            if (unlocked)
            {
                var badge = ArchetypeMark.Create(character.ArchetypeKey);
                badge.AddToClassList("char-tile-badge");
                tile.Add(badge);
            }

            var place = new Label(unlocked ? CharacterFlavor.TileCaption(character.IdValue) : LockedPlaceText);
            place.AddToClassList("char-tile-place");
            tile.Add(place);

            var name = new Label(unlocked ? character.DisplayName : character.MaskedDisplayName);
            name.AddToClassList("char-tile-name");
            name.style.fontSize = NameFontSize(name.text);
            tile.Add(name);

            tile.userData = character.IdValue;
            return tile;
        }

        /// <summary>
        /// Largest font size that keeps the name on one line inside the card.
        /// Wide glyphs count as one em, ASCII letters as 0.6, and spaces as 0.3.
        /// </summary>
        public static float NameFontSize(string text)
        {
            var ems = 0f;
            foreach (var c in text)
            {
                if (c == ' ' || c == '\u3000')
                {
                    ems += 0.3f;
                }
                else
                {
                    ems += c < 0x250 ? 0.6f : 1f;
                }
            }

            if (ems <= 0f)
            {
                return MaxNameFontSize;
            }

            return Mathf.Clamp(NameWidth / ems, MinNameFontSize, MaxNameFontSize);
        }

        public static void PaintFace(VisualElement art, CharacterDefinition character, bool unlocked)
        {
            art.Clear();
            if (!unlocked)
            {
                UiImageUtil.Clear(art);
                art.Add(LockMark.Create());
                return;
            }

            UiImageUtil.ApplyFaceCrop(art, character);
        }
    }
}
