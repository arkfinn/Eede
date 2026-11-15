#nullable enable
namespace Eede.Domain.ImageEditing.DrawingTools;

public interface IDrawStyle
{
    bool AllowOutOfBoundsStart => false;

    DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift);

    DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift);

    DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift);
}

