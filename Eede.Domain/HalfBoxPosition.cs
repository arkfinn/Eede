using Eede.Domain.Positions;
using System;
using System.Drawing;

namespace Eede
{
    public class HalfBoxPosition
    {
        public readonly Size BoxSize;
        public readonly Position Position;
        public readonly Position RealPosition;
        private readonly Size MinimumSize;
        private readonly Position StartPosition;

        private HalfBoxPosition(Size boxSize, Point localPosition, Size minimumSize, Point startPosition)
        {
            BoxSize = boxSize;
            MinimumSize = minimumSize;
            var halfW = MinimumSize.Width / 2;
            var halfH = MinimumSize.Height / 2;
            Position = new Position(
                localPosition.X / halfW,
                localPosition.Y / halfH);
            RealPosition = new Position(
                Position.X * halfW,
                Position.Y * halfH
            );

            StartPosition = new Position(startPosition.X, startPosition.Y);
        }

        public HalfBoxPosition(Size boxSize, Point localPosition) : this(boxSize, localPosition, boxSize, localPosition)
        {
        }

        public Rectangle CreateRealRectangle(Size size)
        {
            return new Rectangle(RealPosition.ToPoint(), size);
        }

        public HalfBoxPosition UpdatePosition(Point location)
        {
            var halfW = MinimumSize.Width / 2;
            var halfH = MinimumSize.Height / 2;

            var startPosition = new Position(
               StartPosition.X / halfW * halfW,
               StartPosition.Y / halfH * halfH);
            //x, w
            var x = location.X - startPosition.X;
            if (x < 0)
            {
                x = x * -1 + halfW - 1;
            }
            var newWidth = MinimumSize.Width + (x / halfW * halfW);
            var newX = Math.Min(location.X, StartPosition.X);
            //y, h
            var y = location.Y - startPosition.Y;
            if (y < 0)
            {
                y = y * -1 + halfH - 1;
            }
            var newHeight = MinimumSize.Height + (y / halfH * halfH);
            var newY = Math.Min(location.Y, StartPosition.Y);
            return new HalfBoxPosition(new Size(newWidth, newHeight), new Point(newX, newY), MinimumSize, StartPosition.ToPoint());
        }
    }
}