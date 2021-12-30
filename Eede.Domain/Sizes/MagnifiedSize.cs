using Eede.Domain.Scales;
using System;
using System.Drawing;

namespace Eede.Domain.Sizes
{
    public class MagnifiedSize
    {
        public readonly Magnification Magnification;

        public readonly int Width;
        public readonly int Height;
        public readonly Size RealSize;

        public MagnifiedSize(Size size, Magnification magnification)
        {
            if (magnification == null)
            {
                throw new ArgumentNullException(nameof(magnification));
            }
            Width = magnification.Magnify(size.Width);
            Height = magnification.Magnify(size.Height);
            Magnification = magnification;
            RealSize = size;
        }

        public Size ToSize()
        {
            return new Size(Width, Height);
        }
    }
}