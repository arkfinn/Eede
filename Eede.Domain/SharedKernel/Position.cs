using System.Text.Json.Serialization;

namespace Eede.Domain.SharedKernel
{
    public readonly record struct Position
    {
        public int X { get; }
        public int Y { get; }

        [JsonConstructor]
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Position operator +(Position a, Position b)
        {
            return new Position(a.X + b.X, a.Y + b.Y);
        }
    }
}
