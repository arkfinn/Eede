using Eede.Domain.PenStyles;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Application.Drawings
{
    public class DrawingRunner : IDisposable
    {
        public DrawingRunner(IPenStyle penStyle, PenCase penCase) : this(penStyle, penCase, null, null)
        {
        }

        private DrawingRunner(IPenStyle penStyle, PenCase penCase, PositionHistory positionHistory, Picture drawingBuffer)
        {
            PenStyle = penStyle;
            PenCase = penCase;
            PositionHistory = positionHistory;
            DrawingBuffer = drawingBuffer?.Clone();
        }

        private readonly IPenStyle PenStyle;
        private readonly PenCase PenCase;
        private readonly PositionHistory PositionHistory;
        private readonly Picture DrawingBuffer;

        public DrawingResult DrawStart(Picture NowPicture, PaintArea paintArea, Position position, bool isShift)
        {
            if (IsDrawing()) return new DrawingResult(NowPicture, this);

            // PositionHistory = PaintArea.DrawStart(Buffer, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())
            var realPosition = paintArea.RealPositionOf(position).ToPosition();
            if (!NowPicture.Contains(realPosition))
            {
                return new DrawingResult(NowPicture, this);
            }

            // beginからfinishまでの間情報を保持するクラス
            // Positionhistory, BeforeBuffer, PenStyle, PenCase

            var nextHistory = new PositionHistory(realPosition);
            var material = new Drawer(NowPicture, NowPicture, PenCase);
            return new DrawingResult(PenStyle.DrawStart(material, nextHistory, isShift), Update(nextHistory, NowPicture));
        }

        public DrawingResult Drawing(Picture NowPicture, PaintArea paintArea, Position position, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(NowPicture, this);
            // PositionHistory = PaintArea.Drawing(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var nextHistory = UpdatePositionHistory(paintArea.RealPositionOf(position));
            var material = new Drawer(DrawingBuffer, NowPicture, PenCase);
            return new DrawingResult(PenStyle.Drawing(material, nextHistory, isShift), Update(nextHistory, DrawingBuffer));
        }

        public DrawingResult DrawEnd(Picture NowPicture, PaintArea paintArea, Position position, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(NowPicture, this);
            // PositionHistory = PaintArea.FinishDraw(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var nextHistory = UpdatePositionHistory(paintArea.RealPositionOf(position));

            var material = new Drawer(DrawingBuffer, NowPicture, PenCase);
            var result = PenStyle.DrawEnd(material, nextHistory, isShift);
            return new DrawingResult(result, Update(null, null));
        }

        public DrawingResult DrawCancel()
        {
            var result = DrawingBuffer.Clone();
            return new DrawingResult(result, Update(null, null));
        }

        private DrawingRunner Update(PositionHistory positionHistory, Picture drawingBuffer)
        {
            return new DrawingRunner(PenStyle, PenCase, positionHistory, drawingBuffer);
        }

        private PositionHistory UpdatePositionHistory(MinifiedPosition pos)
        {
            return PositionHistory.Update(pos.ToPosition());
        }

        public bool IsDrawing()
        {
            return (PositionHistory != null);
        }

        public void Dispose()
        {
            if (DrawingBuffer != null)
            {
                DrawingBuffer.Dispose();
            }
        }
    }
}