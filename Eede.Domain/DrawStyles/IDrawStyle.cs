using Eede.Domain.Pictures;
using Eede.Domain.Positions;

namespace Eede.Domain.DrawStyles
{
    public interface IDrawStyle
    {
        Picture DrawStart(Drawer performer, PositionHistory positionHistory, bool isShift);

        Picture Drawing(Drawer performer, PositionHistory positionHistory, bool isShift);

        Picture DrawEnd(Drawer performer, PositionHistory positionHistory, bool isShift);
    }
}