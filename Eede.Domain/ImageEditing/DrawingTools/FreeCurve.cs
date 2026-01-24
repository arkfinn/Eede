namespace Eede.Domain.ImageEditing.DrawingTools;

public record FreeCurve : IDrawStyle
{

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        return buffer.UpdateDrawing(drawer.DrawPoint(coordinateHistory.Now.ToPosition()));
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Fetch(), penStyle);
        return buffer.UpdateDrawing(Draw(drawer, coordinateHistory));
    }

    public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        return ContextFactory.Create(buffer.Fetch());
    }

    private Picture Draw(Drawer drawer, CoordinateHistory coordinateHistory)
    {
        return drawer.Contains(coordinateHistory.Now.ToPosition())
            ? drawer.DrawLine(coordinateHistory.Last.ToPosition(), coordinateHistory.Now.ToPosition())
            : drawer.Contains(coordinateHistory.Last.ToPosition()) ? drawer.DrawLine(coordinateHistory.Now.ToPosition(), coordinateHistory.Last.ToPosition()) : drawer.DrawingPicture;
    }
}

