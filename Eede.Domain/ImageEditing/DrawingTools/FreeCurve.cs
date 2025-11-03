using Eede.Domain.Pictures;

namespace Eede.Domain.ImageEditing.DrawingTools;

public record FreeCurve : IDrawStyle
{

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        return buffer.UpdateDrawing(drawer.DrawPoint(positionHistory.Now));
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Fetch(), penStyle);
        return buffer.UpdateDrawing(Draw(drawer, positionHistory));
    }

    public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        return ContextFactory.Create(buffer.Fetch());
    }

    private Picture Draw(Drawer drawer, PositionHistory positionHistory)
    {
        return drawer.Contains(positionHistory.Now)
            ? drawer.DrawLine(positionHistory.Last, positionHistory.Now)
            : drawer.Contains(positionHistory.Last) ? drawer.DrawLine(positionHistory.Now, positionHistory.Last) : drawer.DrawingPicture;
    }
}

