using UnityEngine;

namespace EraserWars.Persistence
{
    public readonly record struct CareerXpReceipt(int Gained, int Total);

    public static class CareerXp
    {
        public const string TotalKey = "eraserwars.xp";
        public const int GainPerFinishedMatch = 10;

        public static int ReadTotal()
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(TotalKey, 0));
        }

        public static CareerXpReceipt GrantFinishedMatch()
        {
            var before = ReadTotal();
            var total = before + GainPerFinishedMatch;
            PlayerPrefs.SetInt(TotalKey, total);
            PlayerPrefs.Save();
            return new CareerXpReceipt(GainPerFinishedMatch, total);
        }
    }
}
