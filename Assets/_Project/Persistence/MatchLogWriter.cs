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
    public static class MatchLogWriter
    {
        const string Header = "eraserwars-match 1";
        static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

        public static MatchLog Capture(string id, DateTime startedUtc, DateTime endedUtc, MatchState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Phase != Phase.MatchEnd)
            {
                throw new ArgumentException("A match log records Phase.MatchEnd.", nameof(state));
            }

            var seats = new MatchSeatLog[state.Players.Length];
            for (var i = 0; i < seats.Length; i++)
            {
                var player = state.Players[i];
                seats[i] = new MatchSeatLog(
                    player.Id.Value,
                    player.Stocks,
                    player.Points,
                    player.EliminatedRound);
            }

            Array.Sort(seats, (a, b) => a.Seat.CompareTo(b.Seat));
            int? winner = state.Winner is PlayerId won ? won.Value : null;
            var log = new MatchLog(
                id,
                AsUtc(startedUtc),
                AsUtc(endedUtc),
                state.Rules.WinMode,
                state.Rules.StockCount,
                state.Rules.PointRounds,
                state.Round,
                winner,
                seats);
            Validate(log);
            return log;
        }

        public static string Write(MatchLog log, string directory)
        {
            Validate(log);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, log.Id + ".log");
            var temp = path + ".tmp";
            File.WriteAllText(temp, Format(log), Utf8);
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

        public static MatchLog Read(string path)
        {
            var text = File.ReadAllText(path, Utf8);
            return Parse(text);
        }

        public static string DefaultDirectory()
        {
#if UNITY_EDITOR
            var assets = Application.dataPath;
            var root = Directory.GetParent(assets)?.FullName ?? assets;
            return Path.Combine(root, "playtests", "match-logs");
#elif UNITY_5_3_OR_NEWER
            return Path.Combine(Application.persistentDataPath, "MatchLogs");
#else
            throw new InvalidOperationException("Match log folders are chosen by the Unity player.");
#endif
        }

        static string Format(MatchLog log)
        {
            var sb = new StringBuilder();
            sb.Append(Header).Append('\n');
            Line(sb, "id", log.Id);
            Line(sb, "started", log.StartedUtc.ToString("o", CultureInfo.InvariantCulture));
            Line(sb, "ended", log.EndedUtc.ToString("o", CultureInfo.InvariantCulture));
            Line(sb, "win", FormatWin(log.WinMode));
            Line(sb, "stockCount", log.StockCount.ToString(CultureInfo.InvariantCulture));
            Line(sb, "pointRounds", log.PointRounds.ToString(CultureInfo.InvariantCulture));
            Line(sb, "round", log.Round.ToString(CultureInfo.InvariantCulture));
            Line(sb, "winner", log.WinnerSeat is int seat
                ? seat.ToString(CultureInfo.InvariantCulture)
                : "-");
            for (var i = 0; i < log.Seats.Length; i++)
            {
                var row = log.Seats[i];
                var eliminated = row.EliminatedRound is int eliminatedRound
                    ? eliminatedRound.ToString(CultureInfo.InvariantCulture)
                    : "-";
                sb.Append("seat=")
                    .Append(row.Seat.ToString(CultureInfo.InvariantCulture))
                    .Append(' ')
                    .Append(row.Stocks.ToString(CultureInfo.InvariantCulture))
                    .Append(' ')
                    .Append(row.Points.ToString(CultureInfo.InvariantCulture))
                    .Append(' ')
                    .Append(eliminated)
                    .Append('\n');
            }

            return sb.ToString();
        }

        static void Line(StringBuilder sb, string key, string value)
        {
            sb.Append(key).Append('=').Append(value).Append('\n');
        }

        static MatchLog Parse(string text)
        {
            var lines = text.Split('\n');
            if (lines.Length == 0 || lines[0].TrimEnd('\r') != Header)
            {
                throw new FormatException("Match log header is missing.");
            }

            string id = null;
            DateTime? started = null;
            DateTime? ended = null;
            WinMode? win = null;
            int? stockCount = null;
            int? pointRounds = null;
            int? round = null;
            int? winner = null;
            var winnerDraw = false;
            var seats = new MatchSeatLog[4];
            var seatCount = 0;

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
                    throw new FormatException("Match log line has no key.");
                }

                var key = line.Substring(0, split);
                var value = line.Substring(split + 1);
                switch (key)
                {
                    case "id":
                        AssignOnce(id == null, "id");
                        id = value;
                        break;
                    case "started":
                        AssignOnce(started == null, "started");
                        started = ParseUtc(value, "started");
                        break;
                    case "ended":
                        AssignOnce(ended == null, "ended");
                        ended = ParseUtc(value, "ended");
                        break;
                    case "win":
                        AssignOnce(win == null, "win");
                        win = ParseWin(value);
                        break;
                    case "stockCount":
                        AssignOnce(stockCount == null, "stockCount");
                        stockCount = ParseInt(value, "stockCount");
                        break;
                    case "pointRounds":
                        AssignOnce(pointRounds == null, "pointRounds");
                        pointRounds = ParseInt(value, "pointRounds");
                        break;
                    case "round":
                        AssignOnce(round == null, "round");
                        round = ParseInt(value, "round");
                        break;
                    case "winner":
                        AssignOnce(winner == null && !winnerDraw, "winner");
                        if (value == "-")
                        {
                            winnerDraw = true;
                        }
                        else
                        {
                            winner = ParseInt(value, "winner");
                        }

                        break;
                    case "seat":
                        if (seatCount == seats.Length)
                        {
                            throw new FormatException("Match log has more than 4 seats.");
                        }

                        seats[seatCount] = ParseSeat(value);
                        seatCount++;
                        break;
                    default:
                        throw new FormatException("Match log has an unknown field '" + key + "'.");
                }
            }

            if (id == null || started == null || ended == null || win == null
                || stockCount == null || pointRounds == null || round == null
                || (winner == null && !winnerDraw))
            {
                throw new FormatException("Match log is missing a field.");
            }

            var recorded = new MatchSeatLog[seatCount];
            Array.Copy(seats, recorded, seatCount);
            Array.Sort(recorded, (a, b) => a.Seat.CompareTo(b.Seat));
            var log = new MatchLog(
                id,
                started.Value,
                ended.Value,
                win.Value,
                stockCount.Value,
                pointRounds.Value,
                round.Value,
                winnerDraw ? null : winner,
                recorded);
            Validate(log);
            return log;
        }

        static MatchSeatLog ParseSeat(string value)
        {
            var parts = value.Split(' ');
            if (parts.Length != 4)
            {
                throw new FormatException("A seat line needs seat, stocks, points, and eliminated round.");
            }

            int? eliminated = parts[3] == "-" ? null : ParseInt(parts[3], "eliminated");
            return new MatchSeatLog(
                ParseInt(parts[0], "seat"),
                ParseInt(parts[1], "stocks"),
                ParseInt(parts[2], "points"),
                eliminated);
        }

        static void Validate(MatchLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            if (string.IsNullOrEmpty(log.Id) || !IsSafeId(log.Id))
            {
                throw new ArgumentException("Match id must be a file name.", nameof(log));
            }

            if (log.StartedUtc.Kind != DateTimeKind.Utc || log.EndedUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Match times are UTC.", nameof(log));
            }

            if (log.StockCount < 1 || log.PointRounds < 1 || log.Round < 1)
            {
                throw new ArgumentException("Stock count, point rounds, and round start at 1.", nameof(log));
            }

            if (log.Seats == null || log.Seats.Length is < 2 or > 4)
            {
                throw new ArgumentException("A match log has 2 to 4 seats.", nameof(log));
            }

            FormatWin(log.WinMode);
            for (var i = 0; i < log.Seats.Length; i++)
            {
                var seat = log.Seats[i];
                if (seat == null)
                {
                    throw new ArgumentException("A seat line is missing.", nameof(log));
                }

                if (seat.Seat is < 0 or > 3 || seat.Stocks < 0)
                {
                    throw new ArgumentException("Seat or stocks are out of range.", nameof(log));
                }

                if (seat.EliminatedRound is int eliminated
                    && (eliminated < 1 || eliminated > log.Round))
                {
                    throw new ArgumentException("Eliminated round is outside the match.", nameof(log));
                }

                for (var j = 0; j < i; j++)
                {
                    if (log.Seats[j].Seat == seat.Seat)
                    {
                        throw new ArgumentException("Seat numbers repeat.", nameof(log));
                    }
                }
            }

            if (log.WinnerSeat is int winner && IndexOfSeat(log.Seats, winner) < 0)
            {
                throw new ArgumentException("Winner seat is not in the match.", nameof(log));
            }
        }

        static int IndexOfSeat(MatchSeatLog[] seats, int seat)
        {
            for (var i = 0; i < seats.Length; i++)
            {
                if (seats[i].Seat == seat)
                {
                    return i;
                }
            }

            return -1;
        }

        static bool IsSafeId(string id)
        {
            if (id.Length > 64)
            {
                return false;
            }

            for (var i = 0; i < id.Length; i++)
            {
                var c = id[i];
                var ok = (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || c == '-'
                    || c == '_';
                if (!ok)
                {
                    return false;
                }
            }

            return true;
        }

        static string FormatWin(WinMode mode)
        {
            switch (mode)
            {
                case WinMode.Stock:
                    return "Stock";
                case WinMode.Points:
                    return "Points";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unhandled win mode.");
            }
        }

        static WinMode ParseWin(string value)
        {
            switch (value)
            {
                case "Stock":
                    return WinMode.Stock;
                case "Points":
                    return WinMode.Points;
                default:
                    throw new FormatException("Unknown win mode '" + value + "'.");
            }
        }

        static DateTime ParseUtc(string value, string field)
        {
            if (!DateTime.TryParseExact(
                    value,
                    "o",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var time)
                || time.Kind != DateTimeKind.Utc)
            {
                throw new FormatException("Field " + field + " is not a UTC timestamp.");
            }

            return time;
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

        static DateTime AsUtc(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Utc:
                    return value;
                case DateTimeKind.Local:
                    return value.ToUniversalTime();
                case DateTimeKind.Unspecified:
                    return DateTime.SpecifyKind(value, DateTimeKind.Utc);
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value.Kind, "Unhandled DateTimeKind.");
            }
        }
    }

    public sealed class MatchLogSession
    {
        string id;
        DateTime startedUtc;
        DateTime? endedUtc;
        bool written;
        bool failed;

        public string Id => id;

        public void Begin(DateTime utcNow, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Match id is required.", nameof(id));
            }

            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Match start is UTC.", nameof(utcNow));
            }

            this.id = id;
            startedUtc = utcNow;
            endedUtc = null;
            written = false;
            failed = false;
        }

        public bool TryCommit(MatchState state, DateTime utcNow, string directory, bool retry, out string error)
        {
            error = null;
            if (written || id == null || state == null || state.Phase != Phase.MatchEnd)
            {
                return false;
            }

            if (failed && !retry)
            {
                return false;
            }

            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Match end is UTC.", nameof(utcNow));
            }

            endedUtc ??= utcNow;
            var log = MatchLogWriter.Capture(id, startedUtc, endedUtc.Value, state);
            try
            {
                MatchLogWriter.Write(log, directory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                failed = true;
                error = ex.Message;
                return false;
            }

            written = true;
            failed = false;
            return true;
        }
    }
}
