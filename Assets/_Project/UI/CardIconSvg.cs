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

        public static bool TryMarkup(string iconKey, out string markup)
        {
            switch (iconKey)
            {
                case "ModifyStat:Mass:1.5:1":
                    markup = Fx0;
                    return true;
                case "ModifyStat:Friction:2:2":
                    markup = Fx1;
                    return true;
                case "ModifyStat:Friction:0.5:1":
                    markup = Fx2;
                    return true;
                case "PlaceDeskObject:RulerWall:2":
                    markup = Fx3;
                    return true;
                case "ModifyFlick:1:0:1":
                    markup = Fx4;
                    return true;
                case "ModifyFlick:1.4:1:2":
                    markup = Fx5;
                    return true;
                case "ModifyRule:OpponentTurnTime:0.5:1":
                    markup = Fx6;
                    return true;
                case "SwapPositions":
                    markup = Fx7;
                    return true;
                case "GrantShield:0.5:1":
                    markup = Fx8;
                    return true;
                case "GrantWeapon:0.15:1":
                    markup = Fx9;
                    return true;
                case "PlaceDeskObject:DustPatch:2":
                    markup = Fx10;
                    return true;
                case "RevealTrajectory:2":
                    markup = Fx11;
                    return true;
                case "ModifyStat:Power:1.25:1":
                    markup = Fx12;
                    return true;
                case "ExtraFlick":
                    markup = Fx13;
                    return true;
                case "ModifyStat:Mass:1.8:1":
                    markup = Fx14;
                    return true;
                case "ModifyStat:Speed:1.3:1":
                    markup = Fx15;
                    return true;
                case "ModifyRule:OpponentTurnTime:0.8:1":
                    markup = Fx16;
                    return true;
                case "ModifyFlick:1:0.5:1":
                    markup = Fx17;
                    return true;
                case "ModifyRule:HideTrajectory:1:1":
                    markup = Fx18;
                    return true;
                case "Teleport:0.5:0.5":
                    markup = Fx19;
                    return true;
                case "ModifyRule:PushEdge:0.4:1":
                    markup = Fx20;
                    return true;
                case "ModifyRule:CenterBias:0.3:1":
                    markup = Fx21;
                    return true;
                case "GrantWeapon:0.1:1":
                    markup = Fx22;
                    return true;
                case "GrantWeapon:0.18:1":
                    markup = Fx23;
                    return true;
                case "GrantWeapon:0.2:1":
                    markup = Fx24;
                    return true;
                case "GrantWeapon:0.25:1":
                    markup = Fx25;
                    return true;
                case "ModifyStat:Bounciness:1.4:1":
                    markup = Fx26;
                    return true;
                case "ModifyStat:Friction:2.5:1":
                    markup = Fx27;
                    return true;
                case "PlaceDeskObject:DustPile:2":
                    markup = Fx28;
                    return true;
                case "GrantShield:1:2":
                    markup = Fx29;
                    return true;
                case "ModifyRule:CooldownCut:0.5:1":
                    markup = Fx30;
                    return true;
                case "ModifyRule:SelfTurnTime:1.2:1":
                    markup = Fx31;
                    return true;
                case "PlaceDeskObject:ClearDust:1":
                    markup = Fx32;
                    return true;
                case "ModifyStat:Power:1.15:1":
                    markup = Fx33;
                    return true;
                case "ModifyStat:Power:1.8:1":
                    markup = Fx34;
                    return true;
                case "ModifyStat:Mass:2.2:1":
                    markup = Fx35;
                    return true;
                case "ModifyRule:PushEdge:0.7:1":
                    markup = Fx36;
                    return true;
                case "ModifyFlick:1:0:0.5":
                    markup = Fx37;
                    return true;
                case "ModifyStat:Friction:3:1":
                    markup = Fx38;
                    return true;
                case "PlaceDeskObject:Cone:2":
                    markup = Fx39;
                    return true;
                case "ModifyStat:Speed:1.5:1":
                    markup = Fx40;
                    return true;
                case "ModifyStat:Power:1.35:1":
                    markup = Fx41;
                    return true;
                case "ModifyStat:Bounciness:1.8:1":
                    markup = Fx42;
                    return true;
                case "GrantWeapon:0.3:1":
                    markup = Fx43;
                    return true;
                case "GrantWeapon:0.28:1":
                    markup = Fx44;
                    return true;
                case "ModifyRule:OpponentTurnTime:0.6:1":
                    markup = Fx45;
                    return true;
                case "RestoreStock:1":
                    markup = Fx46;
                    return true;
                case "ModifyRule:SelfTurnTime:1.5:1":
                    markup = Fx47;
                    return true;
                case "ModifyRule:OpponentTurnTime:0.3:1":
                    markup = Fx48;
                    return true;
                case "GrantShield:1:3":
                    markup = Fx49;
                    return true;
                default:
                    markup = null;
                    return false;
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

        public static string EffectFileName(string iconKey)
        {
            return "fx-" + iconKey.Replace(":", "-").Replace(".", "_") + ".svg";
        }

        const string Weight =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""8"" y=""4"" width=""48"" height=""18"" rx=""6"" fill=""#24180F""/>
  <rect x=""26"" y=""16"" width=""12"" height=""32"" fill=""#24180F""/>
  <rect x=""8"" y=""42"" width=""48"" height=""18"" rx=""6"" fill=""#24180F""/>
</svg>
";

        const string Grip =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""4"" y=""10"" width=""18"" height=""44"" rx=""4"" fill=""#24180F""/>
  <rect x=""42"" y=""10"" width=""18"" height=""44"" rx=""4"" fill=""#24180F""/>
  <rect x=""20"" y=""27"" width=""24"" height=""10"" rx=""2"" fill=""#24180F""/>
</svg>
";

        const string Bounce =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""6"" y=""52"" width=""52"" height=""8"" rx=""3"" fill=""#24180F""/>
  <circle cx=""32"" cy=""36"" r=""12"" fill=""#24180F""/>
  <polygon points=""32.0,4.0 44.0,18.0 36.0,18.0 36.0,26.0 28.0,26.0 28.0,18.0 20.0,18.0"" fill=""#24180F""/>
</svg>
";

        const string Power =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,6.0 39.1,24.9 58.0,32.0 39.1,39.1 32.0,58.0 24.9,39.1 6.0,32.0 24.9,24.9"" fill=""#24180F""/>
</svg>
";

        const string Speed =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""4"" y=""12"" width=""40"" height=""8"" rx=""4"" fill=""#24180F""/>
  <rect x=""16"" y=""28"" width=""44"" height=""8"" rx=""4"" fill=""#24180F""/>
  <rect x=""8"" y=""44"" width=""32"" height=""8"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Flick =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""48"" cy=""32"" r=""12"" fill=""#24180F""/>
  <polygon points=""4.0,20.0 34.0,32.0 4.0,44.0"" fill=""#24180F""/>
</svg>
";

        const string Clock =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""32"" r=""24"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""17"" fill=""#FFFCF6""/>
  <rect x=""29"" y=""16"" width=""6"" height=""18"" fill=""#24180F""/>
  <rect x=""29"" y=""29"" width=""18"" height=""6"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""3"" fill=""#24180F""/>
</svg>
";

        const string Sight =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""2.0,32.0 14.0,18.0 32.0,14.0 50.0,18.0 62.0,32.0 50.0,46.0 32.0,50.0 14.0,46.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""10"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""32"" r=""5"" fill=""#24180F""/>
</svg>
";

        const string Edge =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""50"" y=""4"" width=""10"" height=""56"" rx=""2"" fill=""#24180F""/>
  <rect x=""6"" y=""22"" width=""16"" height=""20"" rx=""3"" fill=""#24180F""/>
  <polygon points=""22.0,24.0 46.0,32.0 22.0,40.0"" fill=""#24180F""/>
</svg>
";

        const string Center =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""32"" r=""18"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""11"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""32"" r=""4"" fill=""#24180F""/>
  <rect x=""29"" y=""4"" width=""6"" height=""12"" fill=""#24180F""/>
  <rect x=""29"" y=""48"" width=""6"" height=""12"" fill=""#24180F""/>
  <rect x=""4"" y=""29"" width=""12"" height=""6"" fill=""#24180F""/>
  <rect x=""48"" y=""29"" width=""12"" height=""6"" fill=""#24180F""/>
</svg>
";

        const string Extra =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""8"" y=""18"" width=""30"" height=""36"" rx=""4"" fill=""#24180F""/>
  <rect x=""26"" y=""8"" width=""30"" height=""36"" rx=""4"" fill=""#24180F""/>
  <rect x=""32"" y=""14"" width=""18"" height=""24"" fill=""#FFFCF6""/>
</svg>
";

        const string Swap =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""6"" y=""12"" width=""30"" height=""12"" rx=""3"" fill=""#24180F""/>
  <polygon points=""36.0,10.0 58.0,18.0 36.0,26.0"" fill=""#24180F""/>
  <rect x=""28"" y=""40"" width=""30"" height=""12"" rx=""3"" fill=""#24180F""/>
  <polygon points=""28.0,38.0 6.0,46.0 28.0,54.0"" fill=""#24180F""/>
</svg>
";

        const string Pin =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,60.0 16.0,30.0 48.0,30.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""24"" r=""16"" fill=""#24180F""/>
  <circle cx=""32"" cy=""24"" r=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Desk =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""22"" y=""6"" width=""20"" height=""14"" rx=""3"" fill=""#24180F""/>
  <rect x=""6"" y=""22"" width=""52"" height=""10"" rx=""2"" fill=""#24180F""/>
  <rect x=""12"" y=""32"" width=""8"" height=""24"" fill=""#24180F""/>
  <rect x=""44"" y=""32"" width=""8"" height=""24"" fill=""#24180F""/>
</svg>
";

        const string Shield =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 56.0,14.0 54.0,34.0 32.0,60.0 10.0,34.0 8.0,14.0"" fill=""#24180F""/>
  <rect x=""28"" y=""18"" width=""8"" height=""26"" fill=""#FFFCF6""/>
</svg>
";

        const string Weapon =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,2.0 46.0,20.0 18.0,20.0"" fill=""#24180F""/>
  <rect x=""20"" y=""18"" width=""24"" height=""28"" rx=""2"" fill=""#24180F""/>
  <rect x=""26"" y=""22"" width=""12"" height=""20"" fill=""#FFFCF6""/>
  <rect x=""20"" y=""44"" width=""24"" height=""6"" fill=""#24180F""/>
  <rect x=""22"" y=""50"" width=""20"" height=""10"" rx=""3"" fill=""#24180F""/>
</svg>
";

        const string Heart =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""22"" cy=""24"" r=""14"" fill=""#24180F""/>
  <circle cx=""42"" cy=""24"" r=""14"" fill=""#24180F""/>
  <polygon points=""8.0,30.0 56.0,30.0 32.0,58.0"" fill=""#24180F""/>
  <rect x=""29"" y=""18"" width=""6"" height=""18"" fill=""#FFFCF6""/>
  <rect x=""23"" y=""24"" width=""18"" height=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Star =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,8.0 38.5,25.1 56.7,26.0 42.5,37.4 47.3,55.0 32.0,45.0 16.7,55.0 21.5,37.4 7.3,26.0 25.5,25.1"" fill=""#24180F""/>
</svg>
";

        const string Fx0 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""14"" y=""4"" width=""36"" height=""12"" rx=""4"" fill=""#24180F""/>
  <rect x=""28"" y=""12"" width=""8"" height=""40"" fill=""#24180F""/>
  <rect x=""14"" y=""48"" width=""36"" height=""12"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Fx1 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""24"" y=""6"" width=""16"" height=""10"" rx=""2"" fill=""#24180F""/>
  <rect x=""16"" y=""16"" width=""32"" height=""40"" rx=""8"" fill=""#24180F""/>
  <rect x=""24"" y=""28"" width=""16"" height=""16"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx2 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""6.0,8.0 34.0,22.0 6.0,36.0"" fill=""#24180F""/>
  <polygon points=""30.0,28.0 58.0,42.0 30.0,56.0"" fill=""#24180F""/>
</svg>
";

        const string Fx3 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""4"" y=""26"" width=""56"" height=""14"" rx=""2"" fill=""#24180F""/>
  <rect x=""12"" y=""26"" width=""3"" height=""7"" fill=""#FFFCF6""/>
  <rect x=""22"" y=""26"" width=""3"" height=""7"" fill=""#FFFCF6""/>
  <rect x=""32"" y=""26"" width=""3"" height=""7"" fill=""#FFFCF6""/>
  <rect x=""42"" y=""26"" width=""3"" height=""7"" fill=""#FFFCF6""/>
  <rect x=""52"" y=""26"" width=""3"" height=""7"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx4 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""4.0,26.0 40.0,32.0 4.0,38.0"" fill=""#24180F""/>
  <circle cx=""50"" cy=""32"" r=""8"" fill=""#24180F""/>
</svg>
";

        const string Fx5 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""4.0,16.0 38.0,32.0 4.0,48.0"" fill=""#24180F""/>
  <circle cx=""50"" cy=""32"" r=""12"" fill=""#24180F""/>
  <polygon points=""16.0,4.0 18.1,9.9 24.0,12.0 18.1,14.1 16.0,20.0 13.9,14.1 8.0,12.0 13.9,9.9"" fill=""#24180F""/>
</svg>
";

        const string Fx6 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""10"" y=""8"" width=""44"" height=""8"" rx=""3"" fill=""#24180F""/>
  <polygon points=""12.0,40.0 32.0,22.0 52.0,40.0 32.0,52.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""38"" r=""5"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx7 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""6"" y=""12"" width=""30"" height=""12"" rx=""3"" fill=""#24180F""/>
  <polygon points=""36.0,10.0 58.0,18.0 36.0,26.0"" fill=""#24180F""/>
  <rect x=""28"" y=""40"" width=""30"" height=""12"" rx=""3"" fill=""#24180F""/>
  <polygon points=""28.0,38.0 6.0,46.0 28.0,54.0"" fill=""#24180F""/>
</svg>
";

        const string Fx8 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 56.0,14.0 54.0,34.0 32.0,60.0 10.0,34.0 8.0,14.0"" fill=""#24180F""/>
  <rect x=""28"" y=""18"" width=""8"" height=""26"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx9 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 48.0,22.0 42.0,56.0 22.0,56.0 16.0,22.0"" fill=""#24180F""/>
  <rect x=""26"" y=""30"" width=""12"" height=""8"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx10 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""14"" cy=""16"" r=""5"" fill=""#24180F""/>
  <circle cx=""36"" cy=""12"" r=""4"" fill=""#24180F""/>
  <circle cx=""52"" cy=""24"" r=""3"" fill=""#24180F""/>
  <circle cx=""24"" cy=""34"" r=""7"" fill=""#24180F""/>
  <circle cx=""46"" cy=""40"" r=""5"" fill=""#24180F""/>
  <circle cx=""16"" cy=""50"" r=""4"" fill=""#24180F""/>
</svg>
";

        const string Fx11 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""2.0,32.0 14.0,18.0 32.0,14.0 50.0,18.0 62.0,32.0 50.0,46.0 32.0,50.0 14.0,46.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""10"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""32"" r=""5"" fill=""#24180F""/>
</svg>
";

        const string Fx12 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,6.0 39.1,24.9 58.0,32.0 39.1,39.1 32.0,58.0 24.9,39.1 6.0,32.0 24.9,24.9"" fill=""#24180F""/>
</svg>
";

        const string Fx13 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""8"" y=""18"" width=""30"" height=""36"" rx=""4"" fill=""#24180F""/>
  <rect x=""26"" y=""8"" width=""30"" height=""36"" rx=""4"" fill=""#24180F""/>
  <rect x=""32"" y=""14"" width=""18"" height=""24"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx14 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""8"" y=""4"" width=""48"" height=""16"" rx=""4"" fill=""#24180F""/>
  <rect x=""28"" y=""16"" width=""8"" height=""32"" fill=""#24180F""/>
  <rect x=""8"" y=""44"" width=""48"" height=""16"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Fx15 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""6.0,6.0 48.0,18.0 6.0,30.0"" fill=""#24180F""/>
  <polygon points=""6.0,34.0 48.0,46.0 6.0,58.0"" fill=""#24180F""/>
</svg>
";

        const string Fx16 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""10.0,6.0 40.0,6.0 54.0,20.0 54.0,58.0 10.0,58.0"" fill=""#24180F""/>
  <rect x=""18"" y=""18"" width=""26"" height=""6"" fill=""#FFFCF6""/>
  <rect x=""18"" y=""30"" width=""20"" height=""6"" fill=""#FFFCF6""/>
  <rect x=""18"" y=""42"" width=""24"" height=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx17 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""4.0,22.0 46.0,32.0 4.0,42.0"" fill=""#24180F""/>
  <polygon points=""24.0,40.0 36.0,52.0 18.0,56.0"" fill=""#24180F""/>
</svg>
";

        const string Fx18 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""4.0,32.0 18.0,18.0 32.0,16.0 46.0,18.0 60.0,32.0 46.0,46.0 32.0,48.0 18.0,46.0"" fill=""#24180F""/>
  <polygon points=""14.0,16.0 24.0,16.0 50.0,50.0 40.0,50.0"" fill=""#24180F""/>
  <polygon points=""40.0,16.0 50.0,16.0 24.0,50.0 14.0,50.0"" fill=""#24180F""/>
</svg>
";

        const string Fx19 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,60.0 16.0,30.0 48.0,30.0"" fill=""#24180F""/>
  <circle cx=""32"" cy=""24"" r=""16"" fill=""#24180F""/>
  <circle cx=""32"" cy=""24"" r=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx20 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""50"" y=""8"" width=""8"" height=""48"" fill=""#24180F""/>
  <polygon points=""8.0,24.0 30.0,32.0 8.0,40.0"" fill=""#24180F""/>
</svg>
";

        const string Fx21 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""32"" r=""18"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""11"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""32"" r=""4"" fill=""#24180F""/>
  <rect x=""29"" y=""4"" width=""6"" height=""12"" fill=""#24180F""/>
  <rect x=""29"" y=""48"" width=""6"" height=""12"" fill=""#24180F""/>
  <rect x=""4"" y=""29"" width=""12"" height=""6"" fill=""#24180F""/>
  <rect x=""48"" y=""29"" width=""12"" height=""6"" fill=""#24180F""/>
</svg>
";

        const string Fx22 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,2.0 40.0,16.0 36.0,16.0 36.0,54.0 28.0,54.0 28.0,16.0 24.0,16.0"" fill=""#24180F""/>
  <rect x=""29"" y=""36"" width=""6"" height=""8"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx23 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""34"" r=""22"" fill=""#24180F""/>
  <rect x=""6"" y=""34"" width=""52"" height=""26"" fill=""#FFFCF6""/>
  <rect x=""6"" y=""30"" width=""52"" height=""6"" fill=""#24180F""/>
</svg>
";

        const string Fx24 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""16"" y=""28"" width=""36"" height=""16"" rx=""3"" fill=""#24180F""/>
  <rect x=""20"" y=""12"" width=""28"" height=""18"" rx=""3"" fill=""#24180F""/>
  <rect x=""26"" y=""16"" width=""14"" height=""8"" fill=""#FFFCF6""/>
  <rect x=""20"" y=""46"" width=""8"" height=""10"" fill=""#24180F""/>
  <rect x=""36"" y=""46"" width=""8"" height=""10"" fill=""#24180F""/>
</svg>
";

        const string Fx25 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""14"" y=""12"" width=""10"" height=""36"" rx=""4"" fill=""#24180F""/>
  <rect x=""40"" y=""16"" width=""10"" height=""36"" rx=""4"" fill=""#24180F""/>
  <rect x=""14"" y=""8"" width=""36"" height=""10"" rx=""4"" fill=""#24180F""/>
  <rect x=""18"" y=""46"" width=""28"" height=""10"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Fx26 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""10"" y=""52"" width=""44"" height=""6"" rx=""2"" fill=""#24180F""/>
  <circle cx=""32"" cy=""36"" r=""10"" fill=""#24180F""/>
  <polygon points=""32.0,8.0 42.0,20.0 22.0,20.0"" fill=""#24180F""/>
</svg>
";

        const string Fx27 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""30"" cy=""32"" r=""20"" fill=""#24180F""/>
  <circle cx=""30"" cy=""32"" r=""8"" fill=""#FFFCF6""/>
  <rect x=""36"" y=""26"" width=""24"" height=""12"" rx=""2"" fill=""#24180F""/>
</svg>
";

        const string Fx28 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""6.0,54.0 58.0,54.0 32.0,14.0"" fill=""#24180F""/>
  <circle cx=""24"" cy=""42"" r=""4"" fill=""#FFFCF6""/>
  <circle cx=""40"" cy=""36"" r=""3"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx29 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 56.0,14.0 54.0,34.0 32.0,60.0 10.0,34.0 8.0,14.0"" fill=""#24180F""/>
  <rect x=""28"" y=""18"" width=""8"" height=""26"" fill=""#FFFCF6""/>
  <rect x=""16"" y=""26"" width=""32"" height=""8"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx30 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""32"" cy=""32"" r=""24"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""17"" fill=""#FFFCF6""/>
  <rect x=""29"" y=""16"" width=""6"" height=""18"" fill=""#24180F""/>
  <rect x=""29"" y=""29"" width=""18"" height=""6"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""3"" fill=""#24180F""/>
  <polygon points=""8.0,54.0 16.0,60.0 56.0,12.0 48.0,6.0"" fill=""#24180F""/>
</svg>
";

        const string Fx31 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,6.0 50.0,18.0 46.0,40.0 18.0,40.0 14.0,18.0"" fill=""#24180F""/>
  <rect x=""28"" y=""40"" width=""8"" height=""8"" fill=""#24180F""/>
  <rect x=""16"" y=""50"" width=""32"" height=""6"" rx=""2"" fill=""#24180F""/>
  <circle cx=""32"" cy=""26"" r=""4"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx32 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""30"" y=""4"" width=""6"" height=""34"" fill=""#24180F""/>
  <rect x=""10"" y=""34"" width=""44"" height=""8"" rx=""2"" fill=""#24180F""/>
  <rect x=""14"" y=""42"" width=""4"" height=""16"" fill=""#24180F""/>
  <rect x=""24"" y=""42"" width=""4"" height=""16"" fill=""#24180F""/>
  <rect x=""34"" y=""42"" width=""4"" height=""16"" fill=""#24180F""/>
  <rect x=""44"" y=""42"" width=""4"" height=""16"" fill=""#24180F""/>
</svg>
";

        const string Fx33 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,18.0 36.2,29.8 48.0,34.0 36.2,38.2 32.0,50.0 27.8,38.2 16.0,34.0 27.8,29.8"" fill=""#24180F""/>
</svg>
";

        const string Fx34 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 36.6,20.9 51.8,12.2 43.1,27.4 60.0,32.0 43.1,36.6 51.8,51.8 36.6,43.1 32.0,60.0 27.4,43.1 12.2,51.8 20.9,36.6 4.0,32.0 20.9,27.4 12.2,12.2 27.4,20.9"" fill=""#24180F""/>
</svg>
";

        const string Fx35 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""3"" y=""4"" width=""58"" height=""20"" rx=""4"" fill=""#24180F""/>
  <rect x=""28"" y=""20"" width=""8"" height=""24"" fill=""#24180F""/>
  <rect x=""3"" y=""40"" width=""58"" height=""20"" rx=""4"" fill=""#24180F""/>
</svg>
";

        const string Fx36 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""54"" y=""4"" width=""8"" height=""56"" fill=""#24180F""/>
  <rect x=""6"" y=""28"" width=""22"" height=""8"" fill=""#24180F""/>
  <polygon points=""20.0,16.0 48.0,32.0 20.0,48.0"" fill=""#24180F""/>
</svg>
";

        const string Fx37 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""8.0,26.0 28.0,32.0 8.0,38.0"" fill=""#24180F""/>
  <circle cx=""40"" cy=""32"" r=""6"" fill=""#24180F""/>
</svg>
";

        const string Fx38 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""8"" y=""8"" width=""48"" height=""48"" rx=""8"" fill=""#24180F""/>
  <circle cx=""22"" cy=""24"" r=""5"" fill=""#FFFCF6""/>
  <circle cx=""42"" cy=""24"" r=""5"" fill=""#FFFCF6""/>
  <circle cx=""32"" cy=""42"" r=""5"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx39 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,4.0 54.0,56.0 10.0,56.0"" fill=""#24180F""/>
  <rect x=""20"" y=""34"" width=""24"" height=""8"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx40 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""8.0,4.0 54.0,12.0 8.0,20.0"" fill=""#24180F""/>
  <polygon points=""8.0,24.0 54.0,32.0 8.0,40.0"" fill=""#24180F""/>
  <polygon points=""8.0,44.0 54.0,52.0 8.0,60.0"" fill=""#24180F""/>
</svg>
";

        const string Fx41 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,6.0 37.5,22.5 54.5,19.0 43.0,32.0 54.5,45.0 37.5,41.5 32.0,58.0 26.5,41.5 9.5,45.0 21.0,32.0 9.5,19.0 26.5,22.5"" fill=""#24180F""/>
</svg>
";

        const string Fx42 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""4"" y=""54"" width=""56"" height=""6"" rx=""2"" fill=""#24180F""/>
  <circle cx=""32"" cy=""32"" r=""18"" fill=""#24180F""/>
  <polygon points=""32.0,2.0 48.0,16.0 16.0,16.0"" fill=""#24180F""/>
</svg>
";

        const string Fx43 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""14"" y=""30"" width=""36"" height=""14"" rx=""2"" fill=""#24180F""/>
  <rect x=""18"" y=""14"" width=""26"" height=""18"" rx=""2"" fill=""#24180F""/>
  <polygon points=""6.0,6.0 16.0,6.0 58.0,58.0 48.0,58.0"" fill=""#24180F""/>
  <polygon points=""48.0,6.0 58.0,6.0 16.0,58.0 6.0,58.0"" fill=""#24180F""/>
</svg>
";

        const string Fx44 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <rect x=""26"" y=""4"" width=""12"" height=""12"" rx=""2"" fill=""#24180F""/>
  <polygon points=""18.0,16.0 46.0,16.0 52.0,56.0 12.0,56.0"" fill=""#24180F""/>
  <circle cx=""24"" cy=""30"" r=""4"" fill=""#FFFCF6""/>
  <circle cx=""40"" cy=""38"" r=""5"" fill=""#FFFCF6""/>
  <circle cx=""30"" cy=""48"" r=""4"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx45 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""36"" cy=""36"" r=""16"" fill=""#24180F""/>
  <circle cx=""14"" cy=""28"" r=""8"" fill=""#24180F""/>
  <rect x=""18"" y=""48"" width=""10"" height=""8"" rx=""2"" fill=""#24180F""/>
  <rect x=""40"" y=""48"" width=""10"" height=""8"" rx=""2"" fill=""#24180F""/>
  <rect x=""24"" y=""18"" width=""10"" height=""8"" rx=""2"" fill=""#24180F""/>
</svg>
";

        const string Fx46 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""22"" cy=""24"" r=""14"" fill=""#24180F""/>
  <circle cx=""42"" cy=""24"" r=""14"" fill=""#24180F""/>
  <polygon points=""8.0,30.0 56.0,30.0 32.0,58.0"" fill=""#24180F""/>
  <rect x=""29"" y=""18"" width=""6"" height=""18"" fill=""#FFFCF6""/>
  <rect x=""23"" y=""24"" width=""18"" height=""6"" fill=""#FFFCF6""/>
</svg>
";

        const string Fx47 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""22.0,8.0 40.0,18.0 36.0,38.0 14.0,38.0 10.0,18.0"" fill=""#24180F""/>
  <rect x=""44"" y=""14"" width=""8"" height=""24"" fill=""#24180F""/>
  <rect x=""36"" y=""22"" width=""24"" height=""8"" fill=""#24180F""/>
  <rect x=""12"" y=""50"" width=""28"" height=""6"" rx=""2"" fill=""#24180F""/>
</svg>
";

        const string Fx48 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <circle cx=""26"" cy=""32"" r=""18"" fill=""#24180F""/>
  <circle cx=""38"" cy=""26"" r=""14"" fill=""#FFFCF6""/>
  <rect x=""48"" y=""12"" width=""10"" height=""4"" fill=""#24180F""/>
  <rect x=""54"" y=""12"" width=""4"" height=""10"" fill=""#24180F""/>
</svg>
";

        const string Fx49 =
@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"">
  <polygon points=""32.0,2.0 60.0,16.0 56.0,36.0 32.0,62.0 8.0,36.0 4.0,16.0"" fill=""#24180F""/>
  <rect x=""16"" y=""20"" width=""32"" height=""6"" fill=""#FFFCF6""/>
  <rect x=""16"" y=""32"" width=""32"" height=""6"" fill=""#FFFCF6""/>
  <rect x=""16"" y=""44"" width=""32"" height=""6"" fill=""#FFFCF6""/>
</svg>
";
    }
}
