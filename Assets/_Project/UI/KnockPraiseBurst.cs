using System;
using EraserWars.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace EraserWars.UI
{
    public enum PraiseSwatch
    {
        Paper,
        Gold,
        Seat,
    }

    public readonly struct PraisePiece
    {
        public PraisePiece(float x, float y, float degrees, float alpha, PraiseSwatch swatch)
        {
            X = x;
            Y = y;
            Degrees = degrees;
            Alpha = alpha;
            Swatch = swatch;
        }

        public float X { get; }
        public float Y { get; }
        public float Degrees { get; }
        public float Alpha { get; }
        public PraiseSwatch Swatch { get; }
    }

    public readonly struct PraiseMark
    {
        public PraiseMark(float y, float scale, float alpha)
        {
            Y = y;
            Scale = scale;
            Alpha = alpha;
        }

        public float Y { get; }
        public float Scale { get; }
        public float Alpha { get; }
    }

    public static class KnockPraiseBurst
    {
        public const float DurationSeconds = 0.62f;
        public const int PieceCount = 14;

        const float FadeStartSeconds = 0.34f;
        const float MinSpeed = 120f;
        const float SpeedSpread = 100f;
        const float FanRadians = 1.1f;
        const float Gravity = 160f;

        static readonly PraiseSwatch[] SwatchCycle =
        {
            PraiseSwatch.Paper,
            PraiseSwatch.Gold,
            PraiseSwatch.Seat,
        };

        public static bool TryPraiseSeat(int victimSeat, int knockerSeat, out int seat)
        {
            if (knockerSeat < 0 || knockerSeat == victimSeat)
            {
                seat = -1;
                return false;
            }

            seat = knockerSeat;
            return true;
        }

        public static void WritePieces(float elapsedSeconds, int seed, Span<PraisePiece> pieces)
        {
            var visible = elapsedSeconds >= 0f && elapsedSeconds < DurationSeconds;
            var t = visible ? elapsedSeconds : 0f;
            var alpha = visible ? Fade(elapsedSeconds) : 0f;
            for (var i = 0; i < pieces.Length; i++)
            {
                var fan = (Hash01(seed, i, 0) - 0.5f) * (FanRadians * 2f);
                var speed = MinSpeed + Hash01(seed, i, 1) * SpeedSpread;
                var vx = Mathf.Sin(fan) * speed;
                var vy = -Mathf.Cos(fan) * speed;
                var x = vx * t;
                var y = vy * t + Gravity * t * t;
                var degrees = (Hash01(seed, i, 2) - 0.5f) * 540f * t;
                pieces[i] = new PraisePiece(x, y, degrees, alpha, SwatchCycle[i % SwatchCycle.Length]);
            }
        }

        public static PraiseMark WriteMark(float elapsedSeconds)
        {
            var visible = elapsedSeconds >= 0f && elapsedSeconds < DurationSeconds;
            if (!visible)
            {
                return new PraiseMark(0f, 1f, 0f);
            }

            var pop = Mathf.Clamp01(elapsedSeconds / 0.12f);
            return new PraiseMark(-28f - 16f * pop, 0.7f + 0.42f * pop, Fade(elapsedSeconds));
        }

        static float Fade(float elapsedSeconds)
        {
            if (elapsedSeconds <= FadeStartSeconds)
            {
                return 1f;
            }

            return 1f - (elapsedSeconds - FadeStartSeconds) / (DurationSeconds - FadeStartSeconds);
        }

        static float Hash01(int seed, int index, int channel)
        {
            unchecked
            {
                var n = (uint)seed * 747796405u
                    + (uint)index * 2891336453u
                    + (uint)channel * 374761393u
                    + 1402946731u;
                n = (n ^ (n >> 16)) * 2246822519u;
                n ^= n >> 13;
                return (n & 0xFFFFFF) / 16777215f;
            }
        }
    }

    public sealed class KnockPraiseView
    {
        const float ScrapWidth = 9f;
        const float ScrapHeight = 15f;
        const float FallbackX = 160f;
        const float FallbackY = 72f;

        static readonly Color Paper = new(1f, 0.98f, 0.92f, 1f);
        static readonly Color Gold = new(1f, 0.84f, 0.2f, 1f);

        readonly VisualElement layer;
        readonly Func<int, VisualElement> anchorForSeat;
        readonly VisualElement[] scraps = new VisualElement[KnockPraiseBurst.PieceCount];
        readonly PraisePiece[] sample = new PraisePiece[KnockPraiseBurst.PieceCount];
        readonly Label mark;
        int seat = -1;
        int seed;
        int serial;
        float elapsed;
        bool playing;

        public KnockPraiseView(VisualElement layer, Func<int, VisualElement> anchorForSeat)
        {
            this.layer = layer;
            this.anchorForSeat = anchorForSeat;
            layer.pickingMode = PickingMode.Ignore;
            layer.style.position = Position.Absolute;
            layer.style.left = 0f;
            layer.style.right = 0f;
            layer.style.top = 0f;
            layer.style.bottom = 0f;
            layer.style.overflow = Overflow.Visible;
            layer.style.display = DisplayStyle.None;

            for (var i = 0; i < scraps.Length; i++)
            {
                var scrap = new VisualElement { pickingMode = PickingMode.Ignore };
                scrap.AddToClassList("praise-scrap");
                scrap.style.position = Position.Absolute;
                scrap.style.width = ScrapWidth;
                scrap.style.height = ScrapHeight;
                scrap.style.transformOrigin = new TransformOrigin(
                    new Length(50, LengthUnit.Percent),
                    new Length(50, LengthUnit.Percent),
                    0f);
                layer.Add(scrap);
                scraps[i] = scrap;
            }

            mark = new Label("ナイス!") { name = "praise-mark", pickingMode = PickingMode.Ignore };
            mark.AddToClassList("praise-mark");
            mark.style.position = Position.Absolute;
            mark.style.fontSize = 30;
            mark.style.unityFontStyleAndWeight = FontStyle.Bold;
            mark.style.color = Color.white;
            mark.style.unityTextOutlineWidth = 3f;
            mark.style.whiteSpace = WhiteSpace.NoWrap;
            mark.style.transformOrigin = new TransformOrigin(
                new Length(50, LengthUnit.Percent),
                new Length(100, LengthUnit.Percent),
                0f);
            layer.Add(mark);
        }

        public void Play(int praisedSeat)
        {
            serial++;
            Play(praisedSeat, unchecked(praisedSeat * 97 + serial * 7919));
        }

        public void Play(int praisedSeat, int burstSeed)
        {
            if (praisedSeat < 0)
            {
                return;
            }

            seat = praisedSeat;
            seed = burstSeed;
            elapsed = 0f;
            playing = true;
            layer.style.display = DisplayStyle.Flex;
            Apply(0f);
        }

        public void Tick(float deltaSeconds)
        {
            if (!playing)
            {
                return;
            }

            elapsed += Mathf.Max(0f, deltaSeconds);
            if (elapsed >= KnockPraiseBurst.DurationSeconds)
            {
                playing = false;
                seat = -1;
                layer.style.display = DisplayStyle.None;
                return;
            }

            Apply(elapsed);
        }

        void Apply(float time)
        {
            KnockPraiseBurst.WritePieces(time, seed, sample);
            var origin = Origin();
            var seatColor = SeatPalette.Praise(seat);
            for (var i = 0; i < scraps.Length; i++)
            {
                var piece = sample[i];
                var scrap = scraps[i];
                scrap.style.left = origin.x - ScrapWidth * 0.5f + piece.X;
                scrap.style.top = origin.y - ScrapHeight * 0.5f + piece.Y;
                scrap.style.rotate = new Rotate(Angle.Degrees(piece.Degrees));
                scrap.style.opacity = piece.Alpha;
                scrap.style.backgroundColor = ColorFor(piece.Swatch, seatColor);
            }

            var word = KnockPraiseBurst.WriteMark(time);
            mark.style.left = origin.x;
            mark.style.top = origin.y + word.Y;
            mark.style.translate = new Translate(
                new Length(-50, LengthUnit.Percent),
                new Length(0f, LengthUnit.Pixel));
            mark.style.scale = new Scale(new Vector2(word.Scale, word.Scale));
            mark.style.opacity = word.Alpha;
            mark.style.unityTextOutlineColor = seatColor;
        }

        Vector2 Origin()
        {
            var anchor = anchorForSeat?.Invoke(seat);
            if (anchor != null)
            {
                var bounds = anchor.worldBound;
                if (bounds.width > 1f && bounds.height > 1f)
                {
                    var local = (Vector2)bounds.center - (Vector2)layer.worldBound.position;
                    return local;
                }
            }

            return new Vector2(FallbackX, FallbackY);
        }

        static Color ColorFor(PraiseSwatch swatch, Color seatColor)
        {
            switch (swatch)
            {
                case PraiseSwatch.Paper:
                    return Paper;
                case PraiseSwatch.Gold:
                    return Gold;
                case PraiseSwatch.Seat:
                    return seatColor;
                default:
                    return UnknownSwatch(swatch);
            }
        }

        static Color UnknownSwatch(PraiseSwatch swatch)
        {
            throw new ArgumentOutOfRangeException(nameof(swatch), swatch, null);
        }
    }
}
