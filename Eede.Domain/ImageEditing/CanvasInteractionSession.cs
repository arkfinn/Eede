using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing;

public class CanvasInteractionSession
{
    public DrawingBuffer Buffer { get; }
    public ISelectionState SelectionState { get; }
    public IDrawStyle DrawStyle { get; }

    public CanvasInteractionSession(DrawingBuffer buffer, IDrawStyle drawStyle)
        : this(buffer, drawStyle, new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16))))
    {
    }

    public CanvasInteractionSession(DrawingBuffer buffer, IDrawStyle drawStyle, ISelectionState selectionState)
    {
        Buffer = buffer;
        DrawStyle = drawStyle;
        SelectionState = selectionState;
    }

    public CanvasInteractionSession PointerBegin(
        CanvasCoordinate coordinate,
        PenStyle penStyle,
        bool isShifted,
        Action<Picture>? updateAction)
    {
        // 1. SelectionState の処理
        var currentArea = HalfBoxArea.Create(coordinate.ToPosition(), new PictureSize(16, 16));
        var nextState = SelectionState.HandlePointerLeftButtonPressed(
            currentArea,
            coordinate.ToPosition(),
            null,
            () => Buffer.Previous,
            updateAction);

        if (nextState is DraggingState)
        {
            return new CanvasInteractionSession(Buffer, DrawStyle, nextState);
        }

        // 2. 描画の開始
        // TODO: DrawableArea の責務をここへ集約する
        // 現時点では、描画開始が可能かどうかの不変条件チェックのみ行う
        if (Buffer.IsDrawing() || !Buffer.Fetch().Contains(coordinate.ToPosition()))
        {
            return new CanvasInteractionSession(Buffer, DrawStyle, nextState);
        }

        // 描画開始（CoordinateHistory の生成）
        var history = new CoordinateHistory(coordinate);
        var nextBuffer = DrawStyle.DrawStart(Buffer, penStyle, history, isShifted);

        return new CanvasInteractionSession(nextBuffer, DrawStyle, nextState);
    }
}