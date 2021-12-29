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
            DrawStyle = penStyle;
            PenStyle = penCase;
            PositionHistory = positionHistory;
        }

        private readonly IDrawStyle DrawStyle;
        private readonly PenStyle PenStyle;
        private readonly PositionHistory PositionHistory;

        public DrawingResult DrawStart(DrawingBuffer picture, DrawableArea paintArea, Position position, bool isShift)
        {
            if (IsDrawing()) return new DrawingResult(picture, this);

            var nextHistory = paintArea.CreatePositionHistory(position);
            if (!picture.Fetch().Contains(nextHistory.Now))
            {
                return new DrawingResult(picture, this);
            }

            // beginからfinishまでの間情報を保持するクラス
            // Positionhistory, BeforeBuffer, PenStyle, PenCase

            var material = new Drawer(picture.Previous, PenStyle);
            return new DrawingResult(picture.UpdateDrawing(DrawStyle.DrawStart(material, nextHistory, isShift)), Update(nextHistory));
        }

        public DrawingResult Drawing(DrawingBuffer picture, DrawableArea paintArea, Position position, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(picture, this);

            var nextHistory = paintArea.NextPositionHistory(PositionHistory, position);
            var material = new Drawer(picture.Fetch(), PenStyle);
            return new DrawingResult(picture.UpdateDrawing(DrawStyle.Drawing(material, nextHistory, isShift)), Update(nextHistory));
        }

        public DrawingResult DrawEnd(DrawingBuffer picture, DrawableArea paintArea, Position position, bool isShift)
        {
            if (!IsDrawing()) return new DrawingResult(picture, this);

            var nextHistory = paintArea.NextPositionHistory(PositionHistory, position);
            var material = new Drawer(picture.Fetch(), PenStyle);
            var result = DrawStyle.DrawEnd(material, nextHistory, isShift);
            return new DrawingResult(picture.DecideDrawing(result), Update(null));
        }

        public DrawingResult DrawCancel(DrawingBuffer picture)
        {
            //var result = DrawingBuffer.Clone();
            return new DrawingResult(picture.CancelDrawing(), Update(null));
        }

        private DrawingRunner Update(PositionHistory positionHistory)
        {
            return new DrawingRunner(DrawStyle, PenStyle, positionHistory);
        }

        public bool IsDrawing()
        {
            return (PositionHistory != null);
        }
    }
}