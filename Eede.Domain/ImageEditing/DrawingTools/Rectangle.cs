using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.DrawingTools;

public record Rectangle : IDrawStyle
{
    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        return buffer.UpdateDrawing(Draw(drawer, positionHistory, isShift));
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        return buffer.UpdateDrawing(Draw(drawer, positionHistory, isShift));
    }

    public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        return ContextFactory.Create(Draw(drawer, positionHistory, isShift));
    }

    private Picture Draw(Drawer drawer, PositionHistory positionHistory, bool isShift)
    {
        Position to = isShift ? CalculateShiftedPosition(positionHistory.Start, positionHistory.Now) : positionHistory.Now;
        return drawer.DrawRectangle(positionHistory.Start, to);
    }

    private Position CalculateShiftedPosition(Position start, Position end)
    {
        int width = Math.Abs(end.X - start.X);
        int height = Math.Abs(end.Y - start.Y);
        int size = Math.Max(width, height);

        int newX = start.X + (end.X >= start.X ? size : -size);
        int newY = start.Y + (end.Y >= start.Y ? size : -size);

        return new Position(newX, newY);
    }
}
