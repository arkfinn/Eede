using Eede.Domain.Pictures;
using Eede.Domain.Positions;

namespace Eede.Domain.PenStyles
{
    public interface IPenStyle
    {
        Picture DrawStart(DrawingPerformer performer, PositionHistory positionHistory, bool isShift);

        Picture Drawing(DrawingPerformer performer, PositionHistory positionHistory, bool isShift);

        Picture DrawEnd(DrawingPerformer performer, PositionHistory positionHistory, bool isShift);
    }
}