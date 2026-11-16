using System.Collections.Generic;
using System.Globalization;
using System.IO;
using EraserWars.Cards;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class CardCsvImporter
    {
        const string CsvPath = "Assets/Data/Csv/cards.csv";
        const string CardsDir = "Assets/Data/Cards";

        [MenuItem("EraserWars/Import Cards CSV")]
        public static void Import()
        {
            EnsureFolder(CardsDir);
            if (!File.Exists(CsvPath))
            {
                Debug.LogError($"Missing {CsvPath}");
                return;
            }

            var imported = new List<CardDefinition>();
            var lines = File.ReadAllLines(CsvPath);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var cols = SplitCsv(lines[i]);
                if (cols.Length < 11)
                {
                    Debug.LogWarning($"Skip card row {i}: expected 11 columns, got {cols.Length}");
                    continue;
                }

                var id = cols[0];
                var assetPath = $"{CardsDir}/Card_{id}.asset";
                var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(assetPath);
                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<CardDefinition>();
                    AssetDatabase.CreateAsset(card, assetPath);
                }

                card.IdValue = int.Parse(cols[0], CultureInfo.InvariantCulture);
                card.NameKey = cols[1];
                card.DisplayNameJa = cols[2];
                card.FamilyKey = cols[3];
                card.BuildCost = int.Parse(cols[4], CultureInfo.InvariantCulture);
                card.IsSignature = cols[5] == "1";
                card.MaxCopies = int.Parse(cols[6], CultureInfo.InvariantCulture);
                card.UsesPerMatch = int.Parse(cols[7], CultureInfo.InvariantCulture);
                card.CooldownTurns = int.Parse(cols[8], CultureInfo.InvariantCulture);
                card.DurationTurns = int.Parse(cols[9], CultureInfo.InvariantCulture);
                card.EffectKinds.Clear();
                card.EffectKinds.Add(cols[10]);
                EditorUtility.SetDirty(card);
                imported.Add(card);
            }

            ContentCatalogBuilder.UpsertCards(imported);
            AssetDatabase.SaveAssets();
            Debug.Log($"Card CSV import complete ({imported.Count} cards).");
        }

        static string[] SplitCsv(string line) => line.Split(',');

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
