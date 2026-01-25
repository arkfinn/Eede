using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.DrawingTools;

public record Rectangle : BoxDrawingTool
{
    protected override Picture Draw(Drawer drawer, CoordinateHistory coordinateHistory, bool isShift)
    {
        Position to = isShift ? CalculateShiftedPosition(coordinateHistory.Start.ToPosition(), coordinateHistory.Now.ToPosition()) : coordinateHistory.Now.ToPosition();
        return drawer.DrawRectangle(coordinateHistory.Start.ToPosition(), to);
    }
}