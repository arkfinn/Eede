using Eede.Domain.Scales;
using System;
using System.Drawing;

namespace Eede.Domain.Sizes
{
    public class MagnifiedSize
    {
        public readonly Magnification Magnification;

        public int Width { get; }
        public int Height { get; }

        public MagnifiedSize(Size size, Magnification magnification)
        {
            if (magnification == null)
            {
                throw new ArgumentNullException(nameof(magnification));
            }
            Width = magnification.Magnify(size.Width);
            Height = magnification.Magnify(size.Height);
            Magnification = magnification;
        }

        public Size ToSize()
        {
            return new Size(Width, Height);
        }
    }
}