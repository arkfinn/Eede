using Eede.Domain.Positions;

namespace Eede.Domain.PenStyles
{
    public interface IPenStyle
    {
        AlphaPicture DrawStart(DrawingPerformer performer, PositionHistory positionHistory, bool isShift);

        AlphaPicture Drawing(DrawingPerformer performer, PositionHistory positionHistory, bool isShift);

        AlphaPicture DrawEnd(DrawingPerformer performer, PositionHistory positionHistory, bool isShift);
    }
}