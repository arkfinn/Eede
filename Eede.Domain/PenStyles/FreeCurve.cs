using Eede.Domain.Pictures;
using Eede.Domain.Positions;

namespace Eede.Domain.PenStyles
{
    public class FreeCurve : IPenStyle
    {
        public Picture DrawStart(DrawingPerformer performer, PositionHistory positionHistory, bool isShift)
        {
            return performer.DrawPoint(positionHistory.Now);
        }

        public Picture Drawing(DrawingPerformer performer, PositionHistory positionHistory, bool isShift)
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

        public Picture DrawEnd(DrawingPerformer performer, PositionHistory positionHistory, bool isShift)
        {
            return performer.DrawingPicture;
        }
    }
}