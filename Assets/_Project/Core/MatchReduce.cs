using System;

namespace EraserWars.Core
{
    public static class MatchReduce
    {
        public static MatchState Reduce(MatchState state, MatchEvent ev)
        {
            switch (ev)
            {
                case TurnStarted started:
                    return OnTurnStarted(state, started);
                case CardPlayed played:
                    return OnCardPlayed(state, played);
                case Flicked flicked:
                    return OnFlicked(state, flicked);
                case Passed passed:
                    return OnPassed(state, passed.Player);
                case TimerExpired expired:
                    return OnTimerExpired(state, expired);
                case EraserFell fell:
                    return OnEraserFell(state, fell);
                case SimulationSettled:
                    return OnSimulationSettled(state);
                case PlayerDisconnected disconnected:
                    return SetConnected(state, disconnected.Player, false);
                case PlayerReconnected reconnected:
                    return SetConnected(state, reconnected.Player, true);
                default:
                    throw new InvalidOperationException($"Unhandled match event {ev.GetType().Name}");
            }
        }

        static MatchState OnTurnStarted(MatchState state, TurnStarted started)
        {
            if (state.Phase != Phase.TurnStart)
            {
                return state;
            }

            if (!state.CurrentPlayerId.Equals(started.Player))
            {
                return state;
            }

            return OpenTurn(state);
        }

        static MatchState OnCardPlayed(MatchState state, CardPlayed played)
        {
            if (!state.CurrentPlayerId.Equals(played.Player) || state.Phase != Phase.CardSelect)
            {
                return state;
            }

            var player = Find(state, played.Player);
            if (player == null || !player.Hand.Contains(played.Card))
            {
                return state;
            }

            var updated = player with
            {
                Hand = player.Hand.Remove(played.Card),
                Used = player.Used.Add(played.Card),
            };
            return state.WithPlayer(updated) with { Phase = Phase.Aim };
        }

        static MatchState OnFlicked(MatchState state, Flicked flicked)
        {
            if (!state.CurrentPlayerId.Equals(flicked.Command.Player))
            {
                return state;
            }

            if (state.Phase is not (Phase.Aim or Phase.CardSelect))
            {
                return state;
            }

            return state with { Phase = Phase.Simulating };
        }

        static MatchState OnPassed(MatchState state, PlayerId player)
        {
            if (!state.CurrentPlayerId.Equals(player))
            {
                return state;
            }

            if (state.Phase is Phase.Simulating or Phase.MatchEnd or Phase.RoundEnd)
            {
                return state;
            }

            return Advance(state);
        }

        static MatchState OnTimerExpired(MatchState state, TimerExpired expired)
        {
            if (state.Rules.TurnTimeSeconds <= 0f)
            {
                return state;
            }

            return OnPassed(state, expired.Player);
        }

        static MatchState OnEraserFell(MatchState state, EraserFell fell)
        {
            var victim = Find(state, fell.Victim);
            if (victim == null || !victim.IsAlive)
            {
                return state;
            }

            var stocks = victim.Stocks - 1;
            if (stocks < 0)
            {
                stocks = 0;
            }

            int? eliminated = victim.EliminatedRound;
            if (stocks == 0)
            {
                eliminated = state.Round;
            }

            var next = state.WithPlayer(victim with
            {
                Stocks = stocks,
                EliminatedRound = eliminated,
            });

            switch (state.Rules.WinMode)
            {
                case WinMode.Stock:
                    return next;
                case WinMode.Points:
                    return AwardPoint(next, fell);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(state),
                        state.Rules.WinMode,
                        "Unhandled win mode.");
            }
        }

        static MatchState AwardPoint(MatchState state, EraserFell fell)
        {
            if (fell.LastToucher is not PlayerId toucher || toucher.Equals(fell.Victim))
            {
                return state;
            }

            var scorer = Find(state, toucher);
            if (scorer == null)
            {
                return state;
            }

            return state.WithPlayer(scorer with { Points = scorer.Points + 1 });
        }

        static MatchState OnSimulationSettled(MatchState state)
        {
            if (state.Phase != Phase.Simulating)
            {
                return state;
            }

            return Advance(state);
        }

        static MatchState SetConnected(MatchState state, PlayerId player, bool connected)
        {
            var current = Find(state, player);
            if (current == null)
            {
                return state;
            }

            return state.WithPlayer(current with { Disconnected = !connected });
        }

        static MatchState Advance(MatchState state)
        {
            var nextIndex = state.TurnIndex + 1;
            if (nextIndex >= state.TurnOrder.Length)
            {
                return FinishRound(state);
            }

            return OpenTurn(state with
            {
                TurnIndex = nextIndex,
                Phase = Phase.TurnStart,
            });
        }

        static MatchState OpenTurn(MatchState state)
        {
            while (state.TurnIndex < state.TurnOrder.Length)
            {
                var current = state.CurrentPlayer;
                if (current.IsAlive && !current.Disconnected)
                {
                    return state with
                    {
                        Phase = Phase.CardSelect,
                        TurnTimeLeft = state.Rules.TurnTimeSeconds,
                    };
                }

                var nextIndex = state.TurnIndex + 1;
                if (nextIndex >= state.TurnOrder.Length)
                {
                    return FinishRound(state);
                }

                state = state with
                {
                    TurnIndex = nextIndex,
                    Phase = Phase.TurnStart,
                };
            }

            return FinishRound(state);
        }

        static MatchState FinishRound(MatchState state)
        {
            var alive = 0;
            var actionable = 0;
            foreach (var player in state.Players)
            {
                if (!player.IsAlive)
                {
                    continue;
                }

                alive++;
                if (!player.Disconnected)
                {
                    actionable++;
                }
            }

            if (alive <= 1 || actionable == 0 || RoundCapReached(state))
            {
                return state with { Phase = Phase.MatchEnd };
            }

            return OpenTurn(state with
            {
                Round = state.Round + 1,
                TurnIndex = 0,
                Phase = Phase.TurnStart,
                TurnTimeLeft = state.Rules.TurnTimeSeconds,
            });
        }

        static bool RoundCapReached(MatchState state)
        {
            switch (state.Rules.WinMode)
            {
                case WinMode.Stock:
                    return false;
                case WinMode.Points:
                    return state.Round >= state.Rules.PointRounds;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(state),
                        state.Rules.WinMode,
                        "Unhandled win mode.");
            }
        }

        static PlayerState Find(MatchState state, PlayerId id)
        {
            foreach (var player in state.Players)
            {
                if (player.Id.Equals(id))
                {
                    return player;
                }
            }

            throw new InvalidOperationException($"Unknown player {id}.");
        }
    }
}
