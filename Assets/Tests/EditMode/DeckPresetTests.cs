using System;
using System.Collections.Generic;
using EraserWars.Cards;
using EraserWars.Persistence;
using EraserWars.UI;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class DeckPresetTests
    {
        readonly List<UnityEngine.Object> owned = new();
        bool hadPresets;
        string presets;

        [SetUp]
        public void ClearPresets()
        {
            hadPresets = PlayerPrefs.HasKey(DeckPresets.Key);
            presets = PlayerPrefs.GetString(DeckPresets.Key, string.Empty);
            PlayerPrefs.DeleteKey(DeckPresets.Key);
        }

        [TearDown]
        public void RestorePresets()
        {
            if (hadPresets)
            {
                PlayerPrefs.SetString(DeckPresets.Key, presets);
            }
            else
            {
                PlayerPrefs.DeleteKey(DeckPresets.Key);
            }

            PlayerPrefs.Save();
            for (var i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
        }

        [Test]
        public void FiveSlotsRoundTripThroughPlayerPrefs()
        {
            for (var slot = 0; slot < DeckPresets.SlotsPerCharacter; slot++)
            {
                var ids = new int[slot + 1];
                for (var i = 0; i < ids.Length; i++)
                {
                    ids[i] = 10 + slot;
                }

                DeckPresets.SaveCards(4, slot, ids);
            }

            DeckPresets.SetActiveSlot(4, 3);
            DeckPresets.SetName(4, 3, "遠足");
            var raw = PlayerPrefs.GetString(DeckPresets.Key, string.Empty);

            Assert.IsTrue(raw.Contains("\"CharacterId\":4"));
            Assert.AreEqual(3, DeckPresets.GetActiveSlot(4));
            Assert.AreEqual("遠足", DeckPresets.GetName(4, 3));
            Assert.AreEqual("プリセット1", DeckPresets.GetName(4, 0));
            for (var slot = 0; slot < DeckPresets.SlotsPerCharacter; slot++)
            {
                var loaded = DeckPresets.LoadCards(4, slot);
                Assert.AreEqual(slot + 1, loaded.Count);
                Assert.AreEqual(10 + slot, loaded[0]);
            }
        }

        [Test]
        public void CharactersKeepSeparateActiveSlots()
        {
            DeckPresets.SaveCards(1, 0, new[] { 7 });
            DeckPresets.SaveCards(2, 1, new[] { 8, 8 });
            DeckPresets.SetActiveSlot(1, 4);

            Assert.AreEqual(4, DeckPresets.GetActiveSlot(1));
            Assert.AreEqual(0, DeckPresets.GetActiveSlot(2));
            Assert.AreEqual(7, DeckPresets.LoadCards(1, 0)[0]);
            Assert.AreEqual(2, DeckPresets.LoadCards(2, 1).Count);
            Assert.IsFalse(DeckPresets.HasSaved(1, 1));
            Assert.IsFalse(DeckPresets.HasSaved(2, 0));
        }

        [Test]
        public void SlotIndexClampsAndABlankNameUsesTheDefault()
        {
            DeckPresets.SetActiveSlot(6, 9);
            Assert.AreEqual(4, DeckPresets.GetActiveSlot(6));
            DeckPresets.SetActiveSlot(6, -2);
            Assert.AreEqual(0, DeckPresets.GetActiveSlot(6));
            DeckPresets.SaveCards(6, 8, new[] { 3 });
            Assert.IsTrue(DeckPresets.HasSaved(6, 4));
            Assert.AreEqual(3, DeckPresets.LoadCards(6, 4)[0]);
            DeckPresets.SetName(6, 0, "  ");
            Assert.AreEqual("プリセット1", DeckPresets.GetName(6, 0));
            Assert.AreEqual("プリセット5", DeckPresets.DefaultName(4));
        }

        [Test]
        public void MissingOrBrokenPrefsReadAsAnEmptyBook()
        {
            Assert.IsFalse(DeckPresets.HasSaved(5, 0));
            Assert.AreEqual(0, DeckPresets.LoadCards(5, 0).Count);
            Assert.AreEqual(0, DeckPresets.GetActiveSlot(5));
            PlayerPrefs.SetString(DeckPresets.Key, "{nope");
            Assert.IsFalse(DeckPresets.HasSaved(5, 0));
            DeckPresets.SaveCards(-1, 0, new[] { 1 });
            Assert.IsFalse(PlayerPrefs.GetString(DeckPresets.Key, string.Empty).Contains("\"CharacterId\":-1"));
        }

        [Test]
        public void SavedEmptyPresetDoesNotGrowTheStarterPackage()
        {
            var catalog = Catalog(Hero(3), Card("保存", 9, 1, 1), Card("始め", 1, 1, 1), Card("中", 5, 1, 1), Card("終", 12, 1, 1));
            DeckPresets.SaveCards(3, 0, Array.Empty<int>());
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = catalog.FindCharacter(3);

            loadout.EnsureDefaults(catalog);

            Assert.AreEqual(0, loadout.Commons[0].Count);
        }

        [Test]
        public void EmptySeatLoadsTheActivePreset()
        {
            var catalog = Catalog(Hero(3), Card("保存", 9, 1, 1), Card("始め", 1, 1, 1));
            DeckPresets.SaveCards(3, 2, new[] { 9 });
            DeckPresets.SetActiveSlot(3, 2);
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = catalog.FindCharacter(3);

            loadout.EnsureDefaults(catalog);

            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreEqual(9, loadout.Commons[0][0].IdValue);
        }

        [Test]
        public void SeatChangeCopiesALegalPresetAndLeavesAnIllegalOne()
        {
            var starter = Card("始め", 1, 1, 1);
            var saved = Card("保存", 9, 1, 1);
            var heavy = Card("重い", 4, 1, 1);
            var catalog = Catalog(Hero(3), Hero(8), starter, saved, heavy);
            var loadout = new HotseatLoadout();
            loadout.Seats[0] = catalog.FindCharacter(3);
            loadout.EnsureDefaults(catalog);
            Assert.AreEqual(1, loadout.Commons[0].Count);

            DeckPresets.SaveCards(8, 0, new[] { 9 });
            Assert.IsTrue(loadout.TryAssignUnlocked(0, catalog.FindCharacter(8)));
            loadout.ApplyActivePreset(0, catalog);
            Assert.AreEqual(9, loadout.Commons[0][0].IdValue);

            DeckPresets.SaveCards(3, 0, new[] { 4, 4 });
            Assert.IsTrue(loadout.TryAssignUnlocked(0, catalog.FindCharacter(3)));
            loadout.ApplyActivePreset(0, catalog);
            Assert.AreEqual(1, loadout.Commons[0].Count);
            Assert.AreEqual(9, loadout.Commons[0][0].IdValue);
        }

        ContentCatalog Catalog(CharacterDefinition first, params UnityEngine.Object[] rest)
        {
            var catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            owned.Add(catalog);
            catalog.Characters = new List<CharacterDefinition>();
            catalog.Cards = new List<CardDefinition>();
            Add(catalog, first);
            for (var i = 0; i < rest.Length; i++)
            {
                Add(catalog, rest[i]);
            }

            return catalog;
        }

        void Add(ContentCatalog catalog, UnityEngine.Object asset)
        {
            switch (asset)
            {
                case CharacterDefinition character:
                    catalog.Characters.Add(character);
                    return;
                case CardDefinition card:
                    catalog.Cards.Add(card);
                    return;
                default:
                    throw new InvalidOperationException($"Unhandled catalog asset '{asset}'.");
            }
        }

        CharacterDefinition Hero(int id)
        {
            var eraser = ScriptableObject.CreateInstance<EraserDefinition>();
            owned.Add(eraser);
            eraser.DeckCostCap = 8;
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            owned.Add(character);
            character.IdValue = id;
            character.DisplayNameJa = "P" + id;
            character.Eraser = eraser;
            character.UnlockedByDefault = true;
            return character;
        }

        CardDefinition Card(string name, int id, int cost, int maxCopies)
        {
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            owned.Add(card);
            card.IdValue = id;
            card.DisplayNameJa = name;
            card.BuildCost = cost;
            card.MaxCopies = maxCopies;
            card.FamilyKey = CardFamilies.Kiai;
            return card;
        }
    }
}
