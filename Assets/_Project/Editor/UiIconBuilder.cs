using System.IO;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class UiIconBuilder
    {
        const string IconRoot = "Assets/Art/UI/Icons";

        public static Texture2D BrandIcon => LoadOrCreate("icon_brand_eraser", MakeBrand);
        public static Texture2D Icon2P => LoadOrCreate("icon_mode_2p", () => MakeMode(2));
        public static Texture2D Icon4P => LoadOrCreate("icon_mode_4p", () => MakeMode(4));
        public static Texture2D IconCollection => LoadOrCreate("icon_collection", MakeBook);
        public static Texture2D IconOptions => LoadOrCreate("icon_options", MakeGear);
        public static Texture2D IconStock => LoadOrCreate("icon_stock", MakeHeart);
        public static Texture2D IconPass => LoadOrCreate("icon_pass", MakeSkip);

        [MenuItem("EraserWars/Build UI Icons")]
        public static void BuildAll()
        {
            EnsureFolder(IconRoot);
            _ = BrandIcon;
            _ = Icon2P;
            _ = Icon4P;
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
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillRect(tex, 16, 28, 96, 72, new Color(1f, 0.42f, 0.38f));
            FillRect(tex, 24, 36, 80, 16, new Color(1f, 0.85f, 0.75f));
            FillRect(tex, 40, 58, 48, 28, new Color(0.2f, 0.2f, 0.25f));
            StrokeRect(tex, 16, 28, 96, 72, new Color(0.15f, 0.12f, 0.18f), 4);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeMode(int players)
        {
            const int s = 96;
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            var colors = new[]
            {
                new Color(0.95f, 0.3f, 0.35f),
                new Color(0.25f, 0.45f, 0.95f),
                new Color(0.25f, 0.75f, 0.4f),
                new Color(0.95f, 0.78f, 0.2f),
            };
            for (var i = 0; i < players; i++)
            {
                var x = 14 + (i % 2) * 40;
                var y = players <= 2 ? 28 : 14 + (i / 2) * 40;
                FillRect(tex, x, y, 28, 28, colors[i]);
                StrokeRect(tex, x, y, 28, 28, new Color(0.12f, 0.1f, 0.14f), 3);
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeBook()
        {
            const int s = 96;
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillRect(tex, 20, 16, 56, 64, new Color(0.2f, 0.55f, 0.95f));
            FillRect(tex, 28, 24, 40, 48, new Color(0.95f, 0.95f, 0.9f));
            FillRect(tex, 46, 16, 6, 64, new Color(0.12f, 0.25f, 0.55f));
            StrokeRect(tex, 20, 16, 56, 64, new Color(0.1f, 0.1f, 0.14f), 3);
            tex.Apply();
            return tex;
        }

        static Texture2D MakeGear()
        {
            const int s = 96;
            var tex = New(s);
            Fill(tex, new Color(0f, 0f, 0f, 0f));
            FillCircle(tex, 48, 48, 28, new Color(0.55f, 0.58f, 0.62f));
            FillCircle(tex, 48, 48, 12, new Color(0.95f, 0.95f, 0.92f));
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * 0.25f;
                var cx = 48 + Mathf.Cos(a) * 30f;
                var cy = 48 + Mathf.Sin(a) * 30f;
                FillRect(tex, (int)cx - 6, (int)cy - 6, 12, 12, new Color(0.55f, 0.58f, 0.62f));
            }

            StrokeCircle(tex, 48, 48, 28, new Color(0.12f, 0.1f, 0.14f), 3);
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
