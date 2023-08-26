using Eede.Domain.Drawings;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;

namespace Eede.Domain.DrawStyles
{
    public class FreeCurve : IDrawStyle
    {

        public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            var drawer = new Drawer(buffer.Previous, penStyle);
            return buffer.UpdateDrawing(drawer.DrawPoint(positionHistory.Now));
        }

        public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            var drawer = new Drawer(buffer.Fetch(), penStyle);
            return buffer.UpdateDrawing(Draw(drawer, positionHistory));
        }

        public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            return buffer.DecideDrawing(buffer.Fetch());
        }

        private Picture Draw(Drawer drawer, PositionHistory positionHistory)
        {
            if (drawer.Contains(positionHistory.Now))
            {
                return drawer.DrawLine(positionHistory.Last, positionHistory.Now);
            }
            if (drawer.Contains(positionHistory.Last))
            {
                return drawer.DrawLine(positionHistory.Now, positionHistory.Last);
            }
            return drawer.DrawingPicture;
        }
    }
}