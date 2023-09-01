using Eede.Domain.Drawings;
using Eede.Domain.Positions;

namespace Eede.Domain.DrawStyles
{
    // 実装内容を再検討する
    public class Fill:IDrawStyle
    {
        public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            var drawer = new Drawer(buffer.Previous, penStyle);
            return buffer.UpdateDrawing(drawer.Fill(positionHistory.Now));
        }

        public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            return buffer;
        }

        public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            return buffer.DecideDrawing(buffer.Fetch());
        }

    }
}