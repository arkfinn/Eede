using Avalonia.Input;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
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
    public DrawingBuffer CurrentBuffer => _sessionProvider.CurrentSession.Buffer;

    private PictureArea? _manualSelectingArea;
    public PictureArea? SelectingArea
    {
        get
        {
            var fromState = _interactionSession?.SelectionState.GetSelectingArea();
            if (fromState != null) return fromState;

            if (_interactionSession?.DrawStyle is RegionSelector)
            {
                return _manualSelectingArea;
            }
            return null;
        }
    }

    public bool IsRegionSelecting => SelectingArea.HasValue && _interactionSession?.DrawStyle is RegionSelector;

    public Picture? PreviewPixels
    {
        get
        {
            var info = _interactionSession?.SelectionState.GetSelectionPreviewInfo();
            if (info != null) return info.Pixels;
            return _sessionProvider.CurrentSession.CurrentPreviewContent?.Pixels;
        }
    }

    public Position PreviewPosition
    {
        get
        {
            var info = _interactionSession?.SelectionState.GetSelectionPreviewInfo();
            if (info != null) return info.Position;
            return _sessionProvider.CurrentSession.CurrentPreviewContent?.Position ?? new Position(0, 0);
        }
    }

    public Cursor ActiveCursor { get; private set; } = Cursor.Default;

    public event Action<Picture, Picture, PictureArea?, PictureArea?> Drew;
    public event Action StateChanged;

    private readonly IDrawingSessionProvider _sessionProvider;
    private DrawableArea _drawableArea;
    private CanvasInteractionSession _interactionSession;
    private Magnification _magnification = new(1);
    private PictureArea? _operationInitialSelectingArea;

    public InteractionCoordinator(IDrawingSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
        _drawableArea = new(_magnification, new PictureSize(16, 16), null);
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
        if (CurrentBuffer == null) return;
        if (CurrentBuffer.IsDrawing() && !(_interactionSession?.SelectionState is SelectionPreviewState)) return;

        EnsureInteractionSession(CurrentBuffer, drawStyle);
        _operationInitialSelectingArea = SelectingArea;
        
        var previousState = _interactionSession.SelectionState;
        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, previousState);
        UpdateCursor(pos);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);

        _interactionSession = _interactionSession.PointerBegin(
            canvasCoordinate,
            penStyle,
            isShift,
            internalUpdateCommand);

        var nextState = _interactionSession.SelectionState;

        // プレビュー状態から通常状態へ遷移（範囲外クリック）した場合、確定する
        if (previousState is SelectionPreviewState && nextState is NormalCursorState)
        {
            _sessionProvider.Update(previousState.Commit(_sessionProvider.CurrentSession));
            // 確定後のバッファでセッションを再構築
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);
            NotifyStateChanged();
            return;
        }

        if (_interactionSession.SelectionState is DraggingState draggingState)
        {
            if (previousState is not SelectionPreviewState)
            {
                var originalArea = draggingState.GetOriginalArea();
                var cutPicture = CurrentBuffer.Previous.Clear(originalArea);
                _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateDrawing(cutPicture));
            }

            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
            _drawableArea = _drawableArea.Leave(CurrentBuffer);
            NotifyStateChanged();
            return;
        }

        DrawingResult result = _drawableArea.DrawStart(drawStyle, penStyle, CurrentBuffer, pos, isShift);
        _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
        _drawableArea = result.DrawableArea;
        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
        NotifyStateChanged();
    }

    public void PointerMoved(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize)
    {
        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession == null || CurrentBuffer == null) return;

        UpdateCursor(pos);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        _interactionSession.SelectionState.HandlePointerMoved(
            currentArea,
            true,
            canvasCoordinate.ToPosition(),
            CurrentBuffer.Previous.Size);

        if (_interactionSession.SelectionState is DraggingState or SelectionPreviewState)
        {
            _drawableArea = _drawableArea.Leave(CurrentBuffer);
            NotifyStateChanged();
            return;
        }

        DrawingResult result = _drawableArea.Move(drawStyle, penStyle, CurrentBuffer, pos, isShift);
        _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
        _drawableArea = result.DrawableArea;
        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
        NotifyStateChanged();
    }

    public void PointerRightButtonPressed(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, Action<ArgbColor> colorPickedAction, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        if (CurrentBuffer == null) return;
        EnsureInteractionSession(CurrentBuffer, drawStyle);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        var previousState = _interactionSession.SelectionState;
        (ISelectionState nextState, HalfBoxArea _) = previousState.HandlePointerRightButtonPressed(
            currentArea,
            canvasCoordinate.ToPosition(),
            gridSize,
            internalUpdateCommand);

        if (previousState is SelectionPreviewState && nextState is NormalCursorState)
        {
            _sessionProvider.Update(previousState.Cancel(_sessionProvider.CurrentSession));
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);
            NotifyStateChanged();
            return;
        }
        else
        {
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);
        }

        if (CurrentBuffer.IsDrawing())
        {
            DrawingResult result = _drawableArea.DrawCancel(CurrentBuffer);
            _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
            _drawableArea = result.DrawableArea;
        }
        else if (previousState is not SelectionPreviewState)
        {
            ArgbColor newColor = _drawableArea.PickColor(CurrentBuffer.Fetch(), pos);
            colorPickedAction?.Invoke(newColor);
        }
        NotifyStateChanged();
    }

    public void PointerLeftButtonReleased(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, PenStyle penStyle, bool isShift, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession == null || CurrentBuffer == null) return;

        var previousImage = CurrentBuffer.Previous;
        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        ISelectionState nextState = _interactionSession.SelectionState.HandlePointerLeftButtonReleased(
            currentArea,
            canvasCoordinate.ToPosition(),
            internalUpdateCommand,
            internalUpdateCommand);

        if (_interactionSession.SelectionState is DraggingState)
        {
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);
            NotifyStateChanged();
            return;
        }

        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);

        if (CurrentBuffer.IsDrawing())
        {
            DrawingResult result = _drawableArea.DrawEnd(drawStyle, penStyle, CurrentBuffer, pos, isShift);
            _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
            _drawableArea = result.DrawableArea;
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);
            Drew?.Invoke(previousImage, CurrentBuffer.Previous, _operationInitialSelectingArea, SelectingArea);
        }
        NotifyStateChanged();
    }

    public void CanvasLeave(DrawingBuffer buffer)
    {
        _drawableArea = _drawableArea.Leave(CurrentBuffer);
        NotifyStateChanged();
    }

    public void CommitSelection()
    {
        if (_interactionSession?.SelectionState != null)
        {
            var nextSession = _interactionSession.SelectionState.Commit(_sessionProvider.CurrentSession);
            if (nextSession != _sessionProvider.CurrentSession)
            {
                _sessionProvider.Update(nextSession);
                _interactionSession = new CanvasInteractionSession(CurrentBuffer, _interactionSession.DrawStyle, new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16))));
                NotifyStateChanged();
            }
        }
    }

    public void SyncWithSession()
    {
        var session = _sessionProvider.CurrentSession;
        if (session.CurrentPreviewContent != null)
        {
            // プレビュー情報がある場合、対応する状態に移行
            if (!(_interactionSession?.SelectionState is SelectionPreviewState))
            {
                _interactionSession = new CanvasInteractionSession(CurrentBuffer, _interactionSession?.DrawStyle ?? new RegionSelector(), new SelectionPreviewState(session.CurrentPreviewContent));
                NotifyStateChanged();
            }
        }
        else if (_interactionSession?.SelectionState is SelectionPreviewState)
        {
            // プレビューが消えた場合、通常状態に戻す
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, _interactionSession?.DrawStyle ?? new RegionSelector(), new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16))));
            NotifyStateChanged();
        }
    }

    public void SetupRegionSelector(RegionSelector tool, DrawingBuffer buffer, bool isAnimationMode, PictureSize gridSize)
    {
        tool.OnDrawStart += (sender, args) =>
        {
            _manualSelectingArea = null;
            var currentArea = HalfBoxArea.Create(args.Start, gridSize);
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, tool, new NormalCursorState(currentArea));
            NotifyStateChanged();
        };
        tool.OnDrawing += (sender, args) =>
        {
            _manualSelectingArea = PictureArea.FromPosition(args.Start, args.Now, CurrentBuffer.Previous.Size);
            NotifyStateChanged();
        };
        tool.OnDrawEnd += (sender, args) =>
        {
            _manualSelectingArea = null;
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, tool, new SelectedState(new Selection(PictureArea.FromPosition(args.Start, args.Now, CurrentBuffer.Previous.Size))));
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
}
