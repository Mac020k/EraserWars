using System;
using EraserWars.Core;

namespace EraserWars.Cards
{
    public abstract record EffectSpec;

    public sealed record ModifyStat(string Stat, float Multiplier, int Turns) : EffectSpec;

    public sealed record ModifyFlick(float PowerMultiplier, float SwayAmplitudeMultiplier, float SwayFrequencyMultiplier) : EffectSpec;

    public sealed record ModifyRule(string RuleKey, float Value, int Turns) : EffectSpec;

    public sealed record ExtraFlick : EffectSpec;

    public sealed record SwapPositions : EffectSpec;

    public sealed record Teleport(float DeskNormalizedX, float DeskNormalizedZ) : EffectSpec;

    public sealed record PlaceDeskObject(string PrefabKey, int Turns) : EffectSpec;

    public sealed record GrantShield(float ImpulseAbsorb, int Hits) : EffectSpec;

    public sealed record GrantWeapon(float BonusImpulseOnHit, int Hits) : EffectSpec;

    public sealed record RevealTrajectory(int CollisionDepth) : EffectSpec;

    public static class EffectSpecExhaustive
    {
        public static string Describe(EffectSpec spec) => spec switch
        {
            ModifyStat s => $"stat:{s.Stat}*{s.Multiplier}/{s.Turns}",
            ModifyFlick f => $"flick:{f.PowerMultiplier}",
            ModifyRule r => $"rule:{r.RuleKey}",
            ExtraFlick => "extra-flick",
            SwapPositions => "swap",
            Teleport t => $"teleport:{t.DeskNormalizedX},{t.DeskNormalizedZ}",
            PlaceDeskObject o => $"object:{o.PrefabKey}",
            GrantShield g => $"shield:{g.ImpulseAbsorb}",
            GrantWeapon w => $"weapon:{w.BonusImpulseOnHit}",
            RevealTrajectory r => $"traj:{r.CollisionDepth}",
            _ => throw new InvalidOperationException($"Unhandled EffectSpec {spec.GetType().Name}"),
        };
    }
}
