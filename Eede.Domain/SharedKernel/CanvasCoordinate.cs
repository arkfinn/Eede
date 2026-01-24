namespace Eede.Domain.SharedKernel
{
    public readonly record struct CanvasCoordinate
    {
        public int X { get; }
        public int Y { get; }

        public CanvasCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position ToPosition()
        {
            return new Position(X, Y);
        }
    }
}
