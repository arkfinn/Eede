using Eede.Domain.PenStyles;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using System;

namespace Eede.Application.Drawings
{
    public class DrawingRunner : IDisposable
    {
        public DrawingRunner(IPenStyle penStyle, PenCase penCase, Magnification m)
        {
            PenStyle = penStyle;
            PenCase = penCase;
            this.m = m;
        }

        private readonly IPenStyle PenStyle;
        private readonly PenCase PenCase;
        private readonly Magnification m = new Magnification(1);

        private PositionHistory PositionHistory = null;

        private AlphaPicture DrawingBuffer = null;

        public AlphaPicture DrawStart(AlphaPicture NowPicture, Position position, bool isShift)
        {
            if (PositionHistory != null) return NowPicture;

            // PositionHistory = PaintArea.DrawStart(Buffer, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())
            var pos = new MinifiedPosition(position, m).ToPosition();
            if (!NowPicture.Contains(pos))
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

            PositionHistory = new PositionHistory(pos);
            var material = new DrawingPerformer(DrawingBuffer, NowPicture, PenCase);
            return PenStyle.DrawStart(material, PositionHistory, isShift);
        }

        public AlphaPicture Drawing(AlphaPicture NowPicture, Position position, bool isShift)
        {
            if (PositionHistory == null) return NowPicture;
            // PositionHistory = PaintArea.Drawing(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var pos = new MinifiedPosition(position, m);
            UpdatePositionHistory(pos);
            var material = new DrawingPerformer(DrawingBuffer, NowPicture, PenCase);
            return PenStyle.Drawing(material, PositionHistory, isShift);
        }

        public AlphaPicture DrawEnd(AlphaPicture NowPicture, Position position, bool isShift)
        {
            if (PositionHistory == null) return NowPicture;
            // PositionHistory = PaintArea.FinishDraw(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var pos = new MinifiedPosition(position, m);
            UpdatePositionHistory(pos);

            var material = new DrawingPerformer(DrawingBuffer, NowPicture, PenCase);
            var result = PenStyle.DrawEnd(material, PositionHistory, isShift);
            PositionHistory = null;
            DrawingBuffer.Dispose();
            DrawingBuffer = null;
            return result;
        }

        public AlphaPicture DrawCancel()
        {
            var result = DrawingBuffer.Clone();
            PositionHistory = null;
            DrawingBuffer.Dispose();
            DrawingBuffer = null;
            return result;
        }

        private void UpdatePositionHistory(MinifiedPosition pos)
        {
            PositionHistory = PositionHistory.Update(pos.ToPosition());
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