namespace EraserWars.Core
{
    public sealed record ActiveEffect(
        CardId SourceCard,
        PlayerId Owner,
        PlayerId? Target,
        string SpecKind,
        int TurnsRemaining);
}
