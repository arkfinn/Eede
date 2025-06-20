using System;
using Eede.Domain.Pictures;

namespace Eede.Domain.Positions
{
    /// <summary>
    /// 半分のボックス領域を表すクラス。
    /// グリッドシステムに基づいた位置とサイズを管理します。
    /// </summary>
    public class HalfBoxArea
    {
        /// <summary>
        /// ボックスのサイズ。
        /// </summary>
        public readonly PictureSize BoxSize;
        /// <summary>
        /// グリッド座標系における位置。
        /// </summary>
        public readonly Position GridPosition;
        /// <summary>
        /// 実際のピクセル座標系における位置。
        /// </summary>
        public readonly Position RealPosition;

        private readonly PictureSize DefaultBoxSize;
        private readonly PictureSize GridSize;
        private readonly Position StartPosition;

        /// <summary>
        /// HalfBoxAreaの新しいインスタンスを生成します。
        /// このメソッドは、defaultBoxSizeとstartPositionがboxSizeとlocalPositionと同じ初期値を持つ場合に利用されます。
        /// </summary>
        public static HalfBoxArea Create(Position localPosition, PictureSize boxSize)
        {
            // GridSizeはdefaultBoxSizeから計算されるため、ここではdefaultBoxSizeをboxSizeとして渡す
            PictureSize gridSize = new(boxSize.Width / 2, boxSize.Height / 2);
            return With(localPosition, boxSize, boxSize, localPosition, gridSize);
        }

        private static HalfBoxArea With(Position localPosition, PictureSize boxSize, PictureSize defaultBoxSize, Position startPosition, PictureSize gridSize)
        {
            PictureSize newBoxSize = new(boxSize.Width, boxSize.Height);
            // gridSize上の位置
            Position gridPosition = new(
                localPosition.X / gridSize.Width,
                localPosition.Y / gridSize.Height
            );
            // gridSize上の位置を実際の座標に変換した位置
            Position realPosition = new(
                gridPosition.X * gridSize.Width,
                gridPosition.Y * gridSize.Height
            );

            Position newStartPosition = new(startPosition.X, startPosition.Y);

            return new HalfBoxArea(newBoxSize, gridPosition, realPosition, defaultBoxSize, newStartPosition, gridSize);
        }

        private HalfBoxArea(
            PictureSize boxSize,
            Position gridPosition,
            Position realPosition,
            PictureSize defaultBoxSize,
            Position startPosition,
            PictureSize gridSize)
        {
            BoxSize = boxSize;
            GridPosition = gridPosition;
            RealPosition = realPosition;
            DefaultBoxSize = defaultBoxSize;
            GridSize = gridSize; // コンストラクタで受け取る
            StartPosition = startPosition;
        }

        public PictureArea CreateRealArea(PictureSize size)
        {
            return new PictureArea(RealPosition, size);
        }

        public HalfBoxArea ResizeToLocation(Position location)
        {
            BoundingBoxCoordinates coordinates = CalculateBoundingBoxCoordinates(location);
            PictureArea boxArea = CalculateBoxDimensions(coordinates);

            return With(boxArea.Position, boxArea.Size, DefaultBoxSize, StartPosition, GridSize);
        }

        private BoundingBoxCoordinates CalculateBoundingBoxCoordinates(Position location)
        {
            var minX = Math.Min(StartPosition.X, location.X);
            var maxX = Math.Max(StartPosition.X, location.X);
            var minY = Math.Min(StartPosition.Y, location.Y);
            var maxY = Math.Max(StartPosition.Y, location.Y);
            return new BoundingBoxCoordinates(minX, maxX, minY, maxY);
        }

        private PictureArea CalculateBoxDimensions(BoundingBoxCoordinates coordinates)
        {
            var realX = SnapToGrid(coordinates.MinX, GridSize.Width);
            var realY = SnapToGrid(coordinates.MinY, GridSize.Height);
            var boxWidth = SnapToGrid(coordinates.MaxX, GridSize.Width) - realX + DefaultBoxSize.Width;
            var boxHeight = SnapToGrid(coordinates.MaxY, GridSize.Height) - realY + DefaultBoxSize.Height;
            return new PictureArea(new Position(realX, realY), new PictureSize(boxWidth, boxHeight));
        }

        /// <summary>
        /// 値をグリッドサイズにスナップします。
        /// 負の値は0にスナップされます。
        /// </summary>
        private int SnapToGrid(int value, int gridSize)
        {
            int remainder = value % gridSize;
            return remainder < 0 ? 0 : value - remainder;
        }

        public HalfBoxArea Move(Position localPosition)
        {
            return With(localPosition, BoxSize, DefaultBoxSize, localPosition, GridSize);
        }

        private readonly struct BoundingBoxCoordinates(int minX, int maxX, int minY, int maxY)
        {
            public readonly int MinX = minX;
            public readonly int MaxX = maxX;
            public readonly int MinY = minY;
            public readonly int MaxY = maxY;
        }
    }
}
