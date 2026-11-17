using System;
using System.Collections.Generic;
using System.IO;
using EraserWars.Cards;
using EraserWars.UI;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class CardGlanceTests
    {
        [Test]
        public void MapsEachEffectHeadToASymbol()
        {
            Assert.AreEqual(CardSymbol.Weight, Parse("ModifyStat:Mass:1.5:1"));
            Assert.AreEqual(CardSymbol.Grip, Parse("ModifyStat:Friction:2:2"));
            Assert.AreEqual(CardSymbol.Bounce, Parse("ModifyStat:Bounciness:1.4:1"));
            Assert.AreEqual(CardSymbol.Power, Parse("ModifyStat:Power:1.25:1"));
            Assert.AreEqual(CardSymbol.Speed, Parse("ModifyStat:Speed:1.3:1"));
            Assert.AreEqual(CardSymbol.Power, Parse("ModifyStat:Luck:1:1"));
            Assert.AreEqual(CardSymbol.Flick, Parse("ModifyFlick:1:0:1"));
            Assert.AreEqual(CardSymbol.Clock, Parse("ModifyRule:OpponentTurnTime:0.5:1"));
            Assert.AreEqual(CardSymbol.Clock, Parse("ModifyRule:SelfTurnTime:1.2:1"));
            Assert.AreEqual(CardSymbol.Clock, Parse("ModifyRule:CooldownCut:0.5:1"));
            Assert.AreEqual(CardSymbol.Sight, Parse("ModifyRule:HideTrajectory:1:1"));
            Assert.AreEqual(CardSymbol.Edge, Parse("ModifyRule:PushEdge:0.4:1"));
            Assert.AreEqual(CardSymbol.Center, Parse("ModifyRule:CenterBias:0.3:1"));
            Assert.AreEqual(CardSymbol.Clock, Parse("ModifyRule:Unknown:1:1"));
            Assert.AreEqual(CardSymbol.Extra, Parse("ExtraFlick"));
            Assert.AreEqual(CardSymbol.Swap, Parse("SwapPositions"));
            Assert.AreEqual(CardSymbol.Pin, Parse("Teleport:0.5:0.5"));
            Assert.AreEqual(CardSymbol.Desk, Parse("PlaceDeskObject:RulerWall:2"));
            Assert.AreEqual(CardSymbol.Shield, Parse("GrantShield:0.5:1"));
            Assert.AreEqual(CardSymbol.Weapon, Parse("GrantWeapon:0.15:1"));
            Assert.AreEqual(CardSymbol.Sight, Parse("RevealTrajectory:2"));
            Assert.AreEqual(CardSymbol.Heart, Parse("RestoreStock:1"));
        }

        [Test]
        public void EmptyEffectsUseTheFamilyEmblem()
        {
            Assert.AreEqual(CardSymbol.Power, CardGlance.Primary(CardFamilies.Kiai, null));
            Assert.AreEqual(CardSymbol.Shield, CardGlance.Primary(CardFamilies.Cover, new[] { "not-an-effect" }));
            Assert.AreEqual(CardSymbol.Star, CardGlance.FamilyEmblem("nope"));
            Assert.AreEqual("質量", CardGlance.LabelJa(CardSymbol.Weight));
        }

        [Test]
        public void EveryCardCsvEffectResolves()
        {
            var path = Path.Combine(Application.dataPath, "Data/Csv/cards.csv");
            var lines = File.ReadAllLines(path);
            Assert.Greater(lines.Length, 1);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var cols = lines[i].Split(',');
                Assert.IsTrue(EffectSpecParser.TryParse(cols[10], out var spec), cols[1]);
                var symbol = CardGlance.SymbolFor(spec);
                Assert.AreEqual(symbol, CardGlance.Primary(cols[3], new[] { cols[10] }));
                Assert.IsFalse(string.IsNullOrEmpty(CardGlance.LabelJa(symbol)));
            }
        }

        [Test]
        public void SvgFilesMatchMarkup()
        {
            foreach (CardSymbol symbol in Enum.GetValues(typeof(CardSymbol)))
            {
                var path = Path.Combine(Application.dataPath, "_Project/UI/CardIcons", CardIconSvg.FileName(symbol));
                var file = File.ReadAllText(path).Replace("\r", "");
                var markup = CardIconSvg.Markup(symbol).Replace("\r", "");
                Assert.AreEqual(markup, file, symbol.ToString());
            }
        }

        [Test]
        public void PowerGlyphIsOpaqueAtCenterAndClearAtCorner()
        {
            var tex = CardIconLibrary.Icon(CardSymbol.Power);
            Assert.Greater(tex.GetPixel(tex.width / 2, tex.height / 2).a, 0.5f);
            Assert.Less(tex.GetPixel(0, 0).a, 0.05f);
        }

        [Test]
        public void PatternTileIsCoarsePointFilteredAndMatchesItsElement()
        {
            var tex = CardIconLibrary.Pattern(CardFamilies.Kiai);
            Assert.GreaterOrEqual(tex.width, 96);
            Assert.LessOrEqual(tex.width, 128);
            Assert.AreEqual(tex.width, tex.height);
            Assert.AreEqual(FilterMode.Point, tex.filterMode);
            Assert.AreEqual(TextureWrapMode.Repeat, tex.wrapMode);
            Assert.GreaterOrEqual(LongestClearRun(tex), 16);

            var element = new UnityEngine.UIElements.VisualElement();
            UiImageUtil.ApplyPattern(element, tex);
            var size = element.style.backgroundSize.value;
            Assert.AreEqual(tex.width, size.x.value, 0.01f);
            Assert.AreEqual(tex.height, size.y.value, 0.01f);
        }

        [Test]
        public void PrimaryIconsDivergeWhenEffectsDiffer()
        {
            var path = Path.Combine(Application.dataPath, "Data/Csv/cards.csv");
            var lines = File.ReadAllLines(path);
            var effects = new HashSet<string>();
            var symbolKeys = new Dictionary<CardSymbol, HashSet<string>>();
            var hashes = new Dictionary<string, int>();
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var cols = lines[i].Split(',');
                var effect = cols[10];
                var key = CardGlance.IconKey(cols[3], new[] { effect });
                Assert.AreEqual(effect, key, cols[1]);
                effects.Add(effect);
                Assert.IsTrue(EffectSpecParser.TryParse(effect, out var spec), cols[1]);
                var symbol = CardGlance.SymbolFor(spec);
                if (!symbolKeys.TryGetValue(symbol, out var keys))
                {
                    keys = new HashSet<string>();
                    symbolKeys[symbol] = keys;
                }

                keys.Add(key);
                if (hashes.ContainsKey(key))
                {
                    continue;
                }

                var tex = CardIconLibrary.Glyph(key, symbol);
                hashes[key] = HashTexture(tex);
                Assert.IsTrue(CardIconSvg.TryMarkup(key, out var markup), key);
                var file = File.ReadAllText(Path.Combine(
                    Application.dataPath,
                    "_Project/UI/CardIcons",
                    CardIconSvg.EffectFileName(key))).Replace("\r", "");
                Assert.AreEqual(markup.Replace("\r", ""), file, key);
            }

            Assert.GreaterOrEqual(effects.Count, 50);
            Assert.GreaterOrEqual(symbolKeys[CardSymbol.Clock].Count, 4);
            Assert.GreaterOrEqual(symbolKeys[CardSymbol.Weapon].Count, 4);
            Assert.GreaterOrEqual(symbolKeys[CardSymbol.Desk].Count, 4);
            Assert.AreEqual(hashes.Count, new HashSet<int>(hashes.Values).Count);
        }

        static int LongestClearRun(Texture2D tex)
        {
            var pixels = tex.GetPixels32();
            var best = 0;
            var run = 0;
            for (var x = 0; x < tex.width; x++)
            {
                if (pixels[x].a == 0)
                {
                    run++;
                    if (run > best)
                    {
                        best = run;
                    }
                }
                else
                {
                    run = 0;
                }
            }

            return best;
        }

        static int HashTexture(Texture2D tex)
        {
            var pixels = tex.GetPixels32();
            var hash = 17;
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                hash = unchecked(hash * 31 + pixel.r + (pixel.g << 8) + (pixel.b << 16) + (pixel.a << 24) + i);
            }

            return hash;
        }

        static CardSymbol Parse(string raw)
        {
            Assert.IsTrue(EffectSpecParser.TryParse(raw, out var spec));
            return CardGlance.SymbolFor(spec);
        }
    }
}
