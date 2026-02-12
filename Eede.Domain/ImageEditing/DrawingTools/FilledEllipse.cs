using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.DrawingTools;

public record FilledEllipse : BoxDrawingTool
{
    protected override (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory, bool isShift)
    {
        Position to = isShift ? CalculateShiftedPosition(coordinateHistory.Start.ToPosition(), coordinateHistory.Now.ToPosition()) : coordinateHistory.Now.ToPosition();
        return drawer.DrawFillEllipse(coordinateHistory.Start.ToPosition(), to);
    }
}
