using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.DrawingTools;

public record FreeCurve : IDrawStyle
{
    private PictureArea? AffectedArea;

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = drawer.DrawPoint(coordinateHistory.Now.ToPosition());
        AffectedArea = result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Fetch(), penStyle);
        var result = Draw(drawer, coordinateHistory);
        AffectedArea = AffectedArea.HasValue ? AffectedArea.Value.Combine(result.Area) : result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        var area = AffectedArea;
        AffectedArea = null;
        return new DrawEndResult(ContextFactory.Create(buffer.Fetch()), area);
    }

    private (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory)
    {
        return drawer.Contains(coordinateHistory.Now.ToPosition())
            ? drawer.DrawLine(coordinateHistory.Last.ToPosition(), coordinateHistory.Now.ToPosition())
            : drawer.Contains(coordinateHistory.Last.ToPosition()) ? drawer.DrawLine(coordinateHistory.Now.ToPosition(), coordinateHistory.Last.ToPosition()) : (drawer.DrawingPicture, new PictureArea(coordinateHistory.Now.ToPosition(), new PictureSize(0, 0)));
    }
}

