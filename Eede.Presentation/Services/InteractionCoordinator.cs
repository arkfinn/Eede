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

#nullable enable

public class InteractionCoordinator : IInteractionCoordinator
{
    public DrawingBuffer? CurrentBuffer => _interactionSession?.Buffer ?? _sessionProvider.CurrentSession?.Buffer;

    private PictureArea? _manualSelectingArea;
    public PictureArea? SelectingArea
    {
        get
        {
            var fromState = _interactionSession?.SelectionState?.GetSelectingArea();
            if (fromState != null) return fromState;

            if (_interactionSession?.DrawStyle is RegionSelector && _manualSelectingArea != null)
            {
                return _manualSelectingArea;
            }

            return _sessionProvider.CurrentSession?.CurrentSelectingArea;
        }
    }

    public bool IsRegionSelecting => SelectingArea.HasValue && !SelectingArea.Value.IsEmpty && (IsShowHandles || _interactionSession?.DrawStyle is RegionSelector);

    public bool IsShowHandles => _interactionSession?.SelectionState is SelectedState or ResizingState or SelectionPreviewState or DraggingState;

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

    public StandardCursorType ActiveCursor { get; private set; } = StandardCursorType.Arrow;
    public SelectionCursor ActiveSelectionCursor { get; private set; } = SelectionCursor.Default;
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

    public event Action<Picture, Picture, PictureArea?, PictureArea?, PictureRegion>? Drew;
    public event Action? StateChanged;

    private readonly IDrawingSessionProvider _sessionProvider;
    private DrawableArea _drawableArea;
    private CanvasInteractionSession? _interactionSession;
    private Magnification _magnification = new(1);
    private PictureArea? _operationInitialSelectingArea;
    private Position _lastMousePosition = new(0, 0);

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

    private void UpdateCursor(Position canvasPos)
    {
        if (_interactionSession?.SelectionState == null) return;

        // 画面上で 8ピクセルの判定サイズを、現在の倍率に合わせてキャンバス座標系に変換する
        // 少なくとも 1ピクセル（キャンバス上）は確保する
        int handleSize = Math.Max(1, (int)(8 / _magnification.Value));

        var selectionCursor = _interactionSession.SelectionState.GetCursor(canvasPos, handleSize);
        bool isLogicalCursorChanged = ActiveSelectionCursor != selectionCursor;
        ActiveSelectionCursor = selectionCursor;

        var nextCursor = selectionCursor switch
        {
            SelectionCursor.Move => StandardCursorType.SizeAll,
            SelectionCursor.SizeNWSE => StandardCursorType.TopLeftCorner,
            SelectionCursor.SizeNESW => StandardCursorType.TopRightCorner,
            SelectionCursor.SizeNS => StandardCursorType.TopSide,
            SelectionCursor.SizeWE => StandardCursorType.LeftSide,
            _ => StandardCursorType.Arrow
        };

        if (isLogicalCursorChanged || ActiveCursor != nextCursor)
        {
            ActiveCursor = nextCursor;
            NotifyStateChanged();
        }
    }

    private void EnsureInteractionSession(DrawingBuffer? buffer, IDrawStyle drawStyle)
    {
        if (buffer == null) return;
        if (_interactionSession == null)
        {
            _interactionSession = new CanvasInteractionSession(buffer, drawStyle);
        }
    }

    private bool IsOperating => CurrentBuffer != null && (CurrentBuffer.IsDrawing() || _interactionSession?.SelectionState is DraggingState or RegionSelectingState or AnimationEditingState or ResizingState);

    public void PointerBegin(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        _lastMousePosition = pos;
        var canvasPos = new Position(_magnification.Minify(pos.X), _magnification.Minify(pos.Y));
        int handleSize = Math.Max(1, (int)(8 / _magnification.Value));

        var currentBuffer = CurrentBuffer;
        if (currentBuffer == null) return;
        if (currentBuffer.IsDrawing() && !(_interactionSession?.SelectionState is SelectionPreviewState)) return;

        EnsureInteractionSession(currentBuffer, drawStyle);
        if (_interactionSession == null) return;
        _operationInitialSelectingArea = SelectingArea;

        var workingSession = _sessionProvider.CurrentSession;
        if (workingSession == null) return;

        var currentArea = HalfBoxArea.Create(canvasPos, gridSize);

        var previousState = _interactionSession.SelectionState;

        // 1. プレビュー状態中に別のツールで描画を開始しようとした場合、確定する
        if (previousState is SelectionPreviewState && drawStyle is not RegionSelector)
        {
            if (previousState.GetCursor(canvasPos, handleSize) == SelectionCursor.Default)
            {
                workingSession = previousState.Commit(workingSession, ImageBlender, BackgroundColor);
                workingSession = workingSession.UpdateSelectingArea(null);
                _sessionProvider.Update(workingSession);
                // 確定後の最新バッファと、確定後の状態(NormalCursorState)でセッションを更新
                _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, new NormalCursorState(currentArea));
                previousState = _interactionSession.SelectionState;
            }
        }
        else
        {
            // 確定が発生しなかった場合でも、最新のバッファでセッションを確実に同期させる
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, previousState);
        }

        var currentState = _interactionSession.SelectionState;
        UpdateCursor(canvasPos);
        var nextState = currentState.HandlePointerLeftButtonPressed(
            currentArea,
            canvasPos,
            null,
            () => workingSession.Buffer.Fetch(),
            internalUpdateCommand,
            handleSize);

        if (nextState is DraggingState or ResizingState)
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
            workingSession = workingSession.UpdateSelectingArea(null);
            _sessionProvider.Update(workingSession);
            workingSession = _sessionProvider.CurrentSession!;
            // 確定後のバッファでセッションを再構築し、状態をNormalCursorStateにリセット
            nextState = new NormalCursorState(currentArea);
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, nextState);
            NotifyStateChanged();
        }

        // 4. 新しい描画の開始
        DrawingResult result = _drawableArea.DrawStart(drawStyle, penStyle, workingSession.Buffer, pos, isShift);
        if (result.PictureBuffer != null && result.PictureBuffer != workingSession.Buffer)
        {
            workingSession = workingSession.UpdateBuffer(result.PictureBuffer);
            _sessionProvider.Update(workingSession);
            workingSession = _sessionProvider.CurrentSession!;
        }
        _drawableArea = result.DrawableArea;
        // 確定や新規描画開始によって状態が遷移しているため、nextState(または遷移後の最新状態)を確実に反映させる
        if (workingSession.Buffer != null)
        {
            _interactionSession = new CanvasInteractionSession(workingSession.Buffer, drawStyle, nextState);
        }
        NotifyStateChanged();
    }

    public void PointerMoved(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, PenStyle penStyle, bool isShift, bool isAnimationMode, PictureSize gridSize)
    {
        _lastMousePosition = pos;
        var canvasPos = new Position(_magnification.Minify(pos.X), _magnification.Minify(pos.Y));

        var currentBuffer = CurrentBuffer;
        if (currentBuffer == null) return;
        EnsureInteractionSession(currentBuffer, drawStyle);
        if (_interactionSession?.SelectionState == null) return;

        UpdateCursor(canvasPos);

        var currentArea = HalfBoxArea.Create(canvasPos, gridSize);

        (bool visible, HalfBoxArea nextCursorArea) = _interactionSession.SelectionState.HandlePointerMoved(
            currentArea,
            true,
            canvasPos,
            isShift,
            currentBuffer.Previous.Size);

        // SelectionState自体が更新（DraggingState内部の座標更新等）されるため、セッションを再構築
        _interactionSession = new CanvasInteractionSession(currentBuffer, drawStyle, _interactionSession.SelectionState);

        if (_interactionSession.SelectionState is DraggingState or SelectionPreviewState or ResizingState)
        {
            _drawableArea = _drawableArea.Leave(currentBuffer);
            NotifyStateChanged();
            return;
        }

        DrawingResult result = _drawableArea.Move(drawStyle, penStyle, currentBuffer, pos, isShift);
        if (_sessionProvider.CurrentSession != null && result.PictureBuffer != null)
        {
            _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
        }
        _drawableArea = result.DrawableArea;
        if (result.PictureBuffer != null)
        {
            _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession.SelectionState);
        }
        NotifyStateChanged();
    }

    public void PointerRightButtonPressed(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, Action<ArgbColor> colorPickedAction, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        _lastMousePosition = pos;
        var currentBuffer = CurrentBuffer;
        if (currentBuffer == null) return;
        EnsureInteractionSession(currentBuffer, drawStyle);
        if (_interactionSession?.SelectionState == null) return;

        var canvasPos = new Position(_magnification.Minify(pos.X), _magnification.Minify(pos.Y));
        var currentArea = HalfBoxArea.Create(canvasPos, gridSize);

        var previousState = _interactionSession.SelectionState;
        (ISelectionState nextState, HalfBoxArea _) = previousState.HandlePointerRightButtonPressed(
            currentArea,
            canvasPos,
            gridSize,
            internalUpdateCommand);

        if (previousState is SelectionPreviewState && nextState is NormalCursorState)
        {
            if (_sessionProvider.CurrentSession != null)
            {
                _sessionProvider.Update(previousState.Cancel(_sessionProvider.CurrentSession));
            }
            _interactionSession = new CanvasInteractionSession(currentBuffer, drawStyle, nextState);
            NotifyStateChanged();
            return;
        }
        else
        {
            _interactionSession = new CanvasInteractionSession(currentBuffer, drawStyle, nextState);
        }

        if (currentBuffer.IsDrawing())
        {
            DrawingResult result = _drawableArea.DrawCancel(currentBuffer);
            if (_sessionProvider.CurrentSession != null && result.PictureBuffer != null)
            {
                _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
            }
            _drawableArea = result.DrawableArea;
            if (result.PictureBuffer != null)
            {
                _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession.SelectionState);
            }
        }

        if (previousState is not SelectionPreviewState)
        {
            ArgbColor newColor = _drawableArea.PickColor(currentBuffer.Fetch(), pos);
            colorPickedAction?.Invoke(newColor);
        }
        NotifyStateChanged();
    }

    public void PointerLeftButtonReleased(Position pos, DrawingBuffer buffer, IDrawStyle drawStyle, bool isAnimationMode, PictureSize gridSize, PenStyle penStyle, bool isShift, ReactiveCommand<Picture, Unit> internalUpdateCommand)
    {
        _lastMousePosition = pos;
        var canvasPos = new Position(_magnification.Minify(pos.X), _magnification.Minify(pos.Y));

        var currentBuffer = CurrentBuffer;
        if (currentBuffer == null) return;
        EnsureInteractionSession(currentBuffer, drawStyle);
        if (_interactionSession?.SelectionState == null) return;

        var previousImage = currentBuffer.Previous;
        UpdateCursor(canvasPos);

        var currentArea = HalfBoxArea.Create(canvasPos, gridSize);

        var previousState = _interactionSession.SelectionState;
        ISelectionState nextState = previousState.HandlePointerLeftButtonReleased(
            currentArea,
            canvasPos,
            internalUpdateCommand,
            internalUpdateCommand);

        if (previousState is DraggingState or ResizingState)
        {
            var info = nextState.GetSelectionPreviewInfo();
            // 先に内部状態を更新しておくことで、Update(session) が呼ぶ SyncWithSession() が
            // 正しい nextState を参照できるようにする
            _interactionSession = new CanvasInteractionSession(currentBuffer, drawStyle, nextState);
            if (info != null && _sessionProvider.CurrentSession != null)
            {
                _sessionProvider.Update(_sessionProvider.CurrentSession.UpdatePreviewContent(info));
            }
            NotifyStateChanged();
            return;
        }

        _interactionSession = new CanvasInteractionSession(currentBuffer, drawStyle, nextState);

        if (currentBuffer.IsDrawing())
        {
            DrawingResult result = _drawableArea.DrawEnd(drawStyle, penStyle, currentBuffer, pos, isShift);
            _drawableArea = result.DrawableArea;
            if (result.PictureBuffer != null)
            {
                _interactionSession = new CanvasInteractionSession(result.PictureBuffer, drawStyle, _interactionSession.SelectionState);

                // セッションを更新する前にイベントを飛ばすことで、
                // DrawingSession.Push が「現在の Previous」を「変更前」として記録できるようにする。
                // ただし、ViewModel 側が同期的に Push を呼ぶため、イベント引数として
                // 更新後の画像を確実に渡す必要がある。
                Drew?.Invoke(previousImage, result.PictureBuffer.Previous, _operationInitialSelectingArea, SelectingArea, result.AffectedArea);

                if (_sessionProvider.CurrentSession != null)
                {
                    _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateBuffer(result.PictureBuffer));
                }
            }
        }
        NotifyStateChanged();
    }

    public void CanvasLeave(DrawingBuffer buffer)
    {
        var currentBuffer = CurrentBuffer;
        if (currentBuffer == null) return;
        _drawableArea = _drawableArea.Leave(currentBuffer);
        NotifyStateChanged();
    }

    public void CommitSelection(bool forceClearSelection = false)
    {
        _manualSelectingArea = null;
        var interactionSession = _interactionSession;
        var workingSession = _sessionProvider.CurrentSession;

        if (interactionSession?.SelectionState != null && workingSession != null)
        {
            var nextSession = interactionSession.SelectionState.Commit(workingSession, ImageBlender, BackgroundColor);
            if (forceClearSelection || interactionSession.DrawStyle is not RegionSelector)
            {
                nextSession = nextSession.UpdateSelectingArea(null);
            }

            if (nextSession != workingSession)
            {
                _sessionProvider.Update(nextSession);

                // Updateの副作用で _interactionSession が変更されている可能性があるため、
                // ローカルに保持した interactionSession の情報をもとに安全に再構築する。
                var area = nextSession.CurrentSelectingArea;
                if (area.HasValue && !area.Value.IsEmpty && interactionSession.DrawStyle is RegionSelector)
                {
                    if (nextSession.Buffer != null)
                    {
                        _interactionSession = new CanvasInteractionSession(nextSession.Buffer, interactionSession.DrawStyle, new SelectedState(new Selection(area.Value)));
                    }
                }
                else
                {
                    if (nextSession.Buffer != null)
                    {
                        _interactionSession = new CanvasInteractionSession(nextSession.Buffer, interactionSession.DrawStyle, new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16))));
                    }
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

        bool isPreviewInSession = session.CurrentPreviewContent != null;
        bool isPreviewInState = _interactionSession?.SelectionState is SelectionPreviewState or DraggingState or ResizingState;
        bool isStateMismatch = isPreviewInSession != isPreviewInState;

        // 操作中（ドラッグやリサイズ中）は、ローカルの状態が優先されるため、
        // 外部からの強制リセットや状態の不一致（プレビューの有無の変化）がない限り同期をスキップする。
        // 特に移動中は statePos と sessionPos が不一致になるため、ここでリセットしてはいけない。
        if (IsOperating && !forceReset && !isStateMismatch)
        {
            // 操作中でもバッファが外部要因（Undo等）で変わった場合は、セッションのバッファを同期する
            if (_interactionSession != null && _interactionSession.Buffer != session.Buffer)
            {
                _interactionSession = new CanvasInteractionSession(session.Buffer!, _interactionSession.DrawStyle, _interactionSession.SelectionState);
                NotifyStateChanged();
            }
            return;
        }

        // セッション内のプレビュー位置と、現在の状態が保持しているプレビュー位置が異なる場合も
        // 外部からの更新（Undo等）とみなして同期対象とする
        bool isPositionMismatch = false;
        if (isPreviewInSession && isPreviewInState)
        {
            var statePos = _interactionSession?.SelectionState?.GetSelectionPreviewInfo()?.Position;
            if (statePos.HasValue && statePos.Value != session.CurrentPreviewContent!.Position)
            {
                isPositionMismatch = true;
            }
        }
        else if (isPreviewInState && !isPreviewInSession)
        {
            // Coordinator側にはプレビューがあるが、セッション側にはない（確定された）場合
            isPositionMismatch = true;
        }

        if (forceReset || isStateMismatch || isPositionMismatch || (_interactionSession == null) || (_interactionSession.Buffer != session.Buffer))
        {
            var currentStyle = _interactionSession?.DrawStyle ?? new RegionSelector();
            if (session.Buffer != null)
            {
                if (session.CurrentPreviewContent != null)
                {
                    _interactionSession = new CanvasInteractionSession(session.Buffer, currentStyle, new SelectionPreviewState(session.CurrentPreviewContent));
                }
                else
                {
                    var area = session.CurrentSelectingArea;
                    if (area.HasValue && !area.Value.IsEmpty && currentStyle is RegionSelector)
                    {
                        _interactionSession = new CanvasInteractionSession(session.Buffer, currentStyle, new SelectedState(new Selection(area.Value)));
                    }
                    else
                    {
                        _interactionSession = new CanvasInteractionSession(session.Buffer, currentStyle, new NormalCursorState(HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16))));
                    }
                }
            }
            NotifyStateChanged();
        }
        var canvasPos = new Position(_magnification.Minify(_lastMousePosition.X), _magnification.Minify(_lastMousePosition.Y));
        UpdateCursor(canvasPos);
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
            var currentBuffer = CurrentBuffer;
            if (currentBuffer == null) return;
            _manualSelectingArea = null;
            var currentArea = HalfBoxArea.Create(args.Start, gridSize);
            _interactionSession = new CanvasInteractionSession(currentBuffer, tool, new NormalCursorState(currentArea));
            NotifyStateChanged();
        };
        tool.OnDrawing += (sender, args) =>
        {
            var currentBuffer = CurrentBuffer;
            if (currentBuffer == null) return;
            _manualSelectingArea = PictureArea.FromPosition(args.Start, args.Now, currentBuffer.Previous.Size);
            NotifyStateChanged();
        };
        tool.OnDrawEnd += (sender, args) =>
        {
            var currentBuffer = CurrentBuffer;
            if (currentBuffer == null) return;
            _manualSelectingArea = null;
            var area = PictureArea.FromPosition(args.Start, args.Now, currentBuffer.Previous.Size);
            if (area.IsEmpty)
            {
                _interactionSession = new CanvasInteractionSession(currentBuffer, tool, new NormalCursorState(HalfBoxArea.Create(args.Now, gridSize)));
                if (_sessionProvider.CurrentSession != null)
                {
                    _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateSelectingArea(null));
                }
            }
            else
            {
                var selection = new Selection(area);
                _interactionSession = new CanvasInteractionSession(currentBuffer, tool, new SelectedState(selection));
                if (_sessionProvider.CurrentSession != null)
                {
                    _sessionProvider.Update(_sessionProvider.CurrentSession.UpdateSelectingArea(selection.Area));
                }
            }

            NotifyStateChanged();
        };
    }

    public Picture? Painted(DrawingBuffer buffer, PenStyle penStyle, IImageTransfer imageTransfer)
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
