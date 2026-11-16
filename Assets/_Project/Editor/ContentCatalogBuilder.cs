using System.Collections.Generic;
using System.Linq;
using EraserWars.Cards;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class ContentCatalogBuilder
    {
        public const string CatalogPath = "Assets/Resources/EraserWars/ContentCatalog.asset";

        public static ContentCatalog LoadOrCreate()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/EraserWars");
            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);
            if (catalog != null)
            {
                return catalog;
            }

            catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }

        public static void UpsertCards(IReadOnlyList<CardDefinition> cards)
        {
            var catalog = LoadOrCreate();
            catalog.Cards = cards.Where(c => c != null).OrderBy(c => c.IdValue).ToList();
            EditorUtility.SetDirty(catalog);
        }

        public static void UpsertErasers(IReadOnlyList<EraserDefinition> erasers)
        {
            var catalog = LoadOrCreate();
            catalog.Erasers = erasers.Where(e => e != null).OrderBy(e => e.name).ToList();
            EditorUtility.SetDirty(catalog);
        }

        public static void UpsertCharacters(IReadOnlyList<CharacterDefinition> characters)
        {
            var catalog = LoadOrCreate();
            catalog.Characters = characters.Where(c => c != null).OrderBy(c => c.IdValue).ToList();
            EditorUtility.SetDirty(catalog);
        }

        [MenuItem("EraserWars/Refresh Content Catalog")]
        public static void RefreshFromDisk()
        {
            var catalog = LoadOrCreate();
            catalog.Cards = LoadAll<CardDefinition>("Assets/Data/Cards")
                .OrderBy(c => c.IdValue)
                .ToList();
            catalog.Erasers = LoadAll<EraserDefinition>("Assets/Data/Erasers")
                .OrderBy(e => e.name)
                .ToList();
            catalog.Characters = LoadAll<CharacterDefinition>("Assets/Data/Characters")
                .OrderBy(c => c.IdValue)
                .ToList();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"Content catalog refreshed: {catalog.Cards.Count} cards, {catalog.Characters.Count} characters, {catalog.Erasers.Count} erasers.");
        }

        static List<T> LoadAll<T>(string folder) where T : Object
        {
            var list = new List<T>();
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return list;
            }

            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    list.Add(asset);
                }
            }

            return list;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
