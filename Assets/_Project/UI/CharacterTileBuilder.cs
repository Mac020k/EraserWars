using EraserWars.Cards;
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
            name.EnableInClassList("long", name.text.Length >= 6 && name.text.Length < 8);
            name.EnableInClassList("longer", name.text.Length >= 8);
            tile.Add(name);

            tile.userData = character.IdValue;
            return tile;
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
