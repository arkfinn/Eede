namespace Eede.Domain.Positions
{
    public record Position
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
