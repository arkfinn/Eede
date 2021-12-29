using Eede.Domain.DrawStyles;
using Eede.Domain.Positions;

namespace Eede.Application.Drawings
{
    public class DrawingRunner
    {
        public DrawingRunner(IDrawStyle penStyle, PenStyle penCase) : this(penStyle, penCase, null)
        {
        }

        private DrawingRunner(IDrawStyle penStyle, PenStyle penCase, PositionHistory positionHistory)
        {
            PenStyle = penStyle;
            PenCase = penCase;
            PositionHistory = positionHistory;
        }

        private readonly IDrawStyle PenStyle;
        private readonly PenStyle PenCase;
        private readonly PositionHistory PositionHistory;

        public DrawingResult DrawStart(DrawingBuffer picture, PaintArea paintArea, Position position, bool isShift)
        {
            if (IsDrawing()) return new DrawingResult(picture, this);

            // PositionHistory = PaintArea.DrawStart(Buffer, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())
            var realPosition = paintArea.RealPositionOf(position).ToPosition();
            if (!picture.Fetch().Contains(realPosition))
            {
                return new DrawingResult(picture, this);
            }

            // beginからfinishまでの間情報を保持するクラス
            // Positionhistory, BeforeBuffer, PenStyle, PenCase

            var nextHistory = new PositionHistory(realPosition);
            var material = new Drawer(picture.Previous, PenCase);
            return new DrawingResult(picture.UpdateDrawing(PenStyle.DrawStart(material, nextHistory, isShift)), Update(nextHistory));
        }

        public DrawingResult Drawing(DrawingBuffer picture, PaintArea paintArea, Position position, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(picture, this);
            // PositionHistory = PaintArea.Drawing(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var nextHistory = UpdatePositionHistory(paintArea.RealPositionOf(position));
            var material = new Drawer(picture.Fetch(), PenCase);
            return new DrawingResult(picture.UpdateDrawing(PenStyle.Drawing(material, nextHistory, isShift)), Update(nextHistory));
        }

        public DrawingResult DrawEnd(DrawingBuffer picture, PaintArea paintArea, Position position, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(picture, this);
            // PositionHistory = PaintArea.FinishDraw(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var nextHistory = UpdatePositionHistory(paintArea.RealPositionOf(position));

            var material = new Drawer(picture.Fetch(), PenCase);
            var result = PenStyle.DrawEnd(material, nextHistory, isShift);
            return new DrawingResult(picture.DecideDrawing(result), Update(null));
        }

        public DrawingResult DrawCancel(DrawingBuffer picture)
        {
            //var result = DrawingBuffer.Clone();
            return new DrawingResult(picture.CancelDrawing(), Update(null));
        }

        private DrawingRunner Update(PositionHistory positionHistory)
        {
            return new DrawingRunner(PenStyle, PenCase, positionHistory);
        }

        private PositionHistory UpdatePositionHistory(MinifiedPosition pos)
        {
            return PositionHistory.Update(pos.ToPosition());
        }

        public bool IsDrawing()
        {
            return (PositionHistory != null);
        }
    }
}