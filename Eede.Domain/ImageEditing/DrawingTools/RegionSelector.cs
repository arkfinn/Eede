using System;

namespace Eede.Domain.ImageEditing.DrawingTools;

public class RegionSelector : IDrawStyle
{
    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        OnDrawStart?.Invoke(this, coordinateHistory.ToPositionHistory());
        return buffer.UpdateDrawing(buffer.Previous);
    }

    public event EventHandler<PositionHistory> OnDrawStart;
    public event EventHandler<PositionHistory> OnDrawing;
    public event EventHandler<PositionHistory> OnDrawEnd;

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        OnDrawing?.Invoke(this, coordinateHistory.ToPositionHistory());
        return buffer;
    }

    public DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        OnDrawEnd?.Invoke(this, coordinateHistory.ToPositionHistory());
        return new DrawEndResult(buffer.CancelDrawing(), default);
    }
}

