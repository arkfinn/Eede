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
        private readonly Size GridSize;
        private readonly Position StartPosition;

        private HalfBoxPosition(Size boxSize, Point localPosition, Size minimumSize, Point startPosition)
        {
            BoxSize = boxSize;
            DefaultCursorSize = minimumSize;
            GridSize = new Size(DefaultCursorSize.Width / 2, DefaultCursorSize.Height / 2);
            Position = new Position(
                localPosition.X / GridSize.Width,
                localPosition.Y / GridSize.Height
            );
            RealPosition = new Position(
                Position.X * GridSize.Width,
                Position.Y * GridSize.Height
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

        public HalfBoxPosition UpdatePosition(Point location, Size limit)
        {
            var locationX = Math.Max(0, location.X);
            var newWidth = LimittedDistance(StartPosition.X, locationX, DefaultCursorSize.Width, GridSize.Width, limit.Width);
            var newX = Math.Min(locationX, StartPosition.X);

            var locationY = Math.Max(0, location.Y);
            var newHeight = LimittedDistance(StartPosition.Y, locationY, DefaultCursorSize.Height, GridSize.Height, limit.Height);
            var newY = Math.Min(locationY, StartPosition.Y);

            return new HalfBoxPosition(new Size(newWidth, newHeight), new Point(newX, newY), DefaultCursorSize, StartPosition.ToPoint());
        }

        private int ArrangeTo(int value, int gridLength)
        {
            return value - (value % gridLength);
        }

        private int LimittedDistance(int startValue, int nowValue, int cursorLength, int gridLength, int limitLength)
        {
            var startGrid = ArrangeTo(startValue, gridLength);
            return Math.Min(
                cursorLength + ArrangeFromDistance(nowValue - startGrid, gridLength),
                limitLength - ArrangeTo(startValue, gridLength)
            );
        }

        private int ArrangeFromDistance(int distance, int gridLength)
        {
            // マイナス方向に広がる場合、元のサイズプラスマイナス方向分とする。
            if (distance < 0)
            {
                return ArrangeTo(Math.Abs(distance) + gridLength - 1, gridLength);
            }
            return ArrangeTo(distance, gridLength);
        }
    }
}