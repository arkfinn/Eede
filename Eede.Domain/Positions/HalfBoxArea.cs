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
            var BoxSize = new PictureSize(boxSize.Width, boxSize.Height);
            var GridSize = new PictureSize(defaultBoxSize.Width / 2, defaultBoxSize.Height / 2);
            // gridSize上の位置
            var Position = new Position(
                localPosition.X / GridSize.Width,
                localPosition.Y / GridSize.Height
            );
            // gridSize上の位置を実際の座標に変換した位置
            var RealPosition = new Position(
                Position.X * GridSize.Width,
                Position.Y * GridSize.Height
            );

            var StartPosition = new Position(startPosition.X, startPosition.Y);

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
            var locationX = Math.Max(0, location.X);
            var newWidth = LimittedDistance(StartPosition.X, locationX, DefaultBoxSize.Width, GridSize.Width, limit.Width);
            var newX = Math.Min(locationX, StartPosition.X);

            var locationY = Math.Max(0, location.Y);
            var newHeight = LimittedDistance(StartPosition.Y, locationY, DefaultBoxSize.Height, GridSize.Height, limit.Height);
            var newY = Math.Min(locationY, StartPosition.Y);

            return With(new PictureSize(newWidth, newHeight), new Position(newX, newY), DefaultBoxSize, StartPosition);
        }

        private int ArrangeTo(int value, int gridLength)
        {
            return value - value % gridLength;
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