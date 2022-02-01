using Eede.Domain.Pictures;
using Eede.Domain.Positions;

namespace Eede.Domain.DrawStyles
{
    public class FreeCurve : IDrawStyle
    {
        public Picture DrawStart(Drawer performer, PositionHistory positionHistory, bool isShift)
        {
            return performer.DrawPoint(positionHistory.Now);
        }

        public Picture Drawing(Drawer performer, PositionHistory positionHistory, bool isShift)
        {
            if (performer.Contains(positionHistory.Now))
            {
                return performer.DrawLine(positionHistory.Last, positionHistory.Now);
            }
            if (performer.Contains(positionHistory.Last))
            {
                return performer.DrawLine(positionHistory.Now, positionHistory.Last);
            }
            return performer.DrawingPicture;
        }

        public Picture DrawEnd(Drawer performer, PositionHistory positionHistory, bool isShift)
        {
            return performer.DrawingPicture;
        }
    }
}