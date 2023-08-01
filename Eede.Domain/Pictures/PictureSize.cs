using System;

namespace Eede.Domain.Pictures
{
    public class PictureSize
    {
        public readonly int Width;
        public readonly int Height;

        public PictureSize(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentException($"invalid argument.width:{width}, height:{height}");
            }
            Width = width;
            Height = height;
        }
    }
}
