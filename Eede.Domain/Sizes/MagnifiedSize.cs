using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using System;

namespace Eede.Domain.Sizes
{
    public class MagnifiedSize
    {
        public readonly Magnification Magnification;

        public readonly int Width;
        public readonly int Height;
        public readonly PictureSize BaseSize;

        public MagnifiedSize(PictureSize size, Magnification magnification)
        {
            if (magnification == null)
            {
                throw new ArgumentNullException(nameof(magnification));
            }
            Width = magnification.Magnify(size.Width);
            Height = magnification.Magnify(size.Height);
            Magnification = magnification;
            BaseSize = size;
        }
    }
}