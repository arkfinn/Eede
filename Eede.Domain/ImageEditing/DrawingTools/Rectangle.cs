using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.DrawingTools;

public record Rectangle : BoxDrawingTool
{
    protected override Picture Draw(Drawer drawer, PositionHistory positionHistory, bool isShift)
    {
        Position to = isShift ? CalculateShiftedPosition(positionHistory.Start, positionHistory.Now) : positionHistory.Now;
        return drawer.DrawRectangle(positionHistory.Start, to);
    }
}