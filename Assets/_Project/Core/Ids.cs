namespace EraserWars.Core
{
    public readonly record struct PlayerId(int Value)
    {
        public override string ToString() => $"P{Value}";
    }

    public readonly record struct CharacterId(int Value)
    {
        public override string ToString() => $"C{Value}";
    }

    public readonly record struct CardId(int Value)
    {
        public override string ToString() => $"Card{Value}";
    }
}
