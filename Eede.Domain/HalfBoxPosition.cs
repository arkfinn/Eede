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
        private readonly Size DefaultCursorSize;
        private readonly Size HalfCursorSize;
        private readonly Position StartPosition;

        private HalfBoxPosition(Size boxSize, Point localPosition, Size minimumSize, Point startPosition)
        {
            BoxSize = boxSize;
            DefaultCursorSize = minimumSize;
            HalfCursorSize = new Size(DefaultCursorSize.Width / 2, DefaultCursorSize.Height / 2);
            Position = new Position(
                localPosition.X / HalfCursorSize.Width,
                localPosition.Y / HalfCursorSize.Height);
            RealPosition = new Position(
                Position.X * HalfCursorSize.Width,
                Position.Y * HalfCursorSize.Height
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
            var startPosition = new Position(
               ArrangeTo(StartPosition.X, HalfCursorSize.Width),
               ArrangeTo(StartPosition.Y, HalfCursorSize.Height));
            //x, w
            var x = location.X - startPosition.X;
            if (x < 0)
            {
                x = Math.Abs(x) + HalfCursorSize.Width - 1;
            }
            var newWidth = DefaultCursorSize.Width + ArrangeTo(x, HalfCursorSize.Width);
            var newX = Math.Min(location.X, StartPosition.X);
            //y, h
            var y = location.Y - startPosition.Y;
            if (y < 0)
            {
                y = Math.Abs(y) + HalfCursorSize.Height - 1;
            }
            var newHeight = DefaultCursorSize.Height + ArrangeTo(y, HalfCursorSize.Height);
            var newY = Math.Min(location.Y, StartPosition.Y);
            return new HalfBoxPosition(new Size(newWidth, newHeight), new Point(newX, newY), DefaultCursorSize, StartPosition.ToPoint());
        }

        private int ArrangeTo(int value, int gridLength)
        {
            return value - (value % gridLength);
        }
    }
}