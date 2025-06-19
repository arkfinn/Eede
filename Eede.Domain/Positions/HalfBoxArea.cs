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
            int minX = Math.Min(StartPosition.X, location.X);
            int maxX = Math.Max(StartPosition.X, location.X);
            int minY = Math.Min(StartPosition.Y, location.Y);
            int maxY = Math.Max(StartPosition.Y, location.Y);

            // 新しいRealPositionを計算 (常に0以上)
            int newRealX = Math.Max(0, ArrangeTo(minX, GridSize.Width));
            int newRealY = Math.Max(0, ArrangeTo(minY, GridSize.Height));

            // 新しいBoxSizeを計算
            int newBoxWidth = ArrangeTo(maxX, GridSize.Width) - newRealX + DefaultBoxSize.Width;
            int newBoxHeight = ArrangeTo(maxY, GridSize.Height) - newRealY + DefaultBoxSize.Height;

            return With(new PictureSize(newBoxWidth, newBoxHeight), new Position(newRealX, newRealY), DefaultBoxSize, StartPosition);
        }

        private int ArrangeTo(int value, int gridLength)
        {
            // value を gridLength の倍数に切り捨てる
            // C# の % 演算子は負の数に対して負の結果を返すため、調整が必要
            int remainder = value % gridLength;
            return remainder < 0 ? 0 : value - remainder;
        }

        public HalfBoxArea Move(Position localPosition)
        {
            return With(BoxSize, localPosition, DefaultBoxSize, localPosition);
        }

        public HalfBoxArea UpdateSize(PictureSize size)
        {
            return With(BoxSize, RealPosition, size, StartPosition);
        }
    }
}
