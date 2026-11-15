using System.IO;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class UiIconBuilder
    {
        const string IconRoot = "Assets/Art/UI/Icons";

        public static Texture2D BrandIcon => LoadOrCreate("icon_brand_eraser_v2", MakeBrand);
        public static Texture2D Icon2P => LoadOrCreate("icon_mode_2p_v2", () => MakeMode(2));
        public static Texture2D Icon4P => LoadOrCreate("icon_mode_4p_v2", () => MakeMode(4));
        public static Texture2D IconDeck => LoadOrCreate("icon_deck", MakeDeck);
        public static Texture2D IconCollection => LoadOrCreate("icon_collection_v2", MakeFolder);
        public static Texture2D IconOptions => LoadOrCreate("icon_options_v2", MakeGear);
        public static Texture2D IconStock => LoadOrCreate("icon_stock", MakeHeart);
        public static Texture2D IconPass => LoadOrCreate("icon_pass", MakeSkip);

        [MenuItem("EraserWars/Build UI Icons")]
        public static void BuildAll()
        {
            EnsureFolder(IconRoot);
            _ = BrandIcon;
            _ = Icon2P;
            _ = Icon4P;
            _ = IconDeck;
            _ = IconCollection;
            _ = IconOptions;
            _ = IconStock;
            _ = IconPass;
            AssetDatabase.SaveAssets();
            Debug.Log($"Built UI icons under {IconRoot}");
        }

        static Texture2D LoadOrCreate(string name, System.Func<Texture2D> factory)
        {
            EnsureFolder(IconRoot);
            var path = $"{IconRoot}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
            {
                return existing;
            }

            var tex = factory();
            File.WriteAllBytes(Path.GetFullPath(path), tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D MakeBrand()
        {
            const int s = 128;
            var ink = new Color(0.15f, 0.12f, 0.18f);
            var pink = new Color(1f, 0.55f, 0.7f);
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillRoundRect(tex, 22, 4, 84, 120, 16, Color.white);
            FillRoundRect(tex, 28, 10, 72, 108, 12, ink);
            FillRoundRect(tex, 31, 13, 66, 102, 10, pink);
            FillRect(tex, 31, 13, 66, 34, new Color(0.25f, 0.5f, 0.95f));
            FillRect(tex, 31, 47, 66, 3, ink);
            FillCircle(tex, 51, 88, 6, ink);
            FillCircle(tex, 77, 88, 6, ink);
            FillCircle(tex, 53, 90, 2, Color.white);
            FillCircle(tex, 79, 90, 2, Color.white);
            FillCircle(tex, 42, 76, 5, new Color(1f, 0.35f, 0.5f));
            FillCircle(tex, 86, 76, 5, new Color(1f, 0.35f, 0.5f));
            FillCircle(tex, 64, 74, 7, ink);
            FillRect(tex, 56, 74, 16, 8, pink);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeMode(int players)
        {
            const int s = 96;
            var ink = new Color(0.2f, 0.2f, 0.24f);
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillCircle(tex, 24, 34, 16, ink);
            FillCircle(tex, 72, 34, 16, ink);
            FillRoundRect(tex, 8, 36, 80, 40, 18, ink);
            FillRect(tex, 20, 52, 16, 5, Color.white);
            FillRect(tex, 25, 47, 6, 15, Color.white);
            FillCircle(tex, 62, 62, 4, new Color(0.35f, 0.6f, 1f));
            FillCircle(tex, 78, 62, 4, new Color(1f, 0.85f, 0.25f));
            FillCircle(tex, 70, 69, 4, new Color(0.35f, 0.85f, 0.5f));
            FillCircle(tex, 70, 55, 4, new Color(1f, 0.4f, 0.5f));
            var colors = new[]
            {
                new Color(0.95f, 0.3f, 0.35f),
                new Color(0.25f, 0.45f, 0.95f),
                new Color(0.25f, 0.75f, 0.4f),
                new Color(0.95f, 0.78f, 0.2f),
            };
            for (var i = 0; i < players; i++)
            {
                FillCircle(tex, 48 + (int)((i - (players - 1) * 0.5f) * 14f), 16, 5, colors[i]);
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeDeck()
        {
            const int s = 96;
            var ink = new Color(0.12f, 0.1f, 0.14f);
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillRoundRect(tex, 12, 14, 44, 62, 6, ink);
            FillRoundRect(tex, 15, 17, 38, 56, 4, Color.white);
            FillDiamond(tex, 34, 45, 10, 14, new Color(0.25f, 0.45f, 0.95f));
            FillRoundRect(tex, 40, 20, 44, 62, 6, ink);
            FillRoundRect(tex, 43, 23, 38, 56, 4, Color.white);
            FillDiamond(tex, 62, 51, 10, 14, new Color(0.95f, 0.3f, 0.35f));
            tex.Apply();
            return tex;
        }

        static Texture2D MakeFolder()
        {
            const int s = 96;
            var ink = new Color(0.1f, 0.1f, 0.14f);
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillRoundRect(tex, 8, 56, 36, 20, 5, new Color(0.9f, 0.65f, 0.15f));
            FillRoundRect(tex, 8, 14, 80, 54, 8, ink);
            FillRoundRect(tex, 11, 17, 74, 48, 6, new Color(1f, 0.82f, 0.3f));
            tex.Apply();
            return tex;
        }

        static Texture2D MakeGear()
        {
            const int s = 96;
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillCircle(tex, 48, 48, 28, new Color(0.2f, 0.5f, 0.95f));
            FillCircle(tex, 48, 48, 12, new Color(1f, 0.99f, 0.96f));
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * 0.25f;
                var cx = 48 + Mathf.Cos(a) * 30f;
                var cy = 48 + Mathf.Sin(a) * 30f;
                FillRect(tex, (int)cx - 6, (int)cy - 6, 12, 12, new Color(0.2f, 0.5f, 0.95f));
            }

            StrokeCircle(tex, 48, 48, 12, new Color(0.15f, 0.35f, 0.75f), 2);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeHeart()
        {
            const int s = 64;
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillCircle(tex, 22, 26, 12, new Color(1f, 0.28f, 0.4f));
            FillCircle(tex, 42, 26, 12, new Color(1f, 0.28f, 0.4f));
            FillRect(tex, 14, 28, 36, 18, new Color(1f, 0.28f, 0.4f));
            tex.Apply();
            return tex;
        }

        static Texture2D MakeSkip()
        {
            const int s = 64;
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            for (var y = 12; y < 52; y++)
            {
                for (var x = 14; x < 40; x++)
                {
                    if (x - 14 < (y - 12) * 0.55f && x - 14 > (52 - y) * 0.15f)
                    {
                        tex.SetPixel(x, y, new Color(1f, 0.85f, 0.2f));
                    }
                }
            }

            FillRect(tex, 40, 16, 8, 32, new Color(1f, 0.85f, 0.2f));
            tex.Apply();
            return tex;
        }

        static Texture2D New(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        }

        static void Fill(Texture2D tex, Color c)
        {
            var pixels = new Color[tex.width * tex.height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = c;
            }

            tex.SetPixels(pixels);
        }

        static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
        {
            for (var yy = y; yy < y + h; yy++)
            {
                for (var xx = x; xx < x + w; xx++)
                {
                    if (xx >= 0 && yy >= 0 && xx < tex.width && yy < tex.height)
                    {
                        tex.SetPixel(xx, yy, c);
                    }
                }
            }
        }

        static void FillRoundRect(Texture2D tex, int x, int y, int w, int h, int r, Color c)
        {
            for (var yy = y; yy < y + h; yy++)
            {
                for (var xx = x; xx < x + w; xx++)
                {
                    var dx = Mathf.Max(x + r - xx, 0, xx - (x + w - 1 - r));
                    var dy = Mathf.Max(y + r - yy, 0, yy - (y + h - 1 - r));
                    if (dx * dx + dy * dy <= r * r
                        && xx >= 0 && yy >= 0 && xx < tex.width && yy < tex.height)
                    {
                        tex.SetPixel(xx, yy, c);
                    }
                }
            }
        }

        static void FillDiamond(Texture2D tex, int cx, int cy, int rx, int ry, Color c)
        {
            for (var yy = cy - ry; yy <= cy + ry; yy++)
            {
                for (var xx = cx - rx; xx <= cx + rx; xx++)
                {
                    if (Mathf.Abs(xx - cx) * ry + Mathf.Abs(yy - cy) * rx <= rx * ry
                        && xx >= 0 && yy >= 0 && xx < tex.width && yy < tex.height)
                    {
                        tex.SetPixel(xx, yy, c);
                    }
                }
            }
        }

        static void StrokeRect(Texture2D tex, int x, int y, int w, int h, Color c, int t)
        {
            FillRect(tex, x, y, w, t, c);
            FillRect(tex, x, y + h - t, w, t, c);
            FillRect(tex, x, y, t, h, c);
            FillRect(tex, x + w - t, y, t, h, c);
        }

        static void FillCircle(Texture2D tex, int cx, int cy, int r, Color c)
        {
            var r2 = r * r;
            for (var y = cy - r; y <= cy + r; y++)
            {
                for (var x = cx - r; x <= cx + r; x++)
                {
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2
                        && x >= 0 && y >= 0 && x < tex.width && y < tex.height)
                    {
                        tex.SetPixel(x, y, c);
                    }
                }
            }
        }

        static void StrokeCircle(Texture2D tex, int cx, int cy, int r, Color c, int t)
        {
            for (var i = 0; i < t; i++)
            {
                var rr = r - i;
                var r2 = rr * rr;
                var inner = (rr - 1) * (rr - 1);
                for (var y = cy - rr; y <= cy + rr; y++)
                {
                    for (var x = cx - rr; x <= cx + rr; x++)
                    {
                        var d = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                        if (d <= r2 && d >= inner && x >= 0 && y >= 0 && x < tex.width && y < tex.height)
                        {
                            tex.SetPixel(x, y, c);
                        }
                    }
                }
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
