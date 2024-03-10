using Eede.Domain.Positions;
using System;

namespace Eede.Domain.Pictures
{
    public class PictureArea
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

        public PictureArea UpdatePosition(Position location, PictureSize limit)
        {
            int locationX = Math.Max(0, location.X);
            int newX = Math.Min(locationX, X);
            int newWidth = Math.Min(Distance(X, locationX), limit.Width);

            int locationY = Math.Max(0, location.Y);
            int newHeight = Math.Min(Distance(Y, locationY), limit.Height);
            int newY = Math.Min(locationY, Y);

            return new PictureArea(new Position(newX, newY), new PictureSize(newWidth, newHeight));
        }

        private int Distance(int from, int to)
        {
            return Math.Abs(to - from);
        }
    }
}
