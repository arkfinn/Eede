#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing
{
    public class MagnifiedSize(PictureSize size, Magnification magnification)
    {
        public readonly Magnification Magnification = magnification;

        public readonly int Width = magnification.Magnify(size.Width);
        public readonly int Height = magnification.Magnify(size.Height);
        public readonly PictureSize BaseSize = size;
    }
}
