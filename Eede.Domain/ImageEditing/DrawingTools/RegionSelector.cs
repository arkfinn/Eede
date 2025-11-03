using System;

namespace Eede.Domain.ImageEditing.DrawingTools;

public class RegionSelector : IDrawStyle
{
    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        OnDrawStart?.Invoke(this, positionHistory);
        return buffer.UpdateDrawing(buffer.Previous);
    }

    public event EventHandler<PositionHistory> OnDrawStart;
    public event EventHandler<PositionHistory> OnDrawing;
    public event EventHandler<PositionHistory> OnDrawEnd;

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        OnDrawing?.Invoke(this, positionHistory);
        return buffer;
    }

    public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        OnDrawEnd?.Invoke(this, positionHistory);
        return buffer.CancelDrawing();
    }
}

