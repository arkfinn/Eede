using Eede.Domain.SharedKernel;
using System;

#nullable enable
namespace Eede.Domain.ImageEditing.DrawingTools;

// 実装内容を再検討する
public record Line : IDrawStyle
{
    private PictureArea? AffectedArea;

    public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = drawer.DrawPoint(coordinateHistory.Now.ToPosition());
        AffectedArea = result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = Draw(drawer, coordinateHistory, isShift);
        AffectedArea = result.Area;
        return buffer.UpdateDrawing(result.Picture);
    }

    public DrawEndResult DrawEnd(DrawingBuffer buffer, PenStyle penStyle, CoordinateHistory coordinateHistory, bool isShift)
    {
        Drawer drawer = new(buffer.Previous, penStyle);
        var result = Draw(drawer, coordinateHistory, isShift);
        var area = AffectedArea.HasValue ? AffectedArea.Value.Combine(result.Area) : result.Area;
        AffectedArea = null;
        return new DrawEndResult(ContextFactory.Create(result.Picture), area);
    }

    private (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory, bool isShift)
    {
        Position to = isShift ? CalculateShiftedPosition(coordinateHistory.Start.ToPosition(), coordinateHistory.Now.ToPosition()) : coordinateHistory.Now.ToPosition();
        return drawer.DrawLine(coordinateHistory.Start.ToPosition(), to);
    }

    private Position CalculateShiftedPosition(Position beginPos, Position endPos)
    {
        Position margin = new(endPos.X - beginPos.X, endPos.Y - beginPos.Y);
        int deg = (int)Math.Round(((Math.Atan2(margin.Y, margin.X) * 57.29578) + 180) / 22.5);
        Position revise = deg switch
        {
            0 or 15 or 16 => new Position(-1, 0),
            1 or 2 => new Position(-1, -1),
            3 or 4 => new Position(0, -1),
            5 or 6 => new Position(1, -1),
            7 or 8 => new Position(1, 0),
            9 or 10 => new Position(1, 1),
            11 or 12 => new Position(0, 1),
            13 or 14 => new Position(-1, 1),
            _ => throw new InvalidOperationException("不正な計算によるエラー"),
        };

        // 角度別に処理(強引)
        int plusValue = Math.Max(Math.Abs(margin.X), Math.Abs(margin.Y));
        return new Position(
            beginPos.X + (plusValue * revise.X),
            beginPos.Y + (plusValue * revise.Y)
        );
    }
}
