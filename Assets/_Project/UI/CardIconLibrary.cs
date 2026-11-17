using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using EraserWars.Cards;
using UnityEngine;

namespace EraserWars.UI
{
    public static class CardIconLibrary
    {
        const int IconSize = 128;
        const int PatternSize = 64;

        static readonly Regex TagPattern = new(
            "<(rect|circle|polygon)\\b([^>]*)/?>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        static readonly Regex AttrPattern = new(
            "([\\w:-]+)=\"([^\"]*)\"",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        static readonly Dictionary<CardSymbol, Texture2D> icons = new();
        static readonly Dictionary<string, Texture2D> patterns = new();

        public static Texture2D Icon(CardSymbol symbol)
        {
            if (icons.TryGetValue(symbol, out var cached) && cached != null)
            {
                return cached;
            }

            var tex = Raster(CardIconSvg.Markup(symbol), IconSize);
            tex.name = CardIconSvg.FileName(symbol);
            icons[symbol] = tex;
            return tex;
        }

        public static Texture2D Pattern(string familyKey)
        {
            var key = string.IsNullOrEmpty(familyKey) ? string.Empty : familyKey;
            if (patterns.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var tex = MakePattern(key);
            patterns[key] = tex;
            return tex;
        }

        static Texture2D Raster(string svg, int size)
        {
            var pixels = new Color32[size * size];
            var view = ViewSize(svg);
            var scale = size / view;

            foreach (Match tag in TagPattern.Matches(svg))
            {
                var kind = tag.Groups[1].Value;
                var attrs = Attributes(tag.Groups[2].Value);
                if (!attrs.TryGetValue("fill", out var fillText))
                {
                    continue;
                }

                var fill = ParseColor(fillText);
                switch (kind)
                {
                    case "circle":
                        FillCircle(
                            pixels,
                            size,
                            ScaleX(attrs, "cx", scale),
                            ScaleY(attrs, "cy", view, scale),
                            Float(attrs, "r") * scale,
                            fill);
                        break;
                    case "rect":
                        FillRect(pixels, size, attrs, view, scale, fill);
                        break;
                    case "polygon":
                        FillPolygon(pixels, size, attrs, view, scale, fill);
                        break;
                    default:
                        break;
                }
            }

            return Finish(pixels, size, "card-glyph");
        }

        static void FillRect(
            Color32[] pixels,
            int size,
            Dictionary<string, string> attrs,
            float view,
            float scale,
            Color32 fill)
        {
            var width = Float(attrs, "width") * scale;
            var height = Float(attrs, "height") * scale;
            var x = ScaleX(attrs, "x", scale);
            var y = (view - (Float(attrs, "y") + Float(attrs, "height"))) * scale;
            var radius = attrs.ContainsKey("rx") ? Float(attrs, "rx") * scale : 0f;
            FillRoundRect(pixels, size, x, y, width, height, radius, fill);
        }

        static void FillPolygon(
            Color32[] pixels,
            int size,
            Dictionary<string, string> attrs,
            float view,
            float scale,
            Color32 fill)
        {
            if (!attrs.TryGetValue("points", out var text))
            {
                return;
            }

            var pts = new List<Vector2>();
            var parts = text.Split(' ');
            for (var i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                {
                    continue;
                }

                var pair = parts[i].Split(',');
                if (pair.Length != 2)
                {
                    continue;
                }

                var x = float.Parse(pair[0], CultureInfo.InvariantCulture) * scale;
                var y = (view - float.Parse(pair[1], CultureInfo.InvariantCulture)) * scale;
                pts.Add(new Vector2(x, y));
            }

            if (pts.Count < 3)
            {
                return;
            }

            var minY = pts[0].y;
            var maxY = pts[0].y;
            for (var i = 1; i < pts.Count; i++)
            {
                minY = Mathf.Min(minY, pts[i].y);
                maxY = Mathf.Max(maxY, pts[i].y);
            }

            var yStart = Mathf.FloorToInt(minY);
            var yEnd = Mathf.CeilToInt(maxY);
            var hits = new List<float>();
            for (var py = yStart; py <= yEnd; py++)
            {
                var scan = py + 0.5f;
                hits.Clear();
                for (var i = 0; i < pts.Count; i++)
                {
                    var a = pts[i];
                    var b = pts[(i + 1) % pts.Count];
                    var crosses = (a.y <= scan && scan < b.y) || (b.y <= scan && scan < a.y);
                    if (!crosses)
                    {
                        continue;
                    }

                    hits.Add(a.x + (scan - a.y) * (b.x - a.x) / (b.y - a.y));
                }

                hits.Sort();
                for (var i = 0; i + 1 < hits.Count; i += 2)
                {
                    var start = Mathf.CeilToInt(hits[i]);
                    var end = Mathf.FloorToInt(hits[i + 1]);
                    for (var px = start; px <= end; px++)
                    {
                        Plot(pixels, size, px, py, fill);
                    }
                }
            }
        }

        static void FillCircle(Color32[] pixels, int size, float cx, float cy, float radius, Color32 fill)
        {
            var r2 = radius * radius;
            var min = Mathf.FloorToInt(cx - radius) - 1;
            var maxX = Mathf.CeilToInt(cx + radius) + 1;
            var minY = Mathf.FloorToInt(cy - radius) - 1;
            var maxY = Mathf.CeilToInt(cy + radius) + 1;
            for (var py = minY; py <= maxY; py++)
            {
                for (var px = min; px <= maxX; px++)
                {
                    var dx = px + 0.5f - cx;
                    var dy = py + 0.5f - cy;
                    if (dx * dx + dy * dy <= r2)
                    {
                        Plot(pixels, size, px, py, fill);
                    }
                }
            }
        }

        static void FillRoundRect(
            Color32[] pixels,
            int size,
            float x,
            float y,
            float width,
            float height,
            float radius,
            Color32 fill)
        {
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            var r = Mathf.Min(radius, width * 0.5f, height * 0.5f);
            var cx = x + width * 0.5f;
            var cy = y + height * 0.5f;
            var hx = width * 0.5f - r;
            var hy = height * 0.5f - r;
            var minX = Mathf.FloorToInt(x) - 1;
            var maxX = Mathf.CeilToInt(x + width) + 1;
            var minY = Mathf.FloorToInt(y) - 1;
            var maxY = Mathf.CeilToInt(y + height) + 1;
            var r2 = r * r;
            for (var py = minY; py <= maxY; py++)
            {
                for (var px = minX; px <= maxX; px++)
                {
                    var dx = Mathf.Abs(px + 0.5f - cx) - hx;
                    var dy = Mathf.Abs(py + 0.5f - cy) - hy;
                    var ox = Mathf.Max(dx, 0f);
                    var oy = Mathf.Max(dy, 0f);
                    if (ox * ox + oy * oy <= r2)
                    {
                        Plot(pixels, size, px, py, fill);
                    }
                }
            }
        }

        static Texture2D MakePattern(string familyKey)
        {
            var pixels = new Color32[PatternSize * PatternSize];
            var ink = familyKey == CardFamilies.Signature
                ? new Color32(232, 184, 72, 190)
                : new Color32(40, 22, 16, 175);
            for (var y = 0; y < PatternSize; y++)
            {
                for (var x = 0; x < PatternSize; x++)
                {
                    if (Hits(familyKey, x, y))
                    {
                        pixels[y * PatternSize + x] = ink;
                    }
                }
            }

            var tex = Finish(pixels, PatternSize, "card-pattern-" + familyKey);
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        static bool Hits(string familyKey, int x, int y)
        {
            switch (familyKey)
            {
                case CardFamilies.Kiai:
                    return (x + y) % 7 < 2;
                case CardFamilies.Homework:
                    return Mod(x - y, 7) < 2;
                case CardFamilies.Seat:
                    return (x + y) % 6 < 2;
                case CardFamilies.Stationery:
                    return Mod(x - y, 6) < 2;
                case CardFamilies.Cover:
                    return (x + y) % 8 < 2;
                case CardFamilies.Signature:
                    return (x + y) % 10 < 2 || Mod(x - y, 10) < 2;
                default:
                    return (x + y) % 8 < 2;
            }
        }

        static int Mod(int value, int divisor)
        {
            var result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        static Texture2D Finish(Color32[] pixels, int size, string name)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                name = name,
            };
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        static float ViewSize(string svg)
        {
            var match = Regex.Match(
                svg,
                "viewBox=\"[^\"]*?([0-9.]+)\\s+([0-9.]+)\\s+([0-9.]+)\\s+([0-9.]+)\"",
                RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                return 64f;
            }

            return float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        }

        static Dictionary<string, string> Attributes(string raw)
        {
            var map = new Dictionary<string, string>();
            foreach (Match match in AttrPattern.Matches(raw))
            {
                map[match.Groups[1].Value] = match.Groups[2].Value;
            }

            return map;
        }

        static float ScaleX(Dictionary<string, string> attrs, string key, float scale)
        {
            return Float(attrs, key) * scale;
        }

        static float ScaleY(Dictionary<string, string> attrs, string key, float view, float scale)
        {
            return (view - Float(attrs, key)) * scale;
        }

        static float Float(Dictionary<string, string> attrs, string key)
        {
            return float.Parse(attrs[key], CultureInfo.InvariantCulture);
        }

        static Color32 ParseColor(string text)
        {
            var hex = text.Trim();
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            var value = int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new Color32((byte)((value >> 16) & 255), (byte)((value >> 8) & 255), (byte)(value & 255), 255);
        }

        static void Plot(Color32[] pixels, int size, int x, int y, Color32 color)
        {
            if ((uint)x >= (uint)size || (uint)y >= (uint)size)
            {
                return;
            }

            pixels[y * size + x] = color;
        }
    }
}
