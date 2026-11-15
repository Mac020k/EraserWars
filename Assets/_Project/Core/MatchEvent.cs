namespace EraserWars.Core
{
    public abstract record MatchEvent;

    public sealed record TurnStarted(PlayerId Player) : MatchEvent;

    public sealed record CardPlayed(PlayerId Player, CardId Card) : MatchEvent;

    public sealed record Flicked(FlickCommand Command) : MatchEvent;

    public sealed record Passed(PlayerId Player) : MatchEvent;

    public sealed record EraserFell(PlayerId Victim, PlayerId? LastToucher) : MatchEvent;

    public sealed record SimulationSettled : MatchEvent;

    public sealed record TimerExpired(PlayerId Player) : MatchEvent;

    public sealed record PlayerDisconnected(PlayerId Player) : MatchEvent;

    public sealed record PlayerReconnected(PlayerId Player) : MatchEvent;
}
