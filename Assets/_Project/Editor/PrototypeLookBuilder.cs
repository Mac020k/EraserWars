using System.IO;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class PrototypeLookBuilder
    {
        const string ArtRoot = "Assets/Art/Prototype";
        const string MatRoot = ArtRoot + "/Materials";
        const string TexRoot = ArtRoot + "/Textures";

        static readonly Color DeskTint = new(0.74f, 0.52f, 0.30f);
        static readonly Color FloorTint = new(0.78f, 0.55f, 0.32f);
        static readonly Color WallTint = new(0.95f, 0.89f, 0.75f);

        public static Material DeskMaterial => LoadOrCreateMat("Mat_DeskWood", DeskTint, "Tex_DeskGrain");
        public static Material DeskLegMaterial => LoadOrCreateMat("Mat_DeskLeg", new Color(0.45f, 0.48f, 0.52f), null);
        public static Material FloorMaterial => LoadOrCreateMat("Mat_Floor", FloorTint, "Tex_Linoleum");
        public static Material WallMaterial => LoadOrCreateMat("Mat_Wall", WallTint, "Tex_WallPlaster");
        public static Material ChalkboardMaterial => LoadOrCreateMat("Mat_Chalkboard", new Color(0.10f, 0.36f, 0.25f), null);
        public static Material BoardGreenMaterial => LoadOrCreateMat("Mat_BoardGreen", new Color(0.30f, 0.50f, 0.32f), null);
        public static Material CurtainMaterial => LoadOrCreateMat("Mat_Curtain", new Color(0.97f, 0.62f, 0.74f), null);
        public static Material TrimMaterial => LoadOrCreateMat("Mat_Trim", new Color(0.45f, 0.30f, 0.18f), null);
        public static Material CorkMaterial => LoadOrCreateMat("Mat_Cork", new Color(0.72f, 0.55f, 0.32f), "Tex_Cork");
        public static Material SchoolyardMaterial => LoadOrCreateMat("Mat_Schoolyard", Color.white, "Tex_Schoolyard");
        public static Material YardGroundMaterial => LoadOrCreateMat("Mat_YardGround", Color.white, "Tex_YardGround");
        public static Material DoorMaterial => LoadOrCreateMat("Mat_Door", new Color(0.70f, 0.62f, 0.48f), null);
        public static Material LockerMaterial => LoadOrCreateMat("Mat_Locker", new Color(0.62f, 0.70f, 0.74f), null);
        public static Material GlassMaterial => LoadOrCreateMat("Mat_Glass", new Color(0.75f, 0.88f, 0.95f, 0.35f), null);
        public static Material EraserBaseMaterial => LoadOrCreateMat("Mat_EraserBase", new Color(0.92f, 0.55f, 0.62f), "Tex_Rubber");

        public static readonly Color[] PlayerColors =
        {
            new(0.92f, 0.28f, 0.35f),
            new(0.25f, 0.45f, 0.95f),
            new(0.25f, 0.75f, 0.40f),
            new(0.95f, 0.78f, 0.20f),
        };

        public static void EnsureAssets(bool forceRefreshTextures = false)
        {
            EnsureFolder("Assets/Art");
            EnsureFolder(ArtRoot);
            EnsureFolder(MatRoot);
            EnsureFolder(TexRoot);
            if (forceRefreshTextures)
            {
                RefreshTexture("Tex_DeskGrain", MakeWood(DeskTint));
                RefreshTexture("Tex_Linoleum", MakeFloorboards(FloorTint));
                RefreshTexture("Tex_ClassroomTile", MakeTiles(new Color(0.48f, 0.58f, 0.42f)));
                RefreshTexture("Tex_WallPlaster", MakePlaster(WallTint));
                RefreshTexture("Tex_Cork", MakeCork(new Color(0.72f, 0.55f, 0.32f)));
                RefreshTexture("Tex_Schoolyard", MakeSchoolyard());
                RefreshTexture("Tex_YardGround", MakeYardGround());
                RefreshTexture("Tex_Rubber", MakeRubber(new Color(0.92f, 0.55f, 0.62f)));
            }

            _ = DeskMaterial;
            _ = DeskLegMaterial;
            _ = FloorMaterial;
            _ = WallMaterial;
            _ = ChalkboardMaterial;
            _ = TrimMaterial;
            _ = CorkMaterial;
            _ = SchoolyardMaterial;
            _ = YardGroundMaterial;
            _ = BoardGreenMaterial;
            _ = CurtainMaterial;
            _ = DoorMaterial;
            _ = LockerMaterial;
            _ = GlassMaterial;
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

            if (name is "Mat_Schoolyard" or "Mat_YardGround")
            {
                var unlit = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlit != null && mat.shader != unlit)
                {
                    mat.shader = unlit;
                }
            }

            Texture2D tex = null;
            if (!string.IsNullOrEmpty(textureName))
            {
                tex = LoadOrCreateTexture(textureName, color);
            }

            // Texture already carries the tint, so a non-white base color would multiply it twice.
            SetLitColor(mat, tex != null ? Color.white : color);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                    var scale = name.Contains("Floor") || name.Contains("Linoleum")
                        ? new Vector2(5f, 5f)
                        : name.Contains("YardGround")
                            ? new Vector2(3f, 8f)
                            : name.Contains("Cork")
                                ? new Vector2(2f, 2f)
                                : Vector2.one;
                    mat.SetTextureScale("_BaseMap", scale);
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
                "Tex_DeskGrain" or "Tex_Wood" => MakeWood(baseColor),
                "Tex_Linoleum" => MakeFloorboards(baseColor),
                "Tex_ClassroomTile" or "Tex_FloorTile" => MakeTiles(baseColor),
                "Tex_WallPlaster" => MakePlaster(baseColor),
                "Tex_Cork" => MakeCork(baseColor),
                "Tex_Schoolyard" => MakeSchoolyard(),
                "Tex_YardGround" => MakeYardGround(),
                "Tex_Rubber" => MakeRubber(baseColor),
                _ => MakeSolid(baseColor),
            };

            WriteTexture(path, tex);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static void RefreshTexture(string name, Texture2D tex)
        {
            var path = $"{TexRoot}/{name}.png";
            WriteTexture(path, tex);
        }

        static void WriteTexture(string path, Texture2D tex)
        {
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Path.GetFullPath(path), bytes);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        static Texture2D MakeWood(Color baseColor)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var grain = Mathf.Sin((x + y * 0.12f) * 0.28f) * 0.07f;
                    var streak = Mathf.Abs(Mathf.Sin(x * 0.09f + Mathf.PerlinNoise(x * 0.03f, y * 0.01f))) * 0.14f;
                    var knot = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.06f;
                    var c = baseColor * (0.82f + grain + streak + knot);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeFloorboards(Color baseColor)
        {
            const int size = 256;
            const int plankWidth = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var plank = x / plankWidth;
                    var tint = 0.88f + 0.16f * Mathf.Repeat(Mathf.Sin(plank * 12.9898f) * 43758.5453f, 1f);
                    var grain = Mathf.PerlinNoise(x * 0.06f, (y + plank * 57) * 0.015f);
                    var c = baseColor * tint * (0.92f + grain * 0.16f);
                    if (x % plankWidth < 2)
                    {
                        c *= 0.7f;
                    }

                    var jointOffset = (plank * 83) % size;
                    if ((y + jointOffset) % 128 < 2)
                    {
                        c *= 0.75f;
                    }

                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeCork(Color baseColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var n = Mathf.PerlinNoise(x * 0.2f, y * 0.2f);
                    var c = Color.Lerp(baseColor * 0.85f, baseColor * 1.1f, n);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeSchoolyard()
        {
            // 3:1 backdrop. Bottom strip matches YardGround, then tree line, then sky with clouds.
            const int width = 512;
            const int height = 256;
            var grass = new Color(0.40f, 0.68f, 0.30f);
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            {
                var v = y / (float)height;
                for (var x = 0; x < width; x++)
                {
                    Color c;
                    var treeTop = 0.16f + 0.14f * Mathf.PerlinNoise(x * 0.025f, 4f) + 0.04f * Mathf.PerlinNoise(x * 0.2f, 9f);
                    if (v < 0.10f)
                    {
                        c = grass * (0.92f + 0.16f * Mathf.PerlinNoise(x * 0.08f, y * 0.08f));
                    }
                    else if (v < treeTop)
                    {
                        var leaf = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                        c = Color.Lerp(new Color(0.16f, 0.44f, 0.20f), new Color(0.38f, 0.68f, 0.28f), leaf);
                    }
                    else
                    {
                        var sky = Color.Lerp(new Color(0.72f, 0.88f, 1f), new Color(0.40f, 0.68f, 0.96f), (v - 0.3f) / 0.7f);
                        var cloud = Mathf.Clamp01((Mathf.PerlinNoise(x * 0.012f + 7f, y * 0.03f + 2f) - 0.58f) * 4f);
                        c = Color.Lerp(sky, Color.white, cloud * 0.9f);
                    }

                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeYardGround()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var n = Mathf.PerlinNoise(x * 0.07f, y * 0.07f);
                    var c = Color.Lerp(new Color(0.34f, 0.62f, 0.26f), new Color(0.48f, 0.74f, 0.34f), n);
                    c *= 0.94f + 0.12f * Mathf.PerlinNoise(x * 0.4f, y * 0.4f);
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakeTiles(Color baseColor)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var tileX = x / 32;
                    var tileY = y / 32;
                    var tile = (tileX + tileY) % 2 == 0;
                    var line = x % 32 <= 1 || y % 32 <= 1;
                    var wear = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.08f;
                    var c = tile ? baseColor : baseColor * 0.9f;
                    c *= 0.94f + wear;
                    if (line)
                    {
                        c *= 0.62f;
                    }

                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D MakePlaster(Color baseColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var n = Mathf.PerlinNoise(x * 0.07f, y * 0.07f) * 0.08f;
                    var c = baseColor * (0.96f + n);
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
                var matName = mat.name;
                var smooth = matName.Contains("Chalk") ? 0.08f : matName.Contains("Trim") ? 0.18f : 0.22f;
                mat.SetFloat("_Smoothness", smooth);
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
