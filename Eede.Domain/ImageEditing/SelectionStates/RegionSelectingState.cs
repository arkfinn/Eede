using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class RegionSelectingState : ISelectionState
{
    private HalfBoxArea _startCursorArea;
    private HalfBoxArea _nowCursorArea;

    public RegionSelectingState(HalfBoxArea startCursorArea, HalfBoxArea nowCursorArea)
    {
        _startCursorArea = startCursorArea;
        _nowCursorArea = nowCursorArea;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, Action<Position>? pullAction, Func<Picture> getPicture, Action<Picture>? updateAction)
    {
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, Action<Picture>? picturePushAction, Action<Picture>? pictureUpdateAction)
    {
        var area = GetSelectingArea();
        if (area.HasValue && area.Value.Width > 0 && area.Value.Height > 0)
        {
            return new SelectedState(new Selection(area.Value));
        }
        return new NormalCursorState(_nowCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, Action<Picture>? pictureUpdateAction)
    {
        return (this, cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        _nowCursorArea = cursorArea.Move(nowPosition);
        return (newVisibleCursor, _nowCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, Action<Picture>? picturePushAction)
    {
        return (new NormalCursorState(_nowCursorArea), cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return null;
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return PictureArea.FromPosition(_startCursorArea.RealPosition, _nowCursorArea.RealPosition, new PictureSize(int.MaxValue, int.MaxValue));
    }
}