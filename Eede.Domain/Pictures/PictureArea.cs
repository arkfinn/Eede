using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.Pictures
{
    public readonly record struct PictureArea
    {
        public readonly Position Position;
        public readonly PictureSize Size;

        public PictureArea(Position position, PictureSize size)
        {
            Position = position;
            Size = size;
        }

        public int X => Position.X;
        public int Y => Position.Y;
        public int Width => Size.Width;
        public int Height => Size.Height;

        public static PictureArea FromPosition(Position from, Position to, PictureSize limit)
        {
            int newX = Math.Max(0, Math.Min(from.X, to.X));
            int newWidth = Math.Min(Math.Abs(to.X - from.X), limit.Width - newX);

            int newY = Math.Max(0, Math.Min(from.Y, to.Y));
            int newHeight = Math.Min(Math.Abs(to.Y - from.Y), limit.Height - newY);

            return new PictureArea(new Position(newX, newY), new PictureSize(newWidth, newHeight));
        }
    }
}
