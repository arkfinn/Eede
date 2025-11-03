namespace Eede.Domain.SharedKernel
{
    public readonly record struct Position
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
