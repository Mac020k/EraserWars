using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public sealed class TimerRingElement : VisualElement
    {
        public float Fill { get; private set; }
        public Color ArcColor { get; private set; } = Color.white;
        public bool HasArc { get; private set; }

        public TimerRingElement()
        {
            name = "timer-ring";
            pickingMode = PickingMode.Ignore;
            AddToClassList("timer-ring");
            generateVisualContent += Paint;
        }

        public static TimerRingElement Adopt(VisualElement slot)
        {
            if (slot is TimerRingElement ready)
            {
                return ready;
            }

            var created = new TimerRingElement();
            if (slot == null)
            {
                return created;
            }

            var parent = slot.parent;
            var index = parent.IndexOf(slot);
            parent.Insert(index, created);
            slot.RemoveFromHierarchy();
            return created;
        }

        public void SetArc(float fill, Color color, bool ticking)
        {
            fill = Mathf.Clamp01(fill);
            var hasArc = ticking && fill > 0f;
            if (Mathf.Approximately(Fill, fill) && ArcColor.Equals(color) && HasArc == hasArc)
            {
                return;
            }

            Fill = fill;
            ArcColor = color;
            HasArc = hasArc;
            MarkDirtyRepaint();
        }

        void Paint(MeshGenerationContext ctx)
        {
            var rect = contentRect;
            var radius = Mathf.Min(rect.width, rect.height) * 0.5f - 7f;
            if (radius < 4f)
            {
                return;
            }

            var center = rect.center;
            var painter = ctx.painter2D;
            painter.lineCap = LineCap.Round;
            painter.lineWidth = 9f;
            painter.strokeColor = new Color(1f, 1f, 1f, 0.95f);
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(0f), Angle.Degrees(359.9f));
            painter.Stroke();

            if (!HasArc)
            {
                return;
            }

            var sweep = TurnChrome.SweepDegrees(Fill);
            if (sweep >= 359.9f)
            {
                sweep = 359.9f;
            }

            painter.lineWidth = 11f;
            painter.strokeColor = ArcColor;
            painter.BeginPath();
            painter.Arc(
                center,
                radius,
                Angle.Degrees(-90f),
                Angle.Degrees(-90f + sweep),
                ArcDirection.Clockwise);
            painter.Stroke();
        }
    }
}
