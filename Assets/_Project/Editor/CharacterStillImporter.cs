using System.IO;
using EraserWars.Cards;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class CharacterStillImporter
    {
        const string StillsDir = "Assets/Art/Characters/Stills";
        const string FacesDir = "Assets/Art/Characters/Faces";

        static readonly (int id, string file)[] Map =
        {
            (0, "char_still_00_akaboshi.jpg"),
            (1, "char_still_01_todoroki.jpg"),
            (2, "char_still_02_minazuki.jpg"),
            (3, "char_still_03_hanasaki.jpg"),
            (4, "char_still_04_alex.jpg"),
            (5, "char_still_05_ohori.jpg"),
            (6, "char_still_06_kuroe.jpg"),
            (7, "char_still_07_mei.jpg"),
            (8, "char_still_08_kageyama.jpg"),
            (9, "char_still_09_ozora.jpg"),
            (10, "char_still_10_hinata.jpg"),
            (11, "char_still_11_gou.jpg"),
            (12, "char_still_12_iroha.jpg"),
            (13, "char_still_13_atom.jpg"),
            (14, "char_still_14_meshiko.jpg"),
            (15, "char_still_15_mike.jpg"),
            (16, "char_still_16_pika.jpg"),
            (17, "char_still_17_kiyora.jpg"),
            (18, "char_still_18_sora.jpg"),
            (19, "char_still_19_nyan.jpg"),
        };

        [MenuItem("EraserWars/Import Character Stills")]
        public static void Import()
        {
            var assigned = 0;
            foreach (var (id, file) in Map)
            {
                var path = $"{StillsDir}/{file}";
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"Missing still {path}");
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                var character = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    $"Assets/Data/Characters/Character_{id}.asset");
                if (sprite == null || character == null)
                {
                    Debug.LogWarning($"Could not assign still for character {id}");
                    continue;
                }

                character.Portrait = sprite;
                character.FacePortrait = ImportFace(file);
                EditorUtility.SetDirty(character);
                assigned++;
            }

            ContentCatalogBuilder.RefreshFromDisk();
            AssetDatabase.SaveAssets();
            Debug.Log($"[EraserWars] Import Character Stills assigned={assigned}");
        }

        static Sprite ImportFace(string stillFile)
        {
            var faceFile = stillFile.Replace("char_still_", "char_face_").Replace(".jpg", ".png");
            var path = $"{FacesDir}/{faceFile}";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Missing baked face {path}. Run python tools/bake_faces.py");
                return null;
            }

            AssetDatabase.ImportAsset(path);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
