using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectionPreviewState : ISelectionState
{
    private readonly SelectionPreviewInfo _info;
    private readonly Picture _sourcePixels;

    public SelectionPreviewState(SelectionPreviewInfo info, Picture sourcePixels = null)
    {
        _info = info;
        _sourcePixels = sourcePixels ?? info.SourcePixels;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction, int handleSize = 8)
    {
        var currentArea = new PictureArea(_info.Position, _info.Pixels.Size);

        var handle = SelectionHandleDetector.Detect(currentArea, mousePosition, handleSize);
        if (handle.HasValue)
        {
            // 再リサイズ時も _sourcePixels (変形前の元画像) を渡すことで画質劣化を防ぎ、
            // 連続したリサイズ操作を可能にする
            return new ResizingState(_sourcePixels, currentArea, mousePosition, handle.Value, new NearestNeighborResampler(), _info.Type, _info.OriginalArea);
        }

        // Contains の判定にも handleSize 相当の遊びを持たせるか、
        // あるいは境界ピクセルの判定を緩和する。
        // ここでは単純な包含判定ではなく、ハンドル検出に使用したロジックとの整合性を高める。
        if (Contains(currentArea, mousePosition, handleSize))
        {
            return new DraggingState(_info.Pixels, _sourcePixels, currentArea, mousePosition, _info.Type, _info.OriginalArea);
        }

        return new NormalCursorState(cursorArea);
    }

    private bool Contains(PictureArea area, Position position, int handleSize = 0)
    {
        // 高倍率時の判定誤差を吸収するため、handleSize の半分程度の遊びを持たせる
        // 少なくとも 1ピクセル（キャンバス座標）の遊びを確保する
        int margin = Math.Max(1, handleSize / 2);
        return position.X >= area.X - margin && position.X < area.X + area.Width + margin &&
               position.Y >= area.Y - margin && position.Y < area.Y + area.Height + margin;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
    {
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, bool isShift, PictureSize canvasSize)
    {
        return (visibleCursor, cursorArea.Move(nowPosition));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return _info with { SourcePixels = _sourcePixels };
    }

    public SelectionCursor GetCursor(Position mousePosition, int handleSize = 8)
    {
        var currentArea = new PictureArea(_info.Position, _info.Pixels.Size);

        var handle = SelectionHandleDetector.Detect(currentArea, mousePosition, handleSize);
        if (handle.HasValue)
        {
            return GetCursorForHandle(handle.Value);
        }

        return Contains(currentArea, mousePosition, handleSize) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return new PictureArea(_info.Position, _info.Pixels.Size);
    }

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        var info = _info;
        if (blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
        {
            info = new SelectionPreviewInfo(
                info.Pixels.ApplyTransparency(backgroundColor),
                info.Position,
                info.Type,
                info.OriginalArea,
                info.SourcePixels);
        }
        return session.UpdatePreviewContent(info).CommitPreview(blender);
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session.UpdatePreviewContent(_info).CancelDrawing();
    }

    private SelectionCursor GetCursorForHandle(SelectionHandle handle)
    {
        return handle switch
        {
            SelectionHandle.TopLeft => SelectionCursor.SizeNWSE,
            SelectionHandle.BottomRight => SelectionCursor.SizeNWSE,
            SelectionHandle.TopRight => SelectionCursor.SizeNESW,
            SelectionHandle.BottomLeft => SelectionCursor.SizeNESW,
            SelectionHandle.Top => SelectionCursor.SizeNS,
            SelectionHandle.Bottom => SelectionCursor.SizeNS,
            SelectionHandle.Left => SelectionCursor.SizeWE,
            SelectionHandle.Right => SelectionCursor.SizeWE,
            _ => SelectionCursor.Default
        };
    }
}
