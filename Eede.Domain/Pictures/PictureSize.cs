using Eede.Domain.Positions;
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

        public bool Contains(Position pos)
        {
            if (pos.X < 0) return false;
            if (pos.Y < 0) return false;
            if (pos.X >= Width) return false;
            if (pos.Y >= Height) return false;
            return true;
        }
    }
}
