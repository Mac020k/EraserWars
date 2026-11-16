using System;

namespace EraserWars.UI
{
    public static class JoinCode
    {
        public const int Length = 6;
        public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Generate(Random random)
        {
            random ??= new Random();
            var chars = new char[Length];
            for (var i = 0; i < Length; i++)
            {
                chars[i] = Alphabet[random.Next(Alphabet.Length)];
            }

            return new string(chars);
        }

        public static string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var trimmed = raw.Trim().ToUpperInvariant();
            if (trimmed.Length != Length)
            {
                return null;
            }

            for (var i = 0; i < trimmed.Length; i++)
            {
                if (Alphabet.IndexOf(trimmed[i]) < 0)
                {
                    return null;
                }
            }

            return trimmed;
        }
    }
}
