using System;
using System.Globalization;
using System.IO;
using System.Text;
using EraserWars.Core;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace EraserWars.Persistence
{
    public readonly record struct FlickReplay(
        int Seat,
        float AimX,
        float AimY,
        float Power,
        float ContactX,
        float ContactY);

    public static class ReplayStub
    {
        public const string FileName = "last-flick.txt";
        const string Header = "eraserwars-flick 1";
        static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

        public static FlickReplay From(FlickCommand command)
        {
            var replay = new FlickReplay(
                command.Player.Value,
                command.DirectionX,
                command.DirectionY,
                command.DragLength01,
                command.ContactOffsetX,
                command.ContactOffsetY);
            Validate(replay);
            return replay;
        }

        public static string Format(FlickReplay replay)
        {
            Validate(replay);
            var sb = new StringBuilder();
            sb.Append(Header).Append('\n');
            Line(sb, "seat", replay.Seat.ToString(CultureInfo.InvariantCulture));
            Line(sb, "aim", Pair(replay.AimX, replay.AimY));
            Line(sb, "power", Num(replay.Power));
            Line(sb, "contact", Pair(replay.ContactX, replay.ContactY));
            return sb.ToString();
        }

        public static string Remember(FlickCommand command, string directory)
        {
            Write(From(command), directory);
            return Format(Read(PathFor(directory)));
        }

        public static string Write(FlickReplay replay, string directory)
        {
            Validate(replay);
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("Replay directory is required.", nameof(directory));
            }

            Directory.CreateDirectory(directory);
            var path = PathFor(directory);
            var temp = path + ".tmp";
            File.WriteAllText(temp, Format(replay), Utf8);
            if (File.Exists(path))
            {
                File.Replace(temp, path, destinationBackupFileName: null);
            }
            else
            {
                File.Move(temp, path);
            }

            return path;
        }

        public static FlickReplay Read(string path)
        {
            var text = File.ReadAllText(path, Utf8);
            return Parse(text);
        }

        public static string PathFor(string directory)
        {
            return Path.Combine(directory, FileName);
        }

        public static string DefaultDirectory()
        {
#if UNITY_5_3_OR_NEWER
            return Path.Combine(Application.temporaryCachePath, "replay");
#else
            return Path.Combine(Path.GetTempPath(), "EraserWars", "replay");
#endif
        }

        static FlickReplay Parse(string text)
        {
            var lines = text.Split('\n');
            if (lines.Length == 0 || lines[0].TrimEnd('\r') != Header)
            {
                throw new FormatException("Flick replay header is missing.");
            }

            int? seat = null;
            float? aimX = null;
            float? aimY = null;
            float? power = null;
            float? contactX = null;
            float? contactY = null;

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd('\r');
                if (line.Length == 0)
                {
                    continue;
                }

                var split = line.IndexOf('=');
                if (split <= 0)
                {
                    throw new FormatException("Flick replay line has no key.");
                }

                var key = line.Substring(0, split);
                var value = line.Substring(split + 1);
                switch (key)
                {
                    case "seat":
                        AssignOnce(seat == null, "seat");
                        seat = ParseInt(value, "seat");
                        break;
                    case "aim":
                        AssignOnce(aimX == null, "aim");
                        var aim = ParsePair(value, "aim");
                        aimX = aim.X;
                        aimY = aim.Y;
                        break;
                    case "power":
                        AssignOnce(power == null, "power");
                        power = ParseFloat(value, "power");
                        break;
                    case "contact":
                        AssignOnce(contactX == null, "contact");
                        var contact = ParsePair(value, "contact");
                        contactX = contact.X;
                        contactY = contact.Y;
                        break;
                    default:
                        throw new FormatException("Flick replay has an unknown field '" + key + "'.");
                }
            }

            if (seat == null || aimX == null || aimY == null || power == null || contactX == null || contactY == null)
            {
                throw new FormatException("Flick replay is missing a field.");
            }

            var replay = new FlickReplay(seat.Value, aimX.Value, aimY.Value, power.Value, contactX.Value, contactY.Value);
            Validate(replay);
            return replay;
        }

        static void Validate(FlickReplay replay)
        {
            if (replay.Seat is < 0 or > 3)
            {
                throw new ArgumentException("Seat is 0 to 3.", nameof(replay));
            }

            RequireFinite(replay.AimX, nameof(replay));
            RequireFinite(replay.AimY, nameof(replay));
            RequireFinite(replay.Power, nameof(replay));
            RequireFinite(replay.ContactX, nameof(replay));
            RequireFinite(replay.ContactY, nameof(replay));
        }

        static void RequireFinite(float value, string name)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("Flick numbers must be finite.", name);
            }
        }

        static void Line(StringBuilder sb, string key, string value)
        {
            sb.Append(key).Append('=').Append(value).Append('\n');
        }

        static string Pair(float x, float y)
        {
            return Num(x) + " " + Num(y);
        }

        static string Num(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        static (float X, float Y) ParsePair(string value, string field)
        {
            var parts = value.Split(' ');
            if (parts.Length != 2)
            {
                throw new FormatException("Field " + field + " needs two numbers.");
            }

            return (ParseFloat(parts[0], field), ParseFloat(parts[1], field));
        }

        static float ParseFloat(string value, string field)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                || !float.IsFinite(number))
            {
                throw new FormatException("Field " + field + " is not a finite number.");
            }

            return number;
        }

        static int ParseInt(string value, string field)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                throw new FormatException("Field " + field + " is not an integer.");
            }

            return number;
        }

        static void AssignOnce(bool empty, string field)
        {
            if (!empty)
            {
                throw new FormatException("Field " + field + " repeats.");
            }
        }
    }
}
