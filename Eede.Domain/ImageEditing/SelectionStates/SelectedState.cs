using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectedState : ISelectionState
{
    private readonly Selection _selection;

    public SelectedState(Selection selection)
    {
        _selection = selection;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, Action<Position>? pullAction, Func<Picture> getPicture, Action<Picture>? updateAction)
    {
        if (_selection.Contains(mousePosition))
        {
            var picture = getPicture();
            var cutPicture = picture.CutOut(_selection.Area);
            var nextPicture = picture.Clear(_selection.Area);
            updateAction?.Invoke(nextPicture);
            return new DraggingState(cutPicture, _selection.Area, mousePosition);
        }
        return new NormalCursorState(cursorArea);
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, Action<Picture>? picturePushAction, Action<Picture>? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, Action<Picture>? pictureUpdateAction)
    {
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        HalfBoxArea newCursorArea = cursorArea.Move(nowPosition);
        return (newVisibleCursor, newCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, Action<Picture>? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return null;
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return _selection.Contains(mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return _selection.Area;
    }
}