using EraserWars.Cards;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public static class ArchetypeMark
    {
        public static VisualElement Create(string archetypeKey)
        {
            var mark = new VisualElement { pickingMode = PickingMode.Ignore };
            mark.AddToClassList("arch-mark");
            mark.tooltip = CharacterArchetypes.DisplayJa(archetypeKey);
            switch (archetypeKey)
            {
                case CharacterArchetypes.AllOut:
                    mark.AddToClassList("arch-all-out");
                    mark.Add(Part("arch-diamond"));
                    break;
                case CharacterArchetypes.SeatTake:
                    mark.AddToClassList("arch-seat-take");
                    mark.Add(Part("arch-chair-back"));
                    mark.Add(Part("arch-chair-seat"));
                    break;
                case CharacterArchetypes.Teacher:
                    mark.AddToClassList("arch-teacher");
                    mark.Add(Part("arch-teacher-head"));
                    mark.Add(Part("arch-teacher-body"));
                    break;
                case CharacterArchetypes.Afterschool:
                    mark.AddToClassList("arch-afterschool");
                    mark.Add(Part("arch-flask-neck"));
                    mark.Add(Part("arch-flask-bowl"));
                    break;
                case CharacterArchetypes.DeskKing:
                    mark.AddToClassList("arch-desk-king");
                    var points = new VisualElement { pickingMode = PickingMode.Ignore };
                    points.AddToClassList("arch-crown-points");
                    points.Add(Part("arch-crown-point"));
                    points.Add(Part("arch-crown-point"));
                    points.Add(Part("arch-crown-point"));
                    mark.Add(points);
                    mark.Add(Part("arch-crown-base"));
                    break;
                default:
                    mark.AddToClassList("arch-unknown");
                    break;
            }

            return mark;
        }

        static VisualElement Part(string className)
        {
            var part = new VisualElement { pickingMode = PickingMode.Ignore };
            part.AddToClassList(className);
            return part;
        }
    }

    public static class LockMark
    {
        public const string ElementName = "char-lock";

        public static VisualElement Create()
        {
            var root = new VisualElement
            {
                name = ElementName,
                pickingMode = PickingMode.Ignore,
            };
            root.AddToClassList("char-lock");
            root.Add(Part("char-lock-shackle"));
            root.Add(Part("char-lock-body"));
            return root;
        }

        static VisualElement Part(string className)
        {
            var part = new VisualElement { pickingMode = PickingMode.Ignore };
            part.AddToClassList(className);
            return part;
        }
    }
}
