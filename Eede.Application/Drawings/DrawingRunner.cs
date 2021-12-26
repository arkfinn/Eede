using Eede.Domain.PenStyles;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Application.Drawings
{
    public class DrawingRunner : IDisposable
    {
        public DrawingRunner(IPenStyle penStyle, PenCase penCase)
        {
            PenStyle = penStyle;
            PenCase = penCase;
        }

        private readonly IPenStyle PenStyle;
        private readonly PenCase PenCase;

        private PositionHistory PositionHistory = null;

        private Picture DrawingBuffer = null;

        public Picture DrawStart(Picture NowPicture, PaintArea paintArea, Position position, bool isShift)
        {
            if (PositionHistory != null) return NowPicture;

            // PositionHistory = PaintArea.DrawStart(Buffer, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())
            var realPosition = paintArea.RealPositionOf(position).ToPosition();
            if (!NowPicture.Contains(realPosition))
            {
                return NowPicture;
            }

            if (DrawingBuffer != null)
            {
                DrawingBuffer.Dispose();
            }
            DrawingBuffer = NowPicture.Clone();
            // beginからfinishまでの間情報を保持するクラス
            // Positionhistory, BeforeBuffer, PenStyle, PenCase

            PositionHistory = new PositionHistory(realPosition);
            var material = new Drawer(DrawingBuffer, NowPicture, PenCase);
            return PenStyle.DrawStart(material, PositionHistory, isShift);
        }

        public Picture Drawing(Picture NowPicture, PaintArea paintArea, Position position, bool isShift)
        {
            if (PositionHistory == null) return NowPicture;
            // PositionHistory = PaintArea.Drawing(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            PositionHistory = UpdatePositionHistory(paintArea.RealPositionOf(position));
            var material = new Drawer(DrawingBuffer, NowPicture, PenCase);
            return PenStyle.Drawing(material, PositionHistory, isShift);
        }

        public Picture DrawEnd(Picture NowPicture, PaintArea paintArea, Position position, bool isShift)
        {
            if (PositionHistory == null) return NowPicture;
            // PositionHistory = PaintArea.FinishDraw(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            PositionHistory = UpdatePositionHistory(paintArea.RealPositionOf(position));

            var material = new Drawer(DrawingBuffer, NowPicture, PenCase);
            var result = PenStyle.DrawEnd(material, PositionHistory, isShift);
            PositionHistory = null;
            DrawingBuffer.Dispose();
            DrawingBuffer = null;
            return result;
        }

        public Picture DrawCancel()
        {
            var result = DrawingBuffer.Clone();
            PositionHistory = null;
            DrawingBuffer.Dispose();
            DrawingBuffer = null;
            return result;
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
                DrawingBuffer = null;
            }
        }
    }
}