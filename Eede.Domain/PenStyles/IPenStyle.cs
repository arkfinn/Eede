using Eede.Domain.Positions;

namespace Eede.PenStyles
{
    public interface IPenStyle
    {
        AlphaPicture DrawStart(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift);

        AlphaPicture Drawing(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift);

        AlphaPicture DrawEnd(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift);
    }
}