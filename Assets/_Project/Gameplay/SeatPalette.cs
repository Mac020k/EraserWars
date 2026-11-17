using UnityEngine;

namespace EraserWars.Gameplay
{
    public static class SeatPalette
    {
        public const string PrefKey = "eraserwars.seatPalette";
        public const string HudClass = "cb-seats";
        public const int Count = 4;

        static readonly Color[] StandardBody =
        {
            new(0.92f, 0.28f, 0.35f, 1f),
            new(0.25f, 0.45f, 0.95f, 1f),
            new(0.25f, 0.75f, 0.40f, 1f),
            new(0.95f, 0.78f, 0.20f, 1f),
        };

        static readonly Color[] StandardUi =
        {
            new(1f, 120f / 255f, 160f / 255f, 1f),
            new(70f / 255f, 130f / 255f, 240f / 255f, 1f),
            new(70f / 255f, 190f / 255f, 100f / 255f, 1f),
            new(1f, 160f / 255f, 50f / 255f, 1f),
        };

        // IBM Design Library categorical palette, in seat order.
        // 0 #648FFF, 1 #DC267F, 2 #FE6100, 3 #FFB000.
        static readonly Color[] Accessible =
        {
            new(100f / 255f, 143f / 255f, 1f, 1f),
            new(220f / 255f, 38f / 255f, 127f / 255f, 1f),
            new(254f / 255f, 97f / 255f, 0f, 1f),
            new(1f, 176f / 255f, 0f, 1f),
        };

        public static bool ColorblindPreferred()
        {
            return PlayerPrefs.GetInt(PrefKey, 0) != 0;
        }

        public static Color Body(int seat) => Pick(seat, StandardBody, ColorblindPreferred());

        public static Color Praise(int seat) => Ui(seat, ColorblindPreferred());

        public static Color Ui(int seat, bool colorblind) => Pick(seat, StandardUi, colorblind);

        public static string DebugName(int seat)
        {
            if (seat < 0 || seat >= Count)
            {
                return $"p{seat}";
            }

            if (ColorblindPreferred())
            {
                return seat switch
                {
                    0 => "blue",
                    1 => "magenta",
                    2 => "orange",
                    3 => "gold",
                    _ => $"p{seat}",
                };
            }

            return seat switch
            {
                0 => "red",
                1 => "blue",
                2 => "green",
                3 => "yellow",
                _ => $"p{seat}",
            };
        }

        static Color Pick(int seat, Color[] standard, bool colorblind)
        {
            var index = Mathf.Abs(seat) % Count;
            return colorblind ? Accessible[index] : standard[index];
        }
    }
}
