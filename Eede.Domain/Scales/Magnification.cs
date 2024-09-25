using System;

namespace Eede.Domain.Scales
{
    /// <summary>
    /// ValueObject
    /// </summary>
    public class Magnification
    {
        public readonly float Value;

        public Magnification(float magnification)
        {
            if (magnification <= 0)
            {
                throw new ArgumentOutOfRangeException("Magnification needs more than 0");
            }
            Value = magnification;
        }

        public override bool Equals(object obj)
        {
            return obj is Magnification magnification &&
                   Value == magnification.Value;
        }

        public override int GetHashCode()
        {
            return -192445598 + Value.GetHashCode();
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