using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Drawings;

public interface IDrawActionUseCase
{
    DrawingSession DrawStart(DrawingSession session, DrawingTool tool, DisplayCoordinate displayCoordinate, Magnification magnification, bool isShift);
    DrawingSession Drawing(DrawingSession session, DrawingTool tool, DisplayCoordinate displayCoordinate, Magnification magnification, bool isShift);
    DrawingSession DrawEnd(DrawingSession session, DrawingTool tool, DisplayCoordinate displayCoordinate, Magnification magnification, bool isShift);
}
