using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectedState : ISelectionState
{
    private readonly Selection _selection;

    public SelectedState(Selection selection)
    {
        _selection = selection;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction, int handleSize = 8)
    {
        var handle = SelectionHandleDetector.Detect(_selection.Area, mousePosition, handleSize);
        if (handle.HasValue)
        {
            var picture = getPicture();
            var sourcePicture = picture.CutOut(_selection.Area);
            return new ResizingState(sourcePicture, _selection.Area, mousePosition, handle.Value, new NearestNeighborResampler());
        }

        // mousePosition (生のクリック位置) と cursorArea.RealPosition (スナップ位置) の両方をチェック
        // 高倍率時の 1ピクセル未満の判定誤差を許容するため、遊び(margin)を持たせる
        int margin = Math.Max(1, handleSize / 2);
        bool containsMouse = ContainsWithMargin(_selection.Area, mousePosition, margin);
        bool containsCursor = ContainsWithMargin(_selection.Area, cursorArea.RealPosition, margin);

        if (containsMouse || containsCursor)
        {
            var picture = getPicture();
            var cutPicture = picture.CutOut(_selection.Area);
            return new DraggingState(cutPicture, cutPicture, _selection.Area, mousePosition, SelectionPreviewType.CutAndMove, _selection.Area);
        }
        return new NormalCursorState(cursorArea);
    }

    private bool ContainsWithMargin(PictureArea area, Position position, int margin)
    {
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
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        HalfBoxArea newCursorArea = cursorArea.Move(nowPosition);
        return (newVisibleCursor, newCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return null;
    }

    public SelectionCursor GetCursor(Position mousePosition, int handleSize = 8)
    {
        var handle = SelectionHandleDetector.Detect(_selection.Area, mousePosition, handleSize);
        if (handle.HasValue)
        {
            return GetCursorForHandle(handle.Value);
        }
        return _selection.Contains(mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return _selection.Area;
    }

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        return session;
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session;
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
