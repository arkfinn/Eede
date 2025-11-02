using Eede.Domain.Scales;
using System;

namespace Eede.Domain.Sizes
{
    public class MagnifiedSize(PictureSize size, Magnification magnification)
    {
        public readonly Magnification Magnification = magnification;

        public readonly int Width = magnification.Magnify(size.Width);
        public readonly int Height = magnification.Magnify(size.Height);
        public readonly PictureSize BaseSize = size;
    }
}
