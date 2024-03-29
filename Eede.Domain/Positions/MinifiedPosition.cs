﻿using Eede.Domain.Scales;
using System;

namespace Eede.Domain.Positions
{
    public class MinifiedPosition
    {
        public readonly Magnification Magnification;

        public int X { get; }
        public int Y { get; }

        public MinifiedPosition(Position position, Magnification magnification)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }
            if (magnification == null)
            {
                throw new ArgumentNullException(nameof(magnification));
            }
            X = magnification.Minify(position.X);
            Y = magnification.Minify(position.Y);
            Magnification = magnification;
        }

        public Position ToPosition()
        {
            return new Position(X, Y);
        }
    }
}