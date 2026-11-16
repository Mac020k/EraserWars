namespace EraserWars.Gameplay
{
    public readonly struct CardCutIn
    {
        public const float SlashSeconds = 0.2f;
        public const float NormalHoldSeconds = 0.4f;
        public const float SignatureHoldSeconds = 0.8f;
        public const float SlashTravelPixels = 720f;

        public int CardId { get; }
        public int Seat { get; }
        public string DisplayName { get; }
        public bool Signature { get; }
        public float DurationSeconds { get; }
        public float RemainingSeconds { get; }

        public float ElapsedSeconds => DurationSeconds - RemainingSeconds;

        public bool IsFinished => RemainingSeconds <= 0f;

        public static CardCutIn Begin(int seat, int cardId, string displayName, bool signature)
        {
            var duration = SlashSeconds * 2f + (signature ? SignatureHoldSeconds : NormalHoldSeconds);
            var name = string.IsNullOrWhiteSpace(displayName) ? "カード" : displayName;
            return new CardCutIn(cardId, seat, name, signature, duration, duration);
        }

        public CardCutIn Advance(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return this;
            }

            var remaining = RemainingSeconds - deltaSeconds;
            if (remaining < 0f)
            {
                remaining = 0f;
            }

            return new CardCutIn(CardId, Seat, DisplayName, Signature, DurationSeconds, remaining);
        }

        public CardCutInPose Pose() => CardCutInPose.At(ElapsedSeconds, DurationSeconds);

        CardCutIn(
            int cardId,
            int seat,
            string displayName,
            bool signature,
            float durationSeconds,
            float remainingSeconds)
        {
            CardId = cardId;
            Seat = seat;
            DisplayName = displayName;
            Signature = signature;
            DurationSeconds = durationSeconds;
            RemainingSeconds = remainingSeconds;
        }
    }

    public readonly struct CardCutInPose
    {
        public CardCutInPose(float slidePixels, float opacity)
        {
            SlidePixels = slidePixels;
            Opacity = opacity;
        }

        public float SlidePixels { get; }
        public float Opacity { get; }

        public static CardCutInPose At(float elapsedSeconds, float durationSeconds)
        {
            var slash = CardCutIn.SlashSeconds;
            var travel = CardCutIn.SlashTravelPixels;
            if (durationSeconds <= 0f)
            {
                return new CardCutInPose(travel, 0f);
            }

            var elapsed = elapsedSeconds < 0f ? 0f : elapsedSeconds;
            if (elapsed > durationSeconds)
            {
                elapsed = durationSeconds;
            }

            if (elapsed < slash)
            {
                var enter = elapsed / slash;
                return new CardCutInPose(Lerp(-travel, 0f, enter), enter);
            }

            var exitStart = durationSeconds - slash;
            if (exitStart < slash)
            {
                exitStart = slash;
            }

            if (elapsed < exitStart)
            {
                return new CardCutInPose(0f, 1f);
            }

            var span = durationSeconds - exitStart;
            var leave = span <= 0f ? 1f : (elapsed - exitStart) / span;
            if (leave > 1f)
            {
                leave = 1f;
            }

            return new CardCutInPose(Lerp(0f, travel, leave), 1f - leave);
        }

        static float Lerp(float from, float to, float t) => from + (to - from) * t;
    }
}
