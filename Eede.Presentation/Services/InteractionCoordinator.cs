using Avalonia.Input;
using Eede.Application.Drawings;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using ReactiveUI;
using System;
using System.Reactive;

namespace Eede.Presentation.Services;

public class InteractionCoordinator : IInteractionCoordinator
{
    public DrawingBuffer CurrentBuffer { get; private set; }
    public PictureArea? SelectingArea { get; private set; }
    public bool IsRegionSelecting { get; private set; }
    public Picture? PreviewPixels { get; private set; }
    public Position PreviewPosition { get; private set; }
    public Cursor ActiveCursor { get; private set; } = Cursor.Default;

    public event Action<Picture, Picture, PictureArea?, PictureArea?> Drew;
    public event Action StateChanged;

    private DrawableArea _drawableArea;
    private CanvasInteractionSession _interactionSession;
    private Magnification _magnification = new(1);
    private PictureArea? _operationInitialSelectingArea;

    public InteractionCoordinator()
    {
        _drawableArea = new(CanvasBackgroundService.Instance, _magnification, new PictureSize(16, 16), null);
    }

    public void UpdateMagnification(Magnification magnification)
    {
        _magnification = magnification;
        _drawableArea = _drawableArea.UpdateMagnification(magnification);
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private void UpdateCursor(Position pos)
    {
        if (_interactionSession == null) return;
        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var selectionCursor = _interactionSession.SelectionState.GetCursor(displayCoordinate.ToCanvas(_magnification).ToPosition());
        ActiveCursor = selectionCursor switch
        {
            SelectionCursor.Move => new Cursor(StandardCursorType.SizeAll),
            _ => Cursor.Default
        };
    }

    private void EnsureInteractionSession(DrawingBuffer buffer, IDrawStyle drawStyle)
    {
        if (_interactionSession == null)
        {
            _interactionSession = new CanvasInteractionSession(buffer, drawStyle);
        }
    }

    public void PointerBegin(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        CurrentBuffer = buffer;
        if (CurrentBuffer == null || CurrentBuffer.IsDrawing()) return;

        EnsureInteractionSession(CurrentBuffer, drawStyle);
        _operationInitialSelectingArea = SelectingArea;
        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession?.SelectionState);
        UpdateCursor(pos);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);

        _interactionSession = _interactionSession.PointerBegin(
            canvasCoordinate,
            penStyle,
            isShift,
            internalUpdateCommand);

        if (_interactionSession.SelectionState is DraggingState)
        {
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
            _drawableArea = _drawableArea.Leave(CurrentBuffer);
            SyncSelectionState(drawStyle);
            NotifyStateChanged();
            return;
        }

        DrawingResult result = _drawableArea.DrawStart(drawStyle, penStyle, CurrentBuffer, pos, isShift);
        CurrentBuffer = result.PictureBuffer;
        _drawableArea = result.DrawableArea;
        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
        SyncSelectionState(drawStyle);
        NotifyStateChanged();
    }

    public void PointerMoved(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize)
    {
        CurrentBuffer = buffer;
        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession == null || CurrentBuffer == null) return;

        UpdateCursor(pos);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = isAnimationMode
            ? HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize)
            : HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        _interactionSession.SelectionState.HandlePointerMoved(
            currentArea,
            true,
            canvasCoordinate.ToPosition(),
            CurrentBuffer.Previous.Size);

        if (_interactionSession.SelectionState is DraggingState)
        {
            _drawableArea = _drawableArea.Leave(CurrentBuffer);
            SyncSelectionState(drawStyle);
            NotifyStateChanged();
            return;
        }

        DrawingResult result = _drawableArea.Move(drawStyle, penStyle, CurrentBuffer, pos, isShift);
        CurrentBuffer = result.PictureBuffer;
        _drawableArea = result.DrawableArea;
        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
        SyncSelectionState(drawStyle);
        NotifyStateChanged();
    }

    public void PointerRightButtonPressed(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, Action<ArgbColor> colorPickedAction, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        CurrentBuffer = buffer;
        if (CurrentBuffer == null) return;
        EnsureInteractionSession(CurrentBuffer, drawStyle);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        (ISelectionState nextState, HalfBoxArea _) = _interactionSession.SelectionState.HandlePointerRightButtonPressed(
            currentArea,
            canvasCoordinate.ToPosition(),
            gridSize,
            internalUpdateCommand);

        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);

        if (CurrentBuffer.IsDrawing())
        {
            DrawingResult result = _drawableArea.DrawCancel(CurrentBuffer);
            CurrentBuffer = result.PictureBuffer;
            _drawableArea = result.DrawableArea;
        }
        else
        {
            ArgbColor newColor = _drawableArea.PickColor(CurrentBuffer.Fetch(), pos);
            colorPickedAction?.Invoke(newColor);
        }
        SyncSelectionState(drawStyle);
        NotifyStateChanged();
    }

    public void PointerLeftButtonReleased(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, PenStyle penStyle, bool isShift, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        CurrentBuffer = buffer;
        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession == null || CurrentBuffer == null) return;

        var previous = CurrentBuffer.Previous;
        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        ISelectionState nextState = _interactionSession.SelectionState.HandlePointerLeftButtonReleased(
            currentArea,
            canvasCoordinate.ToPosition(),
            internalUpdateCommand,
            internalUpdateCommand);

        if (_interactionSession.SelectionState is DraggingState draggingState)
        {
            var info = draggingState.GetSelectionPreviewInfo();
            if (info != null)
            {
                var blendedPicture = CurrentBuffer.Previous.Blend(new DirectImageBlender(), info.Pixels, info.Position);
                CurrentBuffer = CurrentBuffer.Reset(blendedPicture);
            }

            var originalArea = draggingState.GetOriginalArea();
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);
            SyncSelectionState(drawStyle);
            Drew?.Invoke(previous, CurrentBuffer.Previous, originalArea, SelectingArea);
            NotifyStateChanged();
            return;
        }

        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);

        if (CurrentBuffer.IsDrawing())
        {
            Picture previousImage = CurrentBuffer.Previous;
            DrawingResult result = _drawableArea.DrawEnd(drawStyle, penStyle, CurrentBuffer, pos, isShift);
            CurrentBuffer = result.PictureBuffer;
            _drawableArea = result.DrawableArea;
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
            SyncSelectionState(drawStyle);
            Drew?.Invoke(previousImage, CurrentBuffer.Previous, _operationInitialSelectingArea, SelectingArea);
        }
        else
        {
            SyncSelectionState(drawStyle);
        }
        NotifyStateChanged();
    }

    public void CanvasLeave(DrawingBuffer buffer)
    {
        CurrentBuffer = buffer;
        _drawableArea = _drawableArea.Leave(CurrentBuffer);
        NotifyStateChanged();
    }

    public void SetupRegionSelector(RegionSelector tool, DrawingBuffer buffer, bool isAnimationMode, PictureSize gridSize)
    {
        tool.OnDrawStart += (sender, args) =>
        {
            IsRegionSelecting = false;
            var currentArea = HalfBoxArea.Create(args.Start, gridSize);
            _interactionSession = new CanvasInteractionSession(buffer, tool, new NormalCursorState(currentArea));
            NotifyStateChanged();
        };
        tool.OnDrawing += (sender, args) =>
        {
            SelectingArea = PictureArea.FromPosition(args.Start, args.Now, buffer.Previous.Size);
            IsRegionSelecting = true;
            NotifyStateChanged();
        };
        tool.OnDrawEnd += (sender, args) =>
        {
            SelectingArea = PictureArea.FromPosition(args.Start, args.Now, buffer.Previous.Size);
            IsRegionSelecting = true;
            _interactionSession = new CanvasInteractionSession(buffer, tool, new SelectedState(new Selection(SelectingArea.Value)));
            NotifyStateChanged();
        };
    }

    public Picture Painted(DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer)
    {
        var picture = _drawableArea.Painted(buffer, penStyle, imageTransfer);
        if (picture == null) return null;

        if (PreviewPixels != null)
        {
            var magnifiedPreview = imageTransfer.Transfer(PreviewPixels, _magnification);
            var magnifiedPosition = new Position(_magnification.Magnify(PreviewPosition.X), _magnification.Magnify(PreviewPosition.Y));
            picture = picture.Blend(new DirectImageBlender(), magnifiedPreview, magnifiedPosition);
        }
        return picture;
    }

    private void SyncSelectionState(IDrawStyle drawStyle)
    {
        if (_interactionSession == null) return;

        var area = _interactionSession.SelectionState?.GetSelectingArea();
        if (area.HasValue && drawStyle is RegionSelector)
        {
            SelectingArea = area.Value;
            IsRegionSelecting = true;
        }
        else if (drawStyle is not RegionSelector)
        {
            IsRegionSelecting = false;
        }

        var info = _interactionSession.SelectionState?.GetSelectionPreviewInfo();
        if (info != null)
        {
            PreviewPixels = info.Pixels;
            PreviewPosition = info.Position;
        }
        else
        {
            PreviewPixels = null;
        }
    }
}