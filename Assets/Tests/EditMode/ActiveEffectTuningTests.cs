using EraserWars.Cards;
using EraserWars.Core;
using EraserWars.Gameplay;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class ActiveEffectTuningTests
    {
        static MatchState Start()
        {
            var state = MatchFactory.CreateHotseat(2, RuleSet.CasualStock);
            return MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
        }

        [Test]
        public void For_combines_stat_friction_and_flick_power_from_MatchEffectRules_kind_strings()
        {
            var owner = new PlayerId(0);
            var state = Start();
            EffectSpec[] effects =
            {
                new ModifyStat("Friction", 0.5f, 1),
                new ModifyFlick(1.25f, 1.25f, 1f),
                new ModifyStat("Mass", 1.2f, 1),
            };
            state = CardResolver.Apply(state, owner, new CardId(1), effects, target: null);

            var tuning = ActiveEffectTuning.For(state, owner);
            Assert.AreEqual(1.25f, tuning.Power, 0.0001f);
            Assert.AreEqual(1.2f, tuning.Mass, 0.0001f);
            Assert.AreEqual(0.5f, tuning.Friction, 0.0001f);
        }

        [Test]
        public void ClearDust_strips_dust_desk_object_effects()
        {
            var owner = new PlayerId(0);
            var state = Start();
            state = CardResolver.Apply(state, owner, new CardId(29), new[] { new PlaceDeskObject("DustPile", 2) }, target: null);
            Assert.AreEqual(1, state.Effects.Length);
            Assert.AreEqual("object:DustPile", state.Effects[0].SpecKind);

            state = CardResolver.Apply(state, owner, new CardId(33), new[] { new PlaceDeskObject("ClearDust", 1) }, target: null);
            Assert.AreEqual(0, state.Effects.Length);
        }
    }
}