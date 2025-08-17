using System;

namespace Eede.Domain.Scales
{
    public readonly record struct Magnification
    {
        public readonly float Value;

        public Magnification(float magnification)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(magnification);
            Value = magnification;
        }

        public int Magnify(int value)
        {
            return (int)(value * Value);
        }

        public int Minify(int value)
        {
            return (int)(value / Value);
        }
    }
}
