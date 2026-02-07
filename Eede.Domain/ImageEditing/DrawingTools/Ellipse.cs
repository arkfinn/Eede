using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.DrawingTools;

public record Ellipse : BoxDrawingTool
{
    protected override (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory, bool isShift)
    {
        Position to = isShift ? CalculateShiftedPosition(coordinateHistory.Start.ToPosition(), coordinateHistory.Now.ToPosition()) : coordinateHistory.Now.ToPosition();
        return drawer.DrawEllipse(coordinateHistory.Start.ToPosition(), to);
    }
}
