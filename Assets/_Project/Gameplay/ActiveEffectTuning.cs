using System.Collections.Generic;
using System.Globalization;
using EraserWars.Core;

namespace EraserWars.Gameplay
{
    public readonly struct EffectModifiers
    {
        public static readonly EffectModifiers Identity = new(1f, 1f, 1f, 1f, 1f);

        public readonly float Power;
        public readonly float Mass;
        public readonly float Friction;
        public readonly float Bounciness;
        public readonly float Speed;

        public EffectModifiers(float power, float mass, float friction, float bounciness, float speed)
        {
            Power = power;
            Mass = mass;
            Friction = friction;
            Bounciness = bounciness;
            Speed = speed;
        }

        public override string ToString() =>
            $"power={Power:0.##} mass={Mass:0.##} friction={Friction:0.##} bounce={Bounciness:0.##} speed={Speed:0.##}";
    }

    public static class ActiveEffectTuning
    {
        public static EffectModifiers For(MatchState state, PlayerId owner)
        {
            if (state == null)
            {
                return EffectModifiers.Identity;
            }

            float power = 1f, mass = 1f, friction = 1f, bounciness = 1f, speed = 1f;
            foreach (var effect in state.Effects)
            {
                if (!effect.Owner.Equals(owner))
                {
                    continue;
                }

                var kind = effect.SpecKind;
                if (kind.StartsWith("flick:"))
                {
                    power *= ParseAfterColon(kind);
                    continue;
                }

                if (!TryParseStat(kind, out var stat, out var multiplier))
                {
                    continue;
                }

                switch (stat)
                {
                    case "Power":
                        power *= multiplier;
                        break;
                    case "Mass":
                        mass *= multiplier;
                        break;
                    case "Friction":
                        friction *= multiplier;
                        break;
                    case "Bounciness":
                        bounciness *= multiplier;
                        break;
                    case "Speed":
                        speed *= multiplier;
                        break;
                }
            }

            return new EffectModifiers(power, mass, friction, bounciness, speed);
        }

        public static void ApplyTo(MatchState state, IReadOnlyList<EraserBody> erasers)
        {
            for (var i = 0; i < erasers.Count; i++)
            {
                var eraser = erasers[i];
                if (eraser != null)
                {
                    eraser.ApplyModifiers(For(state, new PlayerId(eraser.PlayerIndex)));
                }
            }
        }

        public static bool HasExtraFlick(MatchState state, PlayerId owner)
        {
            if (state == null)
            {
                return false;
            }

            foreach (var effect in state.Effects)
            {
                if (effect.Owner.Equals(owner) && effect.SpecKind == "extra-flick")
                {
                    return true;
                }
            }

            return false;
        }

        // SpecKind for ModifyStat is "stat:<Stat>*<Multiplier>/<Turns>".
        static bool TryParseStat(string kind, out string stat, out float multiplier)
        {
            stat = null;
            multiplier = 1f;
            if (!kind.StartsWith("stat:"))
            {
                return false;
            }

            var star = kind.IndexOf('*');
            var slash = kind.IndexOf('/');
            if (star < 0 || slash <= star)
            {
                return false;
            }

            stat = kind.Substring(5, star - 5);
            var raw = kind.Substring(star + 1, slash - star - 1);
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out multiplier);
        }

        static float ParseAfterColon(string kind)
        {
            var colon = kind.IndexOf(':');
            if (colon < 0 || colon + 1 >= kind.Length)
            {
                return 1f;
            }

            var raw = kind.Substring(colon + 1);
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : 1f;
        }
    }
}
