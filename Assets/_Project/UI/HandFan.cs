using EraserWars.Core;
using UnityEngine;

namespace EraserWars.UI
{
    public readonly struct HandFanPose
    {
        public HandFanPose(float degrees, float liftPixels, float overlap)
        {
            Degrees = degrees;
            LiftPixels = liftPixels;
            Overlap = overlap;
        }

        public float Degrees { get; }
        public float LiftPixels { get; }
        public float Overlap { get; }
    }

    public static class HandFan
    {
        public const float StepDegrees = 9f;
        public const float EdgeDrop = 12f;
        public const float SelectedLift = 28f;
        public const float OverlapPixels = 18f;

        public static HandFanPose Pose(int index, int count, bool selected, bool reduceMotion)
        {
            if (count <= 1)
            {
                return new HandFanPose(0f, selected ? -SelectedLift : 0f, 0f);
            }

            var center = (count - 1) * 0.5f;
            var offset = index - center;
            var degrees = reduceMotion ? 0f : offset * StepDegrees;
            var lift = -Mathf.Abs(offset) * EdgeDrop;
            if (selected)
            {
                lift -= SelectedLift;
            }

            return new HandFanPose(degrees, lift, -OverlapPixels);
        }
    }

    public static class HandOffer
    {
        public static bool ShowCards(MatchState state, bool cpuSeat)
        {
            if (state == null || cpuSeat || state.Phase != Phase.CardSelect)
            {
                return false;
            }

            var seat = state.CurrentPlayerId.Value;
            if (seat < 0 || seat >= state.Players.Length)
            {
                return false;
            }

            var hand = state.Players[seat].Hand;
            return !hand.IsDefault && hand.Length > 0;
        }
    }
}
