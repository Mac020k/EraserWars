using System.Collections.Immutable;
using EraserWars.Core;
using EraserWars.UI;
using NUnit.Framework;
using UnityEngine;

namespace EraserWars.Tests.EditMode
{
    public sealed class MatchHudModelTests
    {
        [Test]
        public void FanSpreadsAroundTheHoldAndFlattensWhenMotionIsReduced()
        {
            var left = HandFan.Pose(0, 3, selected: false, reduceMotion: false);
            var middle = HandFan.Pose(1, 3, selected: false, reduceMotion: false);
            var right = HandFan.Pose(2, 3, selected: true, reduceMotion: false);

            Assert.Less(left.Degrees, 0f);
            Assert.AreEqual(0f, middle.Degrees, 0.01f);
            Assert.Greater(right.Degrees, 0f);
            Assert.AreEqual(-left.Degrees, right.Degrees, 0.01f);
            Assert.Less(left.Overlap, 0f);
            Assert.Less(right.LiftPixels, middle.LiftPixels);

            var flat = HandFan.Pose(0, 3, selected: false, reduceMotion: true);
            Assert.AreEqual(0f, flat.Degrees, 0.01f);
            Assert.Less(flat.Overlap, 0f);
        }

        [Test]
        public void HandStaysHiddenWhenTheSeatCannotPlay()
        {
            var cards = ImmutableArray.Create(new CardId(1));
            var hands = new[] { cards, ImmutableArray<CardId>.Empty };
            var holding = MatchFactory.CreateHotseat(2, RuleSet.CasualStock, hands, null) with
            {
                Phase = Phase.CardSelect,
            };

            Assert.IsTrue(HandOffer.ShowCards(holding, cpuSeat: false));
            Assert.IsFalse(HandOffer.ShowCards(holding, cpuSeat: true));
            Assert.IsFalse(HandOffer.ShowCards(holding with { Phase = Phase.Aim }, cpuSeat: false));

            var empty = MatchFactory.CreateHotseat(2, RuleSet.CasualStock) with
            {
                Phase = Phase.CardSelect,
            };
            Assert.IsFalse(HandOffer.ShowCards(empty, cpuSeat: false));
        }

        [Test]
        public void PauseBriefListsTheRulesTheSeatsAndTheSavedTurn()
        {
            UiLocale.Set(UiLang.Ja);
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock) with { Round = 2 };
            var brief = PauseBrief.From(state, GameSettings.Defaults);

            Assert.That(brief.Rules, Does.Contain("ストック戦"));
            Assert.That(brief.Rules, Does.Contain("Round2"));
            Assert.That(brief.Rules, Does.Contain("カードを1枚"));
            Assert.That(brief.Seats, Does.Contain("P1 ストック 3"));
            Assert.That(brief.Seats, Does.Contain("P2 ストック 3"));
            Assert.That(brief.Options, Does.Contain("1ターン 20秒"));
            Assert.That(brief.Options, Does.Contain("動きを減らす: OFF"));
        }

        [Test]
        public void RingStoresTheFillAndDropsTheArcWhenTheClockStops()
        {
            var ring = new TimerRingElement();
            var color = new Color(0.2f, 0.4f, 0.9f, 1f);
            ring.SetArc(0.25f, color, ticking: true);

            Assert.AreEqual(0.25f, ring.Fill, 0.001f);
            Assert.AreEqual(color, ring.ArcColor);
            Assert.IsTrue(ring.HasArc);
            Assert.AreEqual(90f, TurnChrome.SweepDegrees(ring.Fill), 0.01f);

            ring.SetArc(1f, color, ticking: false);
            Assert.AreEqual(1f, ring.Fill, 0.001f);
            Assert.IsFalse(ring.HasArc);
        }
    }
}
