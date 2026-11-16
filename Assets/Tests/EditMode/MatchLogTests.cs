using System;
using System.IO;
using EraserWars.Core;
using EraserWars.Persistence;
using NUnit.Framework;

namespace EraserWars.Tests.EditMode
{
    public sealed class MatchLogTests
    {
        string directory;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "eraserwars-match-log-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void StockKnockoutRoundTripsAndSecondCommitKeepsTheEndTime()
        {
            var state = Knockout(RuleSet.CasualStock with { StockCount = 1 });
            var started = new DateTime(2026, 11, 17, 0, 0, 0, DateTimeKind.Utc);
            var ended = started.AddMinutes(4);
            var session = new MatchLogSession();
            session.Begin(started, "stock-one");

            Assert.IsFalse(session.TryCommit(
                state with { Phase = Phase.TurnStart },
                ended,
                directory,
                retry: false,
                out _));
            Assert.IsFalse(File.Exists(Path.Combine(directory, "stock-one.log")));

            Assert.IsTrue(session.TryCommit(state, ended, directory, retry: false, out var error));
            Assert.IsNull(error);
            Assert.IsFalse(session.TryCommit(state, ended.AddMinutes(9), directory, retry: false, out _));

            var log = MatchLogWriter.Read(Path.Combine(directory, "stock-one.log"));
            Assert.AreEqual("stock-one", log.Id);
            Assert.AreEqual(started, log.StartedUtc);
            Assert.AreEqual(ended, log.EndedUtc);
            Assert.AreEqual(WinMode.Stock, log.WinMode);
            Assert.AreEqual(1, log.StockCount);
            Assert.AreEqual(8, log.PointRounds);
            Assert.AreEqual(1, log.Round);
            Assert.AreEqual(0, log.WinnerSeat);
            Assert.AreEqual(2, log.Seats.Length);
            Assert.AreEqual(1, log.Seats[0].Stocks);
            Assert.AreEqual(0, log.Seats[0].Points);
            Assert.IsNull(log.Seats[0].EliminatedRound);
            Assert.AreEqual(0, log.Seats[1].Stocks);
            Assert.AreEqual(0, log.Seats[1].Points);
            Assert.AreEqual(1, log.Seats[1].EliminatedRound);
        }

        [Test]
        public void PointsKnockoutRecordsTheScorer()
        {
            var state = Knockout(RuleSet.PointsTrial with { StockCount = 1 });
            var started = new DateTime(2026, 11, 17, 1, 0, 0, DateTimeKind.Utc);
            var ended = started.AddMinutes(6);
            var session = new MatchLogSession();
            session.Begin(started, "points-one");
            Assert.IsTrue(session.TryCommit(state, ended, directory, retry: false, out _));

            var log = MatchLogWriter.Read(Path.Combine(directory, "points-one.log"));
            Assert.AreEqual(WinMode.Points, log.WinMode);
            Assert.AreEqual(1, log.StockCount);
            Assert.AreEqual(8, log.PointRounds);
            Assert.AreEqual(0, log.WinnerSeat);
            Assert.AreEqual(1, log.Seats[0].Points);
            Assert.AreEqual(1, log.Seats[0].Stocks);
            Assert.AreEqual(0, log.Seats[1].Points);
            Assert.AreEqual(0, log.Seats[1].Stocks);
        }

        [Test]
        public void MutualFallRecordsADraw()
        {
            var rules = RuleSet.CasualStock with { StockCount = 1 };
            var state = MatchFactory.CreateHotseat(2, rules);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(Flick(state.CurrentPlayerId)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(0), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            state = MatchReduce.Reduce(state, new SimulationSettled());

            var started = new DateTime(2026, 11, 17, 2, 0, 0, DateTimeKind.Utc);
            var session = new MatchLogSession();
            session.Begin(started, "draw-one");
            Assert.IsTrue(session.TryCommit(state, started.AddMinutes(3), directory, retry: false, out _));

            var log = MatchLogWriter.Read(Path.Combine(directory, "draw-one.log"));
            Assert.IsNull(log.WinnerSeat);
            Assert.AreEqual(0, log.Seats[0].Stocks);
            Assert.AreEqual(0, log.Seats[1].Stocks);
            Assert.AreEqual(1, log.Seats[0].EliminatedRound);
            Assert.AreEqual(1, log.Seats[1].EliminatedRound);
        }

        [Test]
        public void NegativePointsRoundTrip()
        {
            var started = new DateTime(2026, 11, 17, 3, 0, 0, DateTimeKind.Utc);
            var log = new MatchLog(
                "self-fall",
                started,
                started.AddMinutes(2),
                WinMode.Points,
                3,
                8,
                2,
                null,
                new[]
                {
                    new MatchSeatLog(0, 2, -1, null),
                    new MatchSeatLog(1, 3, 2, null),
                });
            var path = MatchLogWriter.Write(log, directory);
            var read = MatchLogWriter.Read(path);
            Assert.AreEqual(-1, read.Seats[0].Points);
            Assert.AreEqual(2, read.Seats[1].Points);
            Assert.AreEqual(WinMode.Points, read.WinMode);
            Assert.IsNull(read.WinnerSeat);
            Assert.AreEqual(3, read.StockCount);
            Assert.AreEqual(2, read.Round);
        }

        [Test]
        public void ASecondMatchLeavesTheFirstFile()
        {
            var state = Knockout(RuleSet.CasualStock with { StockCount = 1 });
            var started = new DateTime(2026, 11, 17, 4, 0, 0, DateTimeKind.Utc);
            var session = new MatchLogSession();
            session.Begin(started, "first-match");
            Assert.IsTrue(session.TryCommit(state, started.AddMinutes(1), directory, retry: false, out _));
            session.Begin(started.AddMinutes(2), "second-match");
            Assert.IsTrue(session.TryCommit(state, started.AddMinutes(3), directory, retry: false, out _));

            var first = MatchLogWriter.Read(Path.Combine(directory, "first-match.log"));
            var second = MatchLogWriter.Read(Path.Combine(directory, "second-match.log"));
            Assert.AreEqual(started, first.StartedUtc);
            Assert.AreEqual(started.AddMinutes(2), second.StartedUtc);
        }

        [Test]
        public void TruncatedAndUnknownFieldsThrow()
        {
            var truncated = Path.Combine(directory, "short.log");
            File.WriteAllText(truncated, "eraserwars-match 1\n");
            Assert.Throws<FormatException>(() => MatchLogWriter.Read(truncated));

            var unknown = Path.Combine(directory, "extra.log");
            File.WriteAllText(unknown, "eraserwars-match 1\nbonus=1\n");
            Assert.Throws<FormatException>(() => MatchLogWriter.Read(unknown));
        }

        [Test]
        public void FailedWriteRetriesIntoARealDirectory()
        {
            var state = Knockout(RuleSet.CasualStock with { StockCount = 1 });
            var started = new DateTime(2026, 11, 17, 5, 0, 0, DateTimeKind.Utc);
            var ended = started.AddMinutes(1);
            var blocker = Path.Combine(directory, "not-a-directory");
            File.WriteAllText(blocker, "x");
            var session = new MatchLogSession();
            session.Begin(started, "retry-match");

            Assert.IsFalse(session.TryCommit(state, ended, blocker, retry: false, out var error));
            Assert.IsNotNull(error);
            Assert.IsFalse(session.TryCommit(state, ended.AddMinutes(1), blocker, retry: false, out var quiet));
            Assert.IsNull(quiet);
            Assert.IsTrue(session.TryCommit(state, ended.AddMinutes(2), directory, retry: true, out _));

            var log = MatchLogWriter.Read(Path.Combine(directory, "retry-match.log"));
            Assert.AreEqual(ended, log.EndedUtc);
        }

        static MatchState Knockout(RuleSet rules)
        {
            var state = MatchFactory.CreateHotseat(2, rules);
            state = MatchReduce.Reduce(state, new TurnStarted(state.CurrentPlayerId));
            state = MatchReduce.Reduce(state, new Flicked(Flick(state.CurrentPlayerId)));
            state = MatchReduce.Reduce(state, new EraserFell(new PlayerId(1), new PlayerId(0)));
            return MatchReduce.Reduce(state, new SimulationSettled());
        }

        static FlickCommand Flick(PlayerId player) =>
            new(player, 1f, 0f, 0.5f, 0f, 0f, 0.1f);
    }
}
