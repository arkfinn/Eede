using Eede.Domain.Positions;

namespace Eede.Domain.PenStyles
{
    public interface IPenStyle
    {
        AlphaPicture DrawStart(DrawingMaterial material, PositionHistory positionHistory, bool isShift);

        AlphaPicture Drawing(DrawingMaterial material, PositionHistory positionHistory, bool isShift);

        AlphaPicture DrawEnd(DrawingMaterial material, PositionHistory positionHistory, bool isShift);
    }
}