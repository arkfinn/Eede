using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.DrawingTools;


// 実装内容を再検討する
public record Fill : IDrawStyle
{
    private PictureArea? AffectedArea;

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = drawer.Fill(coordinateHistory.Now.ToPosition());
        AffectedArea = result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        return buffer;
    }

    public DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        var area = AffectedArea;
        AffectedArea = null;
        return new DrawEndResult(ContextFactory.Create(buffer.Fetch()), area);
    }

}

