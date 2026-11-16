using System.Collections.Generic;
using System.Globalization;
using System.IO;
using EraserWars.Cards;
using UnityEditor;
using UnityEngine;

namespace EraserWars.Editor
{
    public static class RosterCsvImporter
    {
        const string ErasersCsv = "Assets/Data/Csv/erasers.csv";
        const string CharactersCsv = "Assets/Data/Csv/characters.csv";
        const string ErasersDir = "Assets/Data/Erasers";
        const string CharactersDir = "Assets/Data/Characters";
        const string CardsDir = "Assets/Data/Cards";

        [MenuItem("EraserWars/Import Roster CSV")]
        public static void Import()
        {
            EnsureFolder(ErasersDir);
            EnsureFolder(CharactersDir);

            var erasers = ImportErasers();
            var characters = ImportCharacters(erasers);
            ContentCatalogBuilder.UpsertErasers(erasers);
            ContentCatalogBuilder.UpsertCharacters(characters);
            AssetDatabase.SaveAssets();
            Debug.Log($"Roster CSV import complete ({erasers.Count} erasers, {characters.Count} characters).");
        }

        [MenuItem("EraserWars/Import All Content CSV")]
        public static void ImportAll()
        {
            CardCsvImporter.Import();
            Import();
            ContentCatalogBuilder.RefreshFromDisk();
            Debug.Log("All content CSV import complete.");
        }

        static List<EraserDefinition> ImportErasers()
        {
            var list = new List<EraserDefinition>();
            if (!File.Exists(ErasersCsv))
            {
                Debug.LogError($"Missing {ErasersCsv}");
                return list;
            }

            var lines = File.ReadAllLines(ErasersCsv);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var cols = lines[i].Split(',');
                if (cols.Length < 13)
                {
                    continue;
                }

                var id = cols[0];
                var assetPath = $"{ErasersDir}/Eraser_{id}.asset";
                var eraser = AssetDatabase.LoadAssetAtPath<EraserDefinition>(assetPath);
                if (eraser == null)
                {
                    eraser = ScriptableObject.CreateInstance<EraserDefinition>();
                    AssetDatabase.CreateAsset(eraser, assetPath);
                }

                eraser.NameKey = cols[1];
                eraser.Mass = ParseFloat(cols[2]);
                eraser.Friction = ParseFloat(cols[3]);
                eraser.Bounciness = ParseFloat(cols[4]);
                eraser.SizeMeters = new Vector3(ParseFloat(cols[5]), ParseFloat(cols[6]), ParseFloat(cols[7]));
                eraser.Power = ParseFloat(cols[8]);
                eraser.Speed = ParseFloat(cols[9]);
                eraser.DeckCostCap = ParseInt(cols[10]);
                eraser.HandLimit = ParseInt(cols[11]);
                eraser.PassiveKey = cols[12];
                EditorUtility.SetDirty(eraser);
                list.Add(eraser);
            }

            return list;
        }

        static List<CharacterDefinition> ImportCharacters(List<EraserDefinition> erasers)
        {
            var list = new List<CharacterDefinition>();
            if (!File.Exists(CharactersCsv))
            {
                Debug.LogError($"Missing {CharactersCsv}");
                return list;
            }

            var eraserById = new Dictionary<int, EraserDefinition>();
            foreach (var eraser in erasers)
            {
                var name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(eraser));
                if (name.StartsWith("Eraser_") && int.TryParse(name.Substring("Eraser_".Length), out var eid))
                {
                    eraserById[eid] = eraser;
                }
            }

            var lines = File.ReadAllLines(CharactersCsv);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var cols = lines[i].Split(',');
                if (cols.Length < 8)
                {
                    continue;
                }

                var id = cols[0];
                var assetPath = $"{CharactersDir}/Character_{id}.asset";
                var character = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(assetPath);
                if (character == null)
                {
                    character = ScriptableObject.CreateInstance<CharacterDefinition>();
                    AssetDatabase.CreateAsset(character, assetPath);
                }

                character.IdValue = ParseInt(cols[0]);
                character.NameKey = cols[1];
                character.DisplayNameJa = cols[2];
                character.ArchetypeKey = cols[3];
                character.UnlockedByDefault = cols[4] == "1";
                var eraserId = ParseInt(cols[5]);
                eraserById.TryGetValue(eraserId, out var eraser);
                character.Eraser = eraser;
                character.SignatureA = LoadCard(cols[6]);
                character.SignatureB = LoadCard(cols[7]);
                EditorUtility.SetDirty(character);
                list.Add(character);
            }

            return list;
        }

        static CardDefinition LoadCard(string idRaw)
        {
            if (string.IsNullOrWhiteSpace(idRaw))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<CardDefinition>($"{CardsDir}/Card_{idRaw.Trim()}.asset");
        }

        static float ParseFloat(string raw) =>
            float.Parse(raw, CultureInfo.InvariantCulture);

        static int ParseInt(string raw) =>
            int.Parse(raw, CultureInfo.InvariantCulture);

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
