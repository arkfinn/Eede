using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

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
        ICommand? updateAction,
        Eede.Domain.ImageEditing.Blending.IImageBlender blender,
        Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        // 1. SelectionState の処理
        var currentArea = HalfBoxArea.Create(coordinate.ToPosition(), new PictureSize(16, 16));
        var nextState = SelectionState.HandlePointerLeftButtonPressed(
            currentArea,
            coordinate.ToPosition(),
            null,
            () => Buffer.Previous,
            updateAction,
            blender,
            backgroundColor);

        if (nextState is DraggingState)
        {
            return new CanvasInteractionSession(Buffer, DrawStyle, nextState);
        }

        // 2. 描画の開始
        if (Buffer.IsDrawing() || !Buffer.Fetch().Contains(coordinate.ToPosition()))
        {
            return new CanvasInteractionSession(Buffer, DrawStyle, nextState);
        }

        var history = new CoordinateHistory(coordinate);
        var nextBuffer = DrawStyle.DrawStart(Buffer, penStyle, history, isShifted);

        return new CanvasInteractionSession(nextBuffer, DrawStyle, nextState);
    }
}
