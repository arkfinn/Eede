using Eede.Application.PaintLayers;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Palettes;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.SharedKernel;
using Eede.Domain.Sizes;

namespace Eede.Application.Drawings
{
    public class DrawableArea
    {
        public DrawableArea(ICanvasBackgroundService background, Magnification magnification, PictureSize gridSize, PositionHistory positionHistory)
        {
            Background = background;
            Magnification = magnification;
            GridSize = gridSize;
            PositionHistory = positionHistory;
        }

        private readonly ICanvasBackgroundService Background;

        public readonly Magnification Magnification;

        private readonly PictureSize GridSize;

        private readonly PositionHistory PositionHistory;

        private MagnifiedSize Magnify(PictureSize size)
        {
            return new MagnifiedSize(size, Magnification);
        }

        public Picture Painted(DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer)
        {
            Picture source = buffer.Fetch();
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

            DrawingBuffer result = drawStyle.DrawStart(picture, penStyle, nextHistory, isShift);
            return new DrawingResult(result, UpdatePositionHistory(nextHistory));
        }

        public DrawingResult Move(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            PositionHistory nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            if (!picture.IsDrawing())
            {
                return new DrawingResult(picture, UpdatePositionHistory(nextHistory));
            }
            DrawingBuffer result = drawStyle.Drawing(picture, penStyle, nextHistory, isShift);
            return new DrawingResult(result, UpdatePositionHistory(nextHistory));
        }

        public DrawingResult DrawEnd(IDrawStyle drawStyle, PenStyle penStyle, DrawingBuffer picture, Position displayPosition, bool isShift)
        {
            if (!picture.IsDrawing())
            {
                return new DrawingResult(picture, this);
            }
            PositionHistory nextHistory = NextPositionHistory(PositionHistory, displayPosition);
            DrawingBuffer result = drawStyle.DrawEnd(picture, penStyle, nextHistory, isShift);
            return new DrawingResult(result, ClearPositionHistory());
        }

        public DrawingResult DrawCancel(DrawingBuffer picture)
        {
            return new DrawingResult(picture.CancelDrawing(), ClearPositionHistory());
        }

        public DrawableArea Leave(DrawingBuffer picture)
        {
            return picture.IsDrawing() ? this : ClearPositionHistory();
        }
    }
}
