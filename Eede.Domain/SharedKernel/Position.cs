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
    }
}
