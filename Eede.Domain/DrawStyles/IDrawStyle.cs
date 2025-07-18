﻿using Eede.Domain.Drawing;
using Eede.Domain.Positions;

namespace Eede.Domain.DrawStyles
{
    public interface IDrawStyle
    {
        DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift);

        DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift);

        DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift);
    }
}
