using System;

namespace Eede.Domain.ImageEditing
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
            return (int)Math.Floor(value * Value);
        }

        public int Minify(int value)
        {
            return (int)Math.Floor(value / Value);
        }

        public float Scale(float value)
        {
            return value * Value;
        }

        public float Unscale(float value)
        {
            return value / Value;
        }
    }
}
