using System;

namespace Eede.Domain.Scales
{
    /// <summary>
    /// ValueObject
    /// </summary>
    public class Magnification
    {
        private readonly float magnification;

        public Magnification(float magnification)
        {
            if (magnification <= 0)
            {
                throw new ArgumentOutOfRangeException("Magnification needs more than 0");
            }
            this.magnification = magnification;
        }

        public override bool Equals(object obj)
        {
            var magnification = obj as Magnification;
            return magnification != null &&
                   this.magnification == magnification.magnification;
        }

        public override int GetHashCode()
        {
            return -192445598 + magnification.GetHashCode();
        }

        public int Magnify(int value)
        {
            return (int)(value * magnification);
        }

        public int Minify(int value)
        {
            return (int)(value / magnification);
        }
    }
}