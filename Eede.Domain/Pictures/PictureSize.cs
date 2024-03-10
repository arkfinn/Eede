using Eede.Domain.Positions;
using System;

namespace Eede.Domain.Pictures
{
    public class PictureSize
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public PictureSize(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentException($"invalid argument.width:{width}, height:{height}");
            }
            Width = width;
            Height = height;
        }

        public bool Contains(Position pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public PictureSize Swap()
        {
            return new PictureSize(Height, Width);
        }
    }
}
