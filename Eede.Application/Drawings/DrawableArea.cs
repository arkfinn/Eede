using Eede.Application.PaintLayers;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Drawings
{
    public class DrawableArea
    {
        public DrawableArea(Magnification magnification, PictureSize gridSize, PositionHistory positionHistory)
        {
            Magnification = magnification;
            GridSize = gridSize;
            PositionHistory = positionHistory;
        }

        public readonly Magnification Magnification;

        private readonly PictureSize GridSize;

        private readonly PositionHistory PositionHistory;

        private MagnifiedSize Magnify(PictureSize size)
        {
            return new MagnifiedSize(size, Magnification);
        }

        public Picture Painted(DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer)
        {
            if (buffer == null)
            {
                return null;
            }
            Picture source = buffer.Fetch();
            if (source == null)
            {
                return null;
            }
            Picture picture = source;
            // 表示上のサイズ・ポジション DisplaySize DisplayPosition
            MagnifiedSize displaySize = Magnify(source.Size);
            //PaintBackgroundLayer backgroundLayer = new(Background);
            //var picture = backgroundLayer.Painted(null);

            if (!buffer.IsDrawing() && PositionHistory != null)
            {
                CursorLayer cursorLayer = new(displaySize, source, penStyle, PositionHistory.Now, imageTransfer);
                picture = cursorLayer.Painted(picture);
            }
            else
            {
                PaintBufferLayer bufferLayer = new(displaySize, source, imageTransfer);
                picture = bufferLayer.Painted(picture);
            }

            //PaintGridLayer gridLayer = new(displaySize, Magnify(GridSize));
            //picture = gridLayer.Paint(picture);
            return picture;
        }

        public DrawableArea UpdateMagnification(Magnification m)
        {
            return new DrawableArea(m, GridSize, PositionHistory);
        }

        private DrawableArea UpdatePositionHistory(PositionHistory positionHistory)
        {
            return new DrawableArea(Magnification, GridSize, positionHistory);
        }

        private DrawableArea ClearPositionHistory()
        {
            return UpdatePositionHistory(null);
        }



        private MinifiedPosition RealPositionOf(Position position)
        {
            return new MinifiedPosition(position, Magnification);
        }

        public PictureSize DisplaySizeOf(Picture picture)
        {
            MagnifiedSize size = Magnify(picture.Size);
            return new PictureSize(size.Width, size.Height);
        }

        public ArgbColor PickColor(Picture picture, Position displayPosition)
        {
            return picture.PickColor(RealPositionOf(displayPosition).ToPosition());
        }

        private PositionHistory CreatePositionHistory(Position displayPosition)
        {
            return new PositionHistory(RealPositionOf(displayPosition).ToPosition());
        }

        private PositionHistory NextPositionHistory(PositionHistory history, Position displayPosition)
        {
            history ??= CreatePositionHistory(displayPosition);
            return history.Update(RealPositionOf(displayPosition).ToPosition());
        }

        private CoordinateHistory ToCoordinateHistory(PositionHistory history)
        {
            if (history == null) return null;
            var h = new CoordinateHistory(new CanvasCoordinate(history.Start.X, history.Start.Y));
            h = h.Update(new CanvasCoordinate(history.Last.X, history.Last.Y));
            h = h.Update(new CanvasCoordinate(history.Now.X, history.Now.Y));
            return h;
        }

        public DrawingResult DrawStart(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (picture.IsDrawing())
            {
                return new DrawingResult(picture, this);
            }

            PositionHistory nextHistory = CreatePositionHistory(displayPosition);
            if (!picture.Fetch().Contains(nextHistory.Now))
            {
                return new DrawingResult(picture, this);
            }

            DrawingBuffer result = drawStyle.DrawStart(picture, penStyle, ToCoordinateHistory(nextHistory), isShift);
            return new DrawingResult(result, UpdatePositionHistory(nextHistory));
        }

        public DrawingResult Move(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            PositionHistory nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            if (!picture.IsDrawing())
            {
                return new DrawingResult(picture, UpdatePositionHistory(nextHistory));
            }
            DrawingBuffer result = drawStyle.Drawing(picture, penStyle, ToCoordinateHistory(nextHistory), isShift);
            return new DrawingResult(result, UpdatePositionHistory(nextHistory));
        }

        public DrawingResult DrawEnd(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (!picture.IsDrawing())
            {
                return new DrawingResult(picture, this);
            }
            PositionHistory nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            DrawingBuffer result = drawStyle.DrawEnd(picture, penStyle, ToCoordinateHistory(nextHistory), isShift);
            return new DrawingResult(result, ClearPositionHistory());
        }

        public DrawingResult DrawCancel(DrawingBuffer picture)
        {
            if (picture == null)
            {
                return new DrawingResult(null, this);
            }
            return new DrawingResult(picture.CancelDrawing(), ClearPositionHistory());
        }

        public DrawableArea Leave(DrawingBuffer picture)
        {
            return picture.IsDrawing() ? this : ClearPositionHistory();
        }
    }
}
