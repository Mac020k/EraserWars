using System.IO;
using UnityEditor;
using UnityEngine;

namespace Keshipin.Editor
{
    public static class CardCsvImporter
    {
        const string CsvPath = "Assets/Data/Csv/cards.csv";

        [MenuItem("Keshipin/Import Cards CSV")]
        public static void Import()
        {
            if (!File.Exists(CsvPath))
            {
                Debug.LogError($"Missing {CsvPath}");
                return;
            }

            var lines = File.ReadAllLines(CsvPath);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var cols = lines[i].Split(',');
                if (cols.Length < 9)
                {
                    continue;
                }

                var id = cols[0];
                var assetPath = $"Assets/Data/Cards/Card_{id}.asset";
                var card = AssetDatabase.LoadAssetAtPath<Keshipin.Cards.CardDefinition>(assetPath);
                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<Keshipin.Cards.CardDefinition>();
                    AssetDatabase.CreateAsset(card, assetPath);
                }

                card.IdValue = int.Parse(cols[0]);
                card.NameKey = cols[1];
                card.BuildCost = int.Parse(cols[2]);
                card.IsSignature = cols[3] == "1";
                card.MaxCopies = int.Parse(cols[4]);
                card.UsesPerMatch = int.Parse(cols[5]);
                card.CooldownTurns = int.Parse(cols[6]);
                card.DurationTurns = int.Parse(cols[7]);
                card.EffectKinds.Clear();
                card.EffectKinds.Add(cols[8]);
                EditorUtility.SetDirty(card);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Card CSV import complete.");
        }
    }
}
