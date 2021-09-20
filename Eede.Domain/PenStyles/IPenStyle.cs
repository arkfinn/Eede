using Eede.Positions;
using System;
using System.Drawing;

namespace Eede.PenStyles
{
    public interface IPenStyle
    {
        void DrawBegin(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift);
        void DrawEnd(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift);
        void Drawing(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift);
    }
}
