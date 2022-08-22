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

        public void Paint(Graphics g, DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer)
        {
            var source = buffer.Fetch();
            // 表示上のサイズ・ポジション DisplaySize DisplayPosition
            var displaySize = Magnify(source.Size);
            var backgroundLayer = new PaintBackgroundLayer(Background);
            backgroundLayer.Paint(g);

            if (!buffer.IsDrawing() && PositionHistory != null)
            {
                var cursorLayer = new CursorLayer(displaySize, source, penStyle, PositionHistory.Now, imageTransfer);
                cursorLayer.Paint(g);
            }
            else
            {
                var bufferLayer = new PaintBufferLayer(displaySize, source, imageTransfer);
                bufferLayer.Paint(g);
            }

            var gridLayer = new PaintGridLayer(displaySize, Magnify(GridSize));
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

        private DrawableArea ClearPositionHistory()
        {
            return UpdatePositionHistory(null);
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
            if (history == null)
            {
                history = CreatePositionHistory(displayPosition);
            }
            return history.Update(RealPositionOf(displayPosition).ToPosition());
        }

        public DrawingResult DrawStart(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (picture.IsDrawing())
            {
                return new DrawingResult(picture, this);
            }

            var nextHistory = CreatePositionHistory(displayPosition);
            if (!picture.Fetch().Contains(nextHistory.Now))
            {
                return new DrawingResult(picture, this);
            }

            var drawer = new Drawer(picture.Previous, penStyle);
            using (var result = drawStyle.DrawStart(drawer, nextHistory, isShift))
            {
                return new DrawingResult(picture.UpdateDrawing(result), UpdatePositionHistory(nextHistory));
            }
        }

        public DrawingResult Move(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            var nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            if (!picture.IsDrawing())
            {
                return new DrawingResult(picture, UpdatePositionHistory(nextHistory));
            }
            var drawer = new Drawer(picture.Fetch(), penStyle);
            using (var result = drawStyle.Drawing(drawer, nextHistory, isShift))
            {
                return new DrawingResult(picture.UpdateDrawing(result), UpdatePositionHistory(nextHistory));
            }
        }

        public DrawingResult DrawEnd(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (!picture.IsDrawing())
            {
                return new DrawingResult(picture, this);
            }

            var nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            var drawer = new Drawer(picture.Fetch(), penStyle);
            using (var result = drawStyle.DrawEnd(drawer, nextHistory, isShift))
            {
                return new DrawingResult(picture.DecideDrawing(result), ClearPositionHistory());
            }
        }

        public DrawingResult DrawCancel(DrawingBuffer picture)
        {
            return new DrawingResult(picture.CancelDrawing(), ClearPositionHistory());
        }

        public DrawableArea Leave(DrawingBuffer picture)
        {
            if (picture.IsDrawing())
            {
                return this;
            }
            return ClearPositionHistory();
        }
    }
}