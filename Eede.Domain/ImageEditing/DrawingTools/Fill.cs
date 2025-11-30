namespace Eede.Domain.ImageEditing.DrawingTools;


// 実装内容を再検討する
public record Fill : IDrawStyle
{
    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        return buffer.UpdateDrawing(drawer.Fill(positionHistory.Now));
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        return buffer;
    }

    public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
    {
        return ContextFactory.Create(buffer.Fetch());
    }

}

