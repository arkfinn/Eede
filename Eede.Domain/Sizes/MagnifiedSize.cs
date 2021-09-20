using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Sizes
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
