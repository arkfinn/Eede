using Eede.Domain.Pictures;
using System;

namespace Eede.Domain.Positions
{
    public class HalfBoxArea
    {
        public readonly PictureSize BoxSize;
        public readonly Position GridPosition;
        public readonly Position RealPosition;
        private readonly PictureSize DefaultBoxSize;
        private readonly PictureSize GridSize;
        private readonly Position StartPosition;

        public static HalfBoxArea Create(PictureSize boxSize, Position localPosition)
        {
            return With(boxSize, localPosition, boxSize, localPosition);
        }

        private static HalfBoxArea With(PictureSize boxSize, Position localPosition, PictureSize defaultBoxSize, Position startPosition)
        {
            PictureSize BoxSize = new(boxSize.Width, boxSize.Height);
            PictureSize GridSize = new(defaultBoxSize.Width / 2, defaultBoxSize.Height / 2);
            // gridSize上の位置
            Position Position = new(
                localPosition.X / GridSize.Width,
                localPosition.Y / GridSize.Height
            );
            // gridSize上の位置を実際の座標に変換した位置
            Position RealPosition = new(
                Position.X * GridSize.Width,
                Position.Y * GridSize.Height
            );

            Position StartPosition = new(startPosition.X, startPosition.Y);

            return new HalfBoxArea(BoxSize, Position, RealPosition, defaultBoxSize, GridSize, StartPosition);
        }

        private HalfBoxArea(
            PictureSize boxSize,
            Position gridPosition,
            Position realPosition,
            PictureSize defaultBoxSize,
            PictureSize gridSize,
            Position startPosition)
        {
            BoxSize = boxSize;
            GridPosition = gridPosition;
            RealPosition = realPosition;
            DefaultBoxSize = defaultBoxSize;
            GridSize = gridSize;
            StartPosition = startPosition;
        }

        public PictureArea CreateRealArea(PictureSize size)
        {
            return new PictureArea(RealPosition, size);
        }

        public HalfBoxArea UpdatePosition(Position location, PictureSize limit)
        {
            int locationX = Math.Max(0, location.X);
            int newWidth = LimittedDistance(StartPosition.X, locationX, DefaultBoxSize.Width, GridSize.Width, limit.Width);
            int newX = Math.Min(locationX, StartPosition.X);

            int locationY = Math.Max(0, location.Y);
            int newHeight = LimittedDistance(StartPosition.Y, locationY, DefaultBoxSize.Height, GridSize.Height, limit.Height);
            int newY = Math.Min(locationY, StartPosition.Y);

            return With(new PictureSize(newWidth, newHeight), new Position(newX, newY), DefaultBoxSize, StartPosition);
        }

        private int ArrangeTo(int value, int gridLength)
        {
            return value - (value % gridLength);
        }

        private int LimittedDistance(int startValue, int nowValue, int cursorLength, int gridLength, int limitLength)
        {
            int startGrid = ArrangeTo(startValue, gridLength);
            return Math.Min(
                cursorLength + ArrangeFromDistance(nowValue - startGrid, gridLength),
                limitLength - ArrangeTo(startValue, gridLength)
            );
        }

        private int ArrangeFromDistance(int distance, int gridLength)
        {
            // マイナス方向に広がる場合、元のサイズプラスマイナス方向分とする。
            return distance < 0 ? ArrangeTo(Math.Abs(distance) + gridLength - 1, gridLength) : ArrangeTo(distance, gridLength);
        }

        public HalfBoxArea Move(Position localPosition)
        {
            return With(BoxSize, localPosition, DefaultBoxSize, localPosition);
        }
    }
}