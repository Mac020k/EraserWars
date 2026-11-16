using System.Globalization;
using EraserWars.Core;

namespace EraserWars.Gameplay
{
    public static class ActiveEffectTuning
    {
        public static float PowerMultiplier(MatchState state, PlayerId owner)
        {
            if (state == null)
            {
                return 1f;
            }

            var mul = 1f;
            foreach (var effect in state.Effects)
            {
                if (!effect.Owner.Equals(owner))
                {
                    continue;
                }

                var kind = effect.SpecKind;
                if (kind.StartsWith("stat:Power*"))
                {
                    mul *= ParseAfterStar(kind);
                }
                else if (kind.StartsWith("flick:"))
                {
                    mul *= ParseAfterColon(kind);
                }
            }

            return mul;
        }

        public static float MassMultiplier(MatchState state, PlayerId owner)
        {
            if (state == null)
            {
                return 1f;
            }

            var mul = 1f;
            foreach (var effect in state.Effects)
            {
                if (!effect.Owner.Equals(owner) || !effect.SpecKind.StartsWith("stat:Mass*"))
                {
                    continue;
                }

                mul *= ParseAfterStar(effect.SpecKind);
            }

            return mul;
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

        static float ParseAfterStar(string kind)
        {
            var star = kind.IndexOf('*');
            var slash = kind.IndexOf('/');
            if (star < 0 || slash <= star)
            {
                return 1f;
            }

            var raw = kind.Substring(star + 1, slash - star - 1);
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : 1f;
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
