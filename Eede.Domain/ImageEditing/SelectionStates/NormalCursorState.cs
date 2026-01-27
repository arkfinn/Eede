using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class NormalCursorState : ISelectionState
{
    private HalfBoxArea _selectingArea;
    private HalfBoxArea _cursorArea;

    public NormalCursorState(HalfBoxArea initialSelectingArea)
    {
        _selectingArea = initialSelectingArea;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, Action<Position>? pullAction, Func<Picture> getPicture, Action<Picture>? updateAction)
    {
        pullAction?.Invoke(cursorArea.RealPosition);
        _cursorArea = cursorArea;
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, Action<Picture>? picturePushAction, Action<Picture>? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, Action<Picture>? pictureUpdateAction)
    {
        _cursorArea = cursorArea;
        HalfBoxArea selectingArea = HalfBoxArea.Create(nowPosition, minCursorSize);
        return (BeginRegionSelection(cursorArea, selectingArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        HalfBoxArea newCursorArea = cursorArea.Move(nowPosition);
        _cursorArea = newCursorArea;
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
        return SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return null;
    }

    private RegionSelectingState BeginRegionSelection(HalfBoxArea cursorArea, HalfBoxArea selectingArea)
    {
        return new RegionSelectingState(cursorArea, selectingArea);
    }
}