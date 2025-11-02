using Eede.Domain.Scales;
using System;

namespace Eede.Domain.Positions
{
    public class MinifiedPosition(Position position, Magnification magnification)
    {
        public readonly Magnification Magnification = magnification;

        public int X { get; } = magnification.Minify(position.X);
        public int Y { get; } = magnification.Minify(position.Y);

        public Position ToPosition()
        {
            return new Position(X, Y);
        }
    }
}
