using System.Collections.Generic;
using System.Linq;
using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.DrawingTools;

public record FreeCurve : IDrawStyle
{
    private readonly List<PictureArea> AffectedAreas = new();

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        AffectedAreas.Clear();
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = drawer.DrawPoint(coordinateHistory.Now.ToPosition());
        AffectedAreas.Add(result.Area);
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Fetch(), penStyle);
        var result = Draw(drawer, coordinateHistory);
        AffectedAreas.Add(result.Area);
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        var result = new DrawEndResult(ContextFactory.Create(buffer.Fetch()), new PictureRegion(AffectedAreas.ToList()));
        AffectedAreas.Clear();
        return result;
    }

    private (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory)
    {
        return drawer.Contains(coordinateHistory.Now.ToPosition())
            ? drawer.DrawLine(coordinateHistory.Last.ToPosition(), coordinateHistory.Now.ToPosition())
            : drawer.Contains(coordinateHistory.Last.ToPosition()) ? drawer.DrawLine(coordinateHistory.Now.ToPosition(), coordinateHistory.Last.ToPosition()) : (drawer.DrawingPicture, new PictureArea(coordinateHistory.Now.ToPosition(), new PictureSize(0, 0)));
    }
}

