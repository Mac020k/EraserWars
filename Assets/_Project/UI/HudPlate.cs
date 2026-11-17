using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public static class HudPlate
    {
        const string BoundClass = "plate-bound";
        const string RailBoundClass = "rail-bound";
        const float Exponent = 4.6f;

        public static void Bind(VisualElement element, Func<PlateInk> ink)
        {
            if (element == null || ink == null || element.ClassListContains(BoundClass))
            {
                return;
            }

            element.AddToClassList(BoundClass);
            element.style.backgroundColor = Color.clear;
            element.style.borderTopWidth = 0f;
            element.style.borderRightWidth = 0f;
            element.style.borderBottomWidth = 0f;
            element.style.borderLeftWidth = 0f;
            element.style.backgroundImage = new StyleBackground(StyleKeyword.None);
            element.generateVisualContent += ctx => PaintPlate(ctx, element, ink());
        }

        public static void BindRail(VisualElement element)
        {
            if (element == null || element.ClassListContains(RailBoundClass))
            {
                return;
            }

            element.AddToClassList(RailBoundClass);
            element.pickingMode = PickingMode.Ignore;
            element.generateVisualContent += ctx => PaintRail(ctx, element);
        }

        public readonly struct PlateInk
        {
            public PlateInk(Color fill, Color stroke, float strokeWidth)
            {
                Fill = fill;
                Stroke = stroke;
                StrokeWidth = strokeWidth;
            }

            public Color Fill { get; }
            public Color Stroke { get; }
            public float StrokeWidth { get; }
        }

        static void PaintPlate(MeshGenerationContext ctx, VisualElement element, PlateInk ink)
        {
            var width = element.layout.width;
            var height = element.layout.height;
            if (width < 2f || height < 2f)
            {
                return;
            }

            var inset = Mathf.Max(1f, ink.StrokeWidth * 0.5f);
            var painter = ctx.painter2D;
            painter.fillColor = ink.Fill;
            painter.strokeColor = ink.Stroke;
            painter.lineWidth = ink.StrokeWidth;
            painter.BeginPath();
            Superellipse(painter, new Rect(0f, 0f, width, height), inset);
            painter.Fill();
            if (ink.StrokeWidth > 0.5f)
            {
                painter.Stroke();
            }
        }

        static void Superellipse(Painter2D painter, Rect rect, float inset)
        {
            var a = rect.width * 0.5f - inset;
            var b = rect.height * 0.5f - inset;
            if (a < 1f || b < 1f)
            {
                return;
            }

            var cx = rect.center.x;
            var cy = rect.center.y;
            const int steps = 36;
            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps * Mathf.PI * 2f;
                var ct = Mathf.Cos(t);
                var st = Mathf.Sin(t);
                var x = cx + a * Mathf.Sign(ct) * Mathf.Pow(Mathf.Abs(ct), 2f / Exponent);
                var y = cy + b * Mathf.Sign(st) * Mathf.Pow(Mathf.Abs(st), 2f / Exponent);
                var point = new Vector2(x, y);
                if (i == 0)
                {
                    painter.MoveTo(point);
                }
                else
                {
                    painter.LineTo(point);
                }
            }

            painter.ClosePath();
        }

        static void PaintRail(MeshGenerationContext ctx, VisualElement element)
        {
            var width = element.layout.width;
            var height = element.layout.height;
            if (width < 32f || height < 32f)
            {
                return;
            }

            const float circleTop = 4f;
            const float circleSize = 156f;
            var radius = circleSize * 0.5f;
            var cy = circleTop + radius;
            var cx = width * 0.5f;
            var y = 48f;
            var dy = y - cy;
            var reach = radius * radius - dy * dy;
            var half = reach > 0f ? Mathf.Sqrt(reach) + 4f : radius;
            var painter = ctx.painter2D;
            painter.strokeColor = new Color(1f, 120f / 255f, 176f / 255f, 1f);
            painter.lineWidth = 10f;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.MoveTo(new Vector2(18f, 6f));
            painter.LineTo(new Vector2(78f, y));
            painter.LineTo(new Vector2(cx - half, y));
            painter.Stroke();
            painter.BeginPath();
            painter.MoveTo(new Vector2(cx + half, y));
            painter.LineTo(new Vector2(width - 78f, y));
            painter.LineTo(new Vector2(width - 18f, 6f));
            painter.Stroke();
        }
    }
}
