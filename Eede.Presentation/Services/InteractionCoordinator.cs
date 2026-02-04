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
    public DrawingBuffer CurrentBuffer => _interactionSession?.Buffer ?? _sessionProvider.CurrentSession?.Buffer;

    private PictureArea? _manualSelectingArea;
    public PictureArea? SelectingArea
    {
        get
        {
            if (_interactionSession == null)
            {
                return _sessionProvider.CurrentSession?.CurrentSelectingArea;
            }
            var fromState = _interactionSession.SelectionState?.GetSelectingArea();
            if (fromState != null) return fromState;

            if (_interactionSession.DrawStyle is RegionSelector)
            {
                return _manualSelectingArea;
            }
            return null;
        }
    }

    public bool IsRegionSelecting => SelectingArea.HasValue && !SelectingArea.Value.IsEmpty && (_interactionSession?.DrawStyle is RegionSelector || _interactionSession == null) && (_interactionSession?.SelectionState is not NormalCursorState || _manualSelectingArea != null);

    public bool IsShowHandles => _interactionSession?.SelectionState is SelectedState or ResizingState or SelectionPreviewState;

    public Picture? PreviewPixels
    {
        get
        {
            var info = _interactionSession?.SelectionState?.GetSelectionPreviewInfo();
            if (info != null) return info.Pixels;
            return _sessionProvider.CurrentSession?.CurrentPreviewContent?.Pixels;
        }
    }

    public Position PreviewPosition
    {
        get
        {
            var info = _interactionSession?.SelectionState?.GetSelectionPreviewInfo();
            if (info != null) return info.Position;
            return _sessionProvider.CurrentSession?.CurrentPreviewContent?.Position ?? new Position(0, 0);
        }
    }

    public Cursor ActiveCursor { get; private set; } = Cursor.Default;
    private IImageBlender _imageBlender = new DirectImageBlender();
    public IImageBlender ImageBlender
    {
        get => _imageBlender;
        set
        {
            _imageBlender = value;
            NotifyStateChanged();
        }
    }
    private ArgbColor _backgroundColor = new ArgbColor(0, 0, 0, 0);
    public ArgbColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            NotifyStateChanged();
        }
    }

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
        if (_interactionSession?.SelectionState == null) return;
        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var selectionCursor = _interactionSession.SelectionState.GetCursor(displayCoordinate.ToCanvas(_magnification).ToPosition());
                                ActiveCursor = selectionCursor switch
                                {
                                    SelectionCursor.Move => new Cursor(StandardCursorType.SizeAll),
                                    SelectionCursor.SizeNWSE => new Cursor(StandardCursorType.TopLeftCorner),
                                    SelectionCursor.SizeNESW => new Cursor(StandardCursorType.TopRightCorner),
                                    SelectionCursor.SizeNS => new Cursor(StandardCursorType.TopSide),
                                    SelectionCursor.SizeWE => new Cursor(StandardCursorType.LeftSide),
                                    _ => Cursor.Default
                                };    }

    private void EnsureInteractionSession(DrawingBuffer buffer, IDrawStyle drawStyle)
    {
        if (buffer == null) return;
        if (_interactionSession == null)
        {
            _interactionSession = new CanvasInteractionSession(buffer, drawStyle);
        }
    }

    private bool IsOperating => CurrentBuffer != null && (CurrentBuffer.IsDrawing() || _interactionSession?.SelectionState is DraggingState or SelectionPreviewState or SelectedState or RegionSelectingState or AnimationEditingState);

    public void PointerBegin(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        if (CurrentBuffer == null) return;
        if (CurrentBuffer.IsDrawing() && !(_interactionSession?.SelectionState is SelectionPreviewState)) return;

        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession == null) return;
        _operationInitialSelectingArea = SelectingArea;
        
        var workingSession = _sessionProvider.CurrentSession;
        if (workingSession == null) return;

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        var previousState = _interactionSession.SelectionState;

        // 1. プレビュー状態中に別のツールで描画を開始しようとした場合、確定する
        if (previousState is SelectionPreviewState && drawStyle is not RegionSelector)
        {
            workingSession = previousState.Commit(workingSession, ImageBlender, BackgroundColor);
            workingSession = workingSession.UpdateSelectingArea(null);
            _sessionProvider.Update(workingSession);
            // 確定後の最新バッファと、確定後の状態(NormalCursorState)でセッションを更新
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, new NormalCursorState(currentArea));
            previousState = _interactionSession.SelectionState;
        }
        else
        {
            // 確定が発生しなかった場合でも、最新のバッファでセッションを確実に同期させる
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, previousState);
        }

        UpdateCursor(pos);

        // 2. 選択状態の更新（移動開始判定など）
        var currentState = _interactionSession.SelectionState;
        var nextState = currentState.HandlePointerLeftButtonPressed(
            currentArea,
            canvasCoordinate.ToPosition(),
            null,
            () => workingSession.Buffer.Fetch(),
            internalUpdateCommand);

        if (nextState is DraggingState)
        {
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, nextState);
            _drawableArea = _drawableArea.Leave(workingSession.Buffer);
            NotifyStateChanged();
            return;
        }
        else
        {
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, nextState);
        }

        // 3. プレビュー状態から通常状態へ遷移（範囲外クリック）した場合、確定する
        if (previousState is SelectionPreviewState && nextState is NormalCursorState)
        {
            workingSession = previousState.Commit(workingSession, ImageBlender, BackgroundColor);
            _sessionProvider.Update(workingSession);
            // 確定後のバッファでセッションを再構築
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, nextState);
            NotifyStateChanged();
        }

        // 4. 新しい描画の開始
        DrawingResult result = _drawableArea.DrawStart(drawStyle, penStyle, workingSession.Buffer, pos, isShift);
        if (result.PictureBuffer != workingSession.Buffer)
        {
            workingSession = workingSession.UpdateBuffer(result.PictureBuffer);
            _sessionProvider.Update(workingSession);
        }
        _drawableArea = result.DrawableArea;
        // _interactionSessionがUpdateによってnullにされている可能性があるため、安全に再代入
        _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession?.SelectionState ?? nextState);
        NotifyStateChanged();
    }

    public void PointerMoved(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize)
    {
        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession?.SelectionState == null || CurrentBuffer == null) return;

        UpdateCursor(pos);

        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        _interactionSession.SelectionState.HandlePointerMoved(
            currentArea,
            true,
            canvasCoordinate.ToPosition(),
            isShift,
            CurrentBuffer.Previous.Size);

        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, _interactionSession.SelectionState);

        if (_interactionSession.SelectionState is DraggingState or SelectionPreviewState)
        {
            _drawableArea = _drawableArea.Leave(CurrentBuffer);
            NotifyStateChanged();
            return;
        }

        DrawingResult result = _drawableArea.Move(drawStyle, penStyle, CurrentBuffer, pos, isShift);
        if (_sessionProvider.CurrentSession != null)
        {
            _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
        }
        _drawableArea = result.DrawableArea;
        _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession?.SelectionState);
        NotifyStateChanged();
    }

    public void PointerRightButtonPressed(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, Action<ArgbColor> colorPickedAction, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        if (CurrentBuffer == null) return;
        EnsureInteractionSession(CurrentBuffer, drawStyle);
        if (_interactionSession?.SelectionState == null) return;

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
            if (_sessionProvider.CurrentSession != null)
            {
                _sessionProvider.Update(previousState.Cancel(_sessionProvider.CurrentSession));
            }
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
            if (_sessionProvider.CurrentSession != null)
            {
                _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
            }
            _drawableArea = result.DrawableArea;
            _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession?.SelectionState ?? nextState);
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
        if (_interactionSession?.SelectionState == null || CurrentBuffer == null) return;

        var previousImage = CurrentBuffer.Previous;
        var displayCoordinate = new DisplayCoordinate(pos.X, pos.Y);
        var canvasCoordinate = displayCoordinate.ToCanvas(_magnification);
        var currentArea = HalfBoxArea.Create(canvasCoordinate.ToPosition(), gridSize);

        ISelectionState nextState = _interactionSession.SelectionState.HandlePointerLeftButtonReleased(
            currentArea,
            canvasCoordinate.ToPosition(),
            internalUpdateCommand,
            internalUpdateCommand);

        if (_interactionSession.SelectionState is DraggingState or ResizingState)
        {
            var info = nextState.GetSelectionPreviewInfo();
            if (info != null && _sessionProvider.CurrentSession != null)
            {
                _sessionProvider.Update(_sessionProvider.CurrentSession.UpdatePreviewContent(info));
            }
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);
            NotifyStateChanged();
            return;
        }

        _interactionSession = new CanvasInteractionSession(CurrentBuffer, drawStyle, nextState);

        if (CurrentBuffer.IsDrawing())
        {
            DrawingResult result = _drawableArea.DrawEnd(drawStyle, penStyle, CurrentBuffer, pos, isShift);
            if (_sessionProvider.CurrentSession != null)
            {
                _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
            }
            _drawableArea = result.DrawableArea;
            _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession?.SelectionState ?? nextState);
            Drew?.Invoke(previousImage, CurrentBuffer.Previous, _operationInitialSelectingArea, SelectingArea);
        }
        NotifyStateChanged();
    }

    public void CanvasLeave(DrawingBuffer buffer)
    {
        if (CurrentBuffer == null) return;
        _drawableArea = _drawableArea.Leave(CurrentBuffer);
        NotifyStateChanged();
    }

    public void CommitSelection(bool forceClearSelection = false)
    {
        _manualSelectingArea = null;
        if (_interactionSession?.SelectionState != null && _sessionProvider.CurrentSession != null)
        {
            var nextSession = _interactionSession.SelectionState.Commit(_sessionProvider.CurrentSession, ImageBlender, BackgroundColor);
            if (forceClearSelection || _interactionSession.DrawStyle is not RegionSelector)
            {
                nextSession = nextSession.UpdateSelectingArea(null);
            }

            if (nextSession != _sessionProvider.CurrentSession)
            {
                _sessionProvider.Update(nextSession);
                
                var area = nextSession.CurrentSelectingArea;
                if (area.HasValue && !area.Value.IsEmpty && _interactionSession.DrawStyle is RegionSelector)
                {
                    _interactionSession = new CanvasInteractionSession(nextSession.Buffer, _interactionSession.DrawStyle, new SelectedState(new Selection(area.Value)));
                }
                else
                {
                    _interactionSession = new CanvasInteractionSession(nextSession.Buffer, _interactionSession.DrawStyle, new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16))));
                }
                NotifyStateChanged();
            }
        }
    }

    public void ChangeDrawStyle(IDrawStyle drawStyle)
    {
        if (_interactionSession != null)
        {
            _interactionSession = new CanvasInteractionSession(_interactionSession.Buffer, drawStyle, _interactionSession.SelectionState);
            NotifyStateChanged();
        }
    }

    public void SyncWithSession()
    {
        SyncWithSession(false);
    }

    public void SyncWithSession(bool forceReset)
    {
        var session = _sessionProvider.CurrentSession;
        if (session == null) return;

        // セッションにプレビューがあるのに、ステートがプレビュー（またはドラッグ）でない場合は、
        // 外部（ペースト等）で状態が変わったとみなして同期を優先する。
        bool isStateMismatch = session.CurrentPreviewContent != null &&
                               !(_interactionSession?.SelectionState is SelectionPreviewState or DraggingState);

        // 操作中（描画、ドラッグ、プレビュー）の場合は絶対にリセットしない（ミスマッチ時を除く）
        if (IsOperating && !forceReset && !isStateMismatch)
        {
            return;
        }

        // バッファが変わった場合（PushやUndoなど外部要因）はリセット
        if (forceReset || (_interactionSession != null && _interactionSession.Buffer != session.Buffer))
        {
            _interactionSession = null;
        }

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

    public void UpdatePicture(Picture picture)
    {
        if (_sessionProvider.CurrentSession != null)
        {
            _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(new DrawingBuffer(picture)));
            SyncWithSession(true); // 明示的な更新時はリセット
        }
    }

    public void SetupRegionSelector(RegionSelector tool, DrawingBuffer buffer, bool isAnimationMode, PictureSize gridSize)
    {
        tool.OnDrawStart += (sender, args) =>
        {
            if (CurrentBuffer == null) return;
            _manualSelectingArea = null;
            var currentArea = HalfBoxArea.Create(args.Start, gridSize);
            _interactionSession = new CanvasInteractionSession(CurrentBuffer, tool, new NormalCursorState(currentArea));
            NotifyStateChanged();
        };
        tool.OnDrawing += (sender, args) =>
        {
            if (CurrentBuffer == null) return;
            _manualSelectingArea = PictureArea.FromPosition(args.Start, args.Now, CurrentBuffer.Previous.Size);
            NotifyStateChanged();
        };
        tool.OnDrawEnd += (sender, args) =>
        {
            if (CurrentBuffer == null) return;
            _manualSelectingArea = null;
            var area = PictureArea.FromPosition(args.Start, args.Now, CurrentBuffer.Previous.Size);
            if (area.IsEmpty)
            {
                _interactionSession = new CanvasInteractionSession(CurrentBuffer, tool, new NormalCursorState(HalfBoxArea.Create(args.Now, gridSize)));
                if (_sessionProvider.CurrentSession != null)
                {
                    _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateSelectingArea(null));
                }
            }
            else
            {
                var selection = new Selection(area);
                _interactionSession = new CanvasInteractionSession(CurrentBuffer, tool, new SelectedState(selection));
                if (_sessionProvider.CurrentSession != null)
                {
                    _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateSelectingArea(selection.Area));
                }
            }

            NotifyStateChanged();
        };
    }

    public Picture Painted(DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer)
    {
        if (buffer == null) return null;

        // 1. 描画中の内容を含むベース画像（未拡大）を取得
        Picture picture = buffer.Fetch();
        
        // 2. プレビュー画像（貼り付けや移動）がある場合、拡大前に合成する
        // 現在の作業状態 (_interactionSession) のプレビュー情報を最優先し、
        // なければグローバルセッション (IDrawingSessionProvider) の情報を使う。
        var preview = _interactionSession?.SelectionState?.GetSelectionPreviewInfo() 
                      ?? _sessionProvider.CurrentSession?.CurrentPreviewContent;

                if (preview != null)
                {
                    var blender = ImageBlender;
                    var bgColor = BackgroundColor;
        
                    if (preview.OriginalArea.HasValue)
                    {
                        picture = picture.Clear(preview.OriginalArea.Value);
                    }
                    
                    var pixels = preview.Pixels;
                    if (blender is AlphaImageBlender)
                    {
                        pixels = pixels.ApplyTransparency(bgColor);
                    }
                    picture = picture.Blend(blender, pixels, preview.Position);
                }
        // 3. 合成済みの画像を拡大し、必要に応じてペン先カーソルを重ねる
        var tempBuffer = buffer.Reset(picture);
        if (buffer.IsDrawing())
        {
            tempBuffer = tempBuffer.UpdateDrawing(picture);
        }
        
        return _drawableArea.Painted(tempBuffer, penStyle, imageTransfer);
    }
}