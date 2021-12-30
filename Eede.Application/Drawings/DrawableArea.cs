using Eede.Application.PaintLayers;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application.Drawings
{
    public class DrawableArea
    {
        public DrawableArea(ICanvasBackgroundService background, Magnification magnification, Size gridSize, PositionHistory positionHistory)
        {
            Background = background;
            Magnification = magnification;
            GridSize = gridSize;
            PositionHistory = positionHistory;
        }

        private readonly ICanvasBackgroundService Background;

        private readonly Magnification Magnification;

        private readonly Size GridSize;

        private readonly PositionHistory PositionHistory;

        private MagnifiedSize Magnify(Size size)
        {
            return new MagnifiedSize(size, Magnification);
        }

        public void Paint(Graphics g, Picture source, IImageTransfer imageTransfer)
        {
            // 表示上のサイズ・ポジション DisplaySize DisplayPosition
            var displaySize = Magnify(source.Size);
            var backgroundLayer = new PaintBackgroundLayer(Background);
            var bufferLayer = new PaintBufferLayer(displaySize, source, imageTransfer);
            var gridLayer = new PaintGridLayer(displaySize, Magnify(GridSize));
            backgroundLayer.Paint(g);
            bufferLayer.Paint(g);
            gridLayer.Paint(g);
        }

        public DrawableArea UpdateMagnification(Magnification m)
        {
            return new DrawableArea(Background, m, GridSize, PositionHistory);
        }

        private DrawableArea UpdatePositionHistory(PositionHistory positionHistory)
        {
            return new DrawableArea(Background, Magnification, GridSize, positionHistory);
        }

        private MinifiedPosition RealPositionOf(Position position)
        {
            return new MinifiedPosition(position, Magnification);
        }

        public Size DisplaySizeOf(Picture picture)
        {
            return Magnify(picture.Size).ToSize();
        }

        public Color PickColor(Picture picture, Position displayPosition)
        {
            return picture.PickColor(RealPositionOf(displayPosition).ToPosition());
        }

        private PositionHistory CreatePositionHistory(Position displayPosition)
        {
            return new PositionHistory(RealPositionOf(displayPosition).ToPosition());
        }

        private PositionHistory NextPositionHistory(PositionHistory history, Position displayPosition)
        {
            return history.Update(RealPositionOf(displayPosition).ToPosition());
        }

        public DrawingResult DrawStart(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (IsDrawing()) return new DrawingResult(picture, this);

            var nextHistory = CreatePositionHistory(displayPosition);
            if (!picture.Fetch().Contains(nextHistory.Now))
            {
                return new DrawingResult(picture, this);
            }

            // beginからfinishまでの間情報を保持するクラス
            // Positionhistory, BeforeBuffer, PenStyle, PenCase

            var drawer = new Drawer(picture.Previous, penStyle);
            using (var result = drawStyle.DrawStart(drawer, nextHistory, isShift))
            {
                return new DrawingResult(picture.UpdateDrawing(result), UpdatePositionHistory(nextHistory));
            }
        }

        public DrawingResult Drawing(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(picture, this);

            var nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            var drawer = new Drawer(picture.Fetch(), penStyle);
            using (var result = drawStyle.Drawing(drawer, nextHistory, isShift))
            {
                return new DrawingResult(picture.UpdateDrawing(result), UpdatePositionHistory(nextHistory));
            }
        }

        public DrawingResult DrawEnd(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(picture, this);

            var nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            var drawer = new Drawer(picture.Fetch(), penStyle);
            using (var result = drawStyle.DrawEnd(drawer, nextHistory, isShift))
            {
                return new DrawingResult(picture.DecideDrawing(result), UpdatePositionHistory(null));
            }
        }

        public DrawingResult DrawCancel(DrawingBuffer picture)
        {
            return new DrawingResult(picture.CancelDrawing(), UpdatePositionHistory(null));
        }

        public bool IsDrawing()
        {
            return (PositionHistory != null);
        }
    }
}