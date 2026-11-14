namespace Keshipin.Core
{
    public readonly struct FlickCommand
    {
        public FlickCommand(
            PlayerId player,
            float directionX,
            float directionY,
            float dragLength01,
            float contactOffsetX,
            float contactOffsetY,
            float releaseTime)
        {
            Player = player;
            DirectionX = directionX;
            DirectionY = directionY;
            DragLength01 = dragLength01;
            ContactOffsetX = contactOffsetX;
            ContactOffsetY = contactOffsetY;
            ReleaseTime = releaseTime;
        }

        public PlayerId Player { get; }
        public float DirectionX { get; }
        public float DirectionY { get; }
        public float DragLength01 { get; }
        public float ContactOffsetX { get; }
        public float ContactOffsetY { get; }
        public float ReleaseTime { get; }
    }
}
