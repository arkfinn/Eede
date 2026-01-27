using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class DraggingState : ISelectionState
{
    private readonly Picture _pixels;
    private readonly PictureArea _originalArea;
    private readonly Position _startPosition;
    private Position _nowPosition;

    public DraggingState(Picture pixels, PictureArea originalArea, Position startPosition)
    {
        _pixels = pixels;
        _originalArea = originalArea;
        _startPosition = startPosition;
        _nowPosition = startPosition;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, Action<Position>? pullAction, Func<Picture> getPicture, Action<Picture>? updateAction)
    {
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, Action<Picture>? picturePushAction, Action<Picture>? pictureUpdateAction)
    {
        var nextArea = GetSelectingArea();
        if (nextArea.HasValue)
        {
            return new SelectedState(new Selection(nextArea.Value));
        }
        return new NormalCursorState(cursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, Action<Picture>? pictureUpdateAction)
    {
        return (this, cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        _nowPosition = nowPosition;
        return (visibleCursor, cursorArea.Move(nowPosition));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, Action<Picture>? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        var offset = new Position(_nowPosition.X - _startPosition.X, _nowPosition.Y - _startPosition.Y);
        var nextPosition = new Position(_originalArea.X + offset.X, _originalArea.Y + offset.Y);
        return new SelectionPreviewInfo(_pixels, nextPosition);
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return SelectionCursor.Move;
    }

    public PictureArea? GetSelectingArea()
    {
        var info = GetSelectionPreviewInfo();
        return info == null ? null : new PictureArea(info.Position, info.Pixels.Size);
    }

    public PictureArea GetOriginalArea()
    {
        return _originalArea;
    }
}