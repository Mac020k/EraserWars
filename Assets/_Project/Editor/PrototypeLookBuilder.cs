using System.IO;
using UnityEditor;
using UnityEngine;

namespace Keshipin.Editor
{
    public static class PrototypeLookBuilder
    {
        const string ArtRoot = "Assets/Art/Prototype";
        const string MatRoot = ArtRoot + "/Materials";
        const string TexRoot = ArtRoot + "/Textures";

        public static Material DeskMaterial => LoadOrCreateMat("Mat_DeskWood", new Color(0.62f, 0.42f, 0.24f), "Tex_Wood");
        public static Material DeskLegMaterial => LoadOrCreateMat("Mat_DeskLeg", new Color(0.28f, 0.18f, 0.10f), null);
        public static Material FloorMaterial => LoadOrCreateMat("Mat_Floor", new Color(0.55f, 0.62f, 0.48f), "Tex_FloorTile");
        public static Material EraserBaseMaterial => LoadOrCreateMat("Mat_EraserBase", new Color(0.92f, 0.55f, 0.62f), "Tex_Rubber");

        public static readonly Color[] PlayerColors =
        {
            new(0.92f, 0.28f, 0.35f),
            new(0.25f, 0.45f, 0.95f),
            new(0.25f, 0.75f, 0.40f),
            new(0.95f, 0.78f, 0.20f),
        };

        public static void EnsureAssets()
        {
            EnsureFolder("Assets/Art");
            EnsureFolder(ArtRoot);
            EnsureFolder(MatRoot);
            EnsureFolder(TexRoot);
            _ = DeskMaterial;
            _ = DeskLegMaterial;
            _ = FloorMaterial;
            _ = EraserBaseMaterial;
            AssetDatabase.SaveAssets();
        }

        public static void ApplyRendererColor(GameObject go, Material shared, Color? tint = null)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null || shared == null)
            {
                return;
            }

            var instance = Object.Instantiate(shared);
            instance.name = shared.name + "_Instance";
            if (tint.HasValue)
            {
                if (instance.HasProperty("_BaseColor"))
                {
                    instance.SetColor("_BaseColor", tint.Value);
                }

                if (instance.HasProperty("_Color"))
                {
                    instance.SetColor("_Color", tint.Value);
                }
            }

            renderer.sharedMaterial = instance;
        }

        public static void ApplySharedMaterial(GameObject go, Material shared)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && shared != null)
            {
                renderer.sharedMaterial = shared;
            }
        }

        static Material LoadOrCreateMat(string name, Color color, string textureName)
        {
            EnsureFolder(MatRoot);
            EnsureFolder(TexRoot);
            var path = $"{MatRoot}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                             ?? Shader.Find("Standard");
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }

            Texture2D tex = null;
            if (!string.IsNullOrEmpty(textureName))
            {
                tex = LoadOrCreateTexture(textureName, color);
            }

            SetLitColor(mat, color);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                }

                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", tex);
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Texture2D LoadOrCreateTexture(string name, Color baseColor)
        {
            var path = $"{TexRoot}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
            {
                return existing;
            }

            var tex = name switch
            {
                "Tex_Wood" => MakeWood(baseColor),
                "Tex_FloorTile" => MakeTiles(baseColor),
                "Tex_Rubber" => MakeRubber(baseColor),
                _ => MakeSolid(baseColor),
            };

            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Path.GetFullPath(path), bytes);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D MakeWood(Color baseColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var grain = Mathf.Sin((x + y * 0.15f) * 0.35f) * 0.08f;
                    var streak = Mathf.Abs(Mathf.Sin(x * 0.12f)) * 0.12f;
                    var c = baseColor * (0.85f + grain + streak);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeTiles(Color baseColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var tile = ((x / 16) + (y / 16)) % 2 == 0;
                    var line = x % 16 == 0 || y % 16 == 0;
                    var c = tile ? baseColor : baseColor * 0.88f;
                    if (line)
                    {
                        c *= 0.7f;
                    }

                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeRubber(Color baseColor)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var n = Mathf.PerlinNoise(x * 0.18f, y * 0.18f) * 0.15f;
                    var c = baseColor * (0.9f + n);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeSolid(Color color)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        static void SetLitColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.25f);
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
