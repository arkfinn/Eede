using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.DrawingTools;

public abstract record BoxDrawingTool : IDrawStyle
{
    private PictureArea? AffectedArea;

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = Draw(drawer, coordinateHistory, isShift);
        AffectedArea = result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = Draw(drawer, coordinateHistory, isShift);
        AffectedArea = result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = Draw(drawer, coordinateHistory, isShift);
        var area = AffectedArea.HasValue ? AffectedArea.Value.Combine(result.Area) : result.Area;
        AffectedArea = null;
        return new DrawEndResult(ContextFactory.Create(result.Picture), area);
    }

    protected abstract (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory, bool isShift);

    protected Position CalculateShiftedPosition(Position start, Position end)
    {
        int width = Math.Abs(end.X - start.X);
        int height = Math.Abs(end.Y - start.Y);
        int size = Math.Max(width, height);

        int newX = start.X + (end.X >= start.X ? size : -size);
        int newY = start.Y + (end.Y >= start.Y ? size : -size);

        return new Position(newX, newY);
    }
}
