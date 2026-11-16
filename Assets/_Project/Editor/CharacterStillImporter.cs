using System.IO;
using EraserWars.Cards;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class CharacterStillImporter
    {
        const string StillsDir = "Assets/Art/Characters/Stills";

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
                EditorUtility.SetDirty(character);
                assigned++;
            }

            ContentCatalogBuilder.RefreshFromDisk();
            AssetDatabase.SaveAssets();
            Debug.Log($"[EraserWars] Import Character Stills assigned={assigned}");
        }
    }
}
