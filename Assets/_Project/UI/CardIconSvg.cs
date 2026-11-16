using System;
using EraserWars.Cards;

namespace EraserWars.UI
{
    public static class CardIconSvg
    {
        public static string Markup(CardSymbol symbol)
        {
            switch (symbol)
            {
                case CardSymbol.Weight:
                    return Weight;
                case CardSymbol.Grip:
                    return Grip;
                case CardSymbol.Bounce:
                    return Bounce;
                case CardSymbol.Power:
                    return Power;
                case CardSymbol.Speed:
                    return Speed;
                case CardSymbol.Flick:
                    return Flick;
                case CardSymbol.Clock:
                    return Clock;
                case CardSymbol.Sight:
                    return Sight;
                case CardSymbol.Edge:
                    return Edge;
                case CardSymbol.Center:
                    return Center;
                case CardSymbol.Extra:
                    return Extra;
                case CardSymbol.Swap:
                    return Swap;
                case CardSymbol.Pin:
                    return Pin;
                case CardSymbol.Desk:
                    return Desk;
                case CardSymbol.Shield:
                    return Shield;
                case CardSymbol.Weapon:
                    return Weapon;
                case CardSymbol.Heart:
                    return Heart;
                case CardSymbol.Star:
                    return Star;
                default:
                    throw new InvalidOperationException($"Unhandled CardSymbol {symbol}");
            }
        }

        public static string FileName(CardSymbol symbol)
        {
            switch (symbol)
            {
                case CardSymbol.Weight:
                    return "weight.svg";
                case CardSymbol.Grip:
                    return "grip.svg";
                case CardSymbol.Bounce:
                    return "bounce.svg";
                case CardSymbol.Power:
                    return "power.svg";
                case CardSymbol.Speed:
                    return "speed.svg";
                case CardSymbol.Flick:
                    return "flick.svg";
                case CardSymbol.Clock:
                    return "clock.svg";
                case CardSymbol.Sight:
                    return "sight.svg";
                case CardSymbol.Edge:
                    return "edge.svg";
                case CardSymbol.Center:
                    return "center.svg";
                case CardSymbol.Extra:
                    return "extra.svg";
                case CardSymbol.Swap:
                    return "swap.svg";
                case CardSymbol.Pin:
                    return "pin.svg";
                case CardSymbol.Desk:
                    return "desk.svg";
                case CardSymbol.Shield:
                    return "shield.svg";
                case CardSymbol.Weapon:
                    return "weapon.svg";
                case CardSymbol.Heart:
                    return "heart.svg";
                case CardSymbol.Star:
                    return "star.svg";
                default:
                    throw new InvalidOperationException($"Unhandled CardSymbol {symbol}");
            }
        }

        const string Weight =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""16"" y=""8"" width=""32"" height=""14"" rx=""4"" fill=""#24180F""/>
  <rect x=""28"" y=""22"" width=""8"" height=""18"" fill=""#24180F""/>
  <rect x=""16"" y=""40"" width=""32"" height=""14"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Grip =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""12"" y=""14"" width=""40"" height=""8"" rx=""4"" fill=""#24180F""/>
  <rect x=""12"" y=""28"" width=""40"" height=""8"" rx=""4"" fill=""#24180F""/>
  <rect x=""12"" y=""42"" width=""40"" height=""8"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Bounce =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,6.0 46.0,24.0 38.0,24.0 38.0,32.0 26.0,32.0 26.0,24.0 18.0,24.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""48"" r=""10"" fill=""#24180F""/>
</svg>
";

        const string Power =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,6.0 39.1,24.9 58.0,32.0 39.1,39.1 32.0,58.0 24.9,39.1 6.0,32.0 24.9,24.9"" fill=""#24180F""/>
</svg>
";

        const string Speed =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""6.0,16.0 28.0,32.0 6.0,48.0 14.0,48.0 36.0,32.0 14.0,16.0"" fill=""#24180F""/>
  <polygon points=""26.0,16.0 48.0,32.0 26.0,48.0 34.0,48.0 56.0,32.0 34.0,16.0"" fill=""#24180F""/>
</svg>
";

        const string Flick =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""8.0,26.0 34.0,26.0 34.0,14.0 56.0,32.0 34.0,50.0 34.0,38.0 8.0,38.0"" fill=""#24180F""/>
</svg>
";

        const string Clock =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""32"" r=""22"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""16"" fill=""#FFFCF6""/>
  <rect x=""30"" y=""18"" width=""4"" height=""16"" fill=""#24180F""/>
  <rect x=""30"" y=""30"" width=""14"" height=""4"" fill=""#24180F""/>
</svg>
";

        const string Sight =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""4.0,32.0 16.0,20.0 32.0,16.0 48.0,20.0 60.0,32.0 48.0,44.0 32.0,48.0 16.0,44.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""8"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""32"" r=""4"" fill=""#24180F""/>
</svg>
";

        const string Edge =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""48"" y=""8"" width=""8"" height=""48"" rx=""2"" fill=""#24180F""/>
  <polygon points=""8.0,26.0 28.0,26.0 28.0,18.0 44.0,32.0 28.0,46.0 28.0,38.0 8.0,38.0"" fill=""#24180F""/>
</svg>
";

        const string Center =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""32"" r=""16"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""10"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""32"" r=""4"" fill=""#24180F""/>
  <rect x=""30"" y=""6"" width=""4"" height=""10"" fill=""#24180F""/>
  <rect x=""30"" y=""48"" width=""4"" height=""10"" fill=""#24180F""/>
  <rect x=""6"" y=""30"" width=""10"" height=""4"" fill=""#24180F""/>
  <rect x=""48"" y=""30"" width=""10"" height=""4"" fill=""#24180F""/>
</svg>
";

        const string Extra =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""4.0,16.0 26.0,16.0 26.0,10.0 40.0,22.0 26.0,34.0 26.0,28.0 4.0,28.0"" fill=""#24180F""/>
  <polygon points=""20.0,36.0 42.0,36.0 42.0,30.0 56.0,42.0 42.0,54.0 42.0,48.0 20.0,48.0"" fill=""#24180F""/>
</svg>
";

        const string Swap =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""6.0,12.0 34.0,12.0 34.0,6.0 52.0,20.0 34.0,34.0 34.0,28.0 6.0,28.0"" fill=""#24180F""/>
  <polygon points=""58.0,36.0 30.0,36.0 30.0,30.0 12.0,44.0 30.0,58.0 30.0,52.0 58.0,52.0"" fill=""#24180F""/>
</svg>
";

        const string Pin =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,58.0 20.0,30.0 44.0,30.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""24"" r=""14"" fill=""#24180F""/>
  <circle cx=""32"" cy=""24"" r=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Desk =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""26"" y=""12"" width=""12"" height=""14"" rx=""2"" fill=""#24180F""/>
  <rect x=""8"" y=""26"" width=""48"" height=""8"" rx=""2"" fill=""#24180F""/>
  <rect x=""14"" y=""34"" width=""6"" height=""18"" fill=""#24180F""/>
  <rect x=""44"" y=""34"" width=""6"" height=""18"" fill=""#24180F""/>
</svg>
";

        const string Shield =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 54.0,16.0 50.0,36.0 32.0,58.0 14.0,36.0 10.0,16.0"" fill=""#24180F""/>
  <polygon points=""32.0,14.0 46.0,22.0 43.0,36.0 32.0,50.0 21.0,36.0 18.0,22.0"" fill=""#FFFCF6""/>
</svg>
";

        const string Weapon =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 42.0,20.0 22.0,20.0"" fill=""#24180F""/>
  <rect x=""24"" y=""18"" width=""16"" height=""30"" rx=""2"" fill=""#24180F""/>
  <rect x=""28"" y=""22"" width=""8"" height=""22"" fill=""#FFFCF6""/>
  <rect x=""24"" y=""46"" width=""16"" height=""8"" rx=""2"" fill=""#24180F""/>
</svg>
";

        const string Heart =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""23"" cy=""26"" r=""12"" fill=""#24180F""/>
  <circle cx=""41"" cy=""26"" r=""12"" fill=""#24180F""/>
  <polygon points=""12.0,32.0 52.0,32.0 32.0,56.0"" fill=""#24180F""/>
  <rect x=""29"" y=""20"" width=""6"" height=""20"" fill=""#FFFCF6""/>
  <rect x=""22"" y=""27"" width=""20"" height=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Star =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,8.0 38.5,25.1 56.7,26.0 42.5,37.4 47.3,55.0 32.0,45.0 16.7,55.0 21.5,37.4 7.3,26.0 25.5,25.1"" fill=""#24180F""/>
</svg>
";
    }
}
