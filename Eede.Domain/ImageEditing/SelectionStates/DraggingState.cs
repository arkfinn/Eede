using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class DraggingState : ISelectionState
{
    private readonly Picture _pixels;
    private readonly PictureArea _originalArea;
    private readonly Position _startPosition;
    private Position _nowPosition;
    private readonly SelectionPreviewType _type;

    public DraggingState(Picture pixels, PictureArea originalArea, Position startPosition, SelectionPreviewType type = SelectionPreviewType.CutAndMove)
    {
        _pixels = pixels;
        _originalArea = originalArea;
        _startPosition = startPosition;
        _nowPosition = startPosition;
        _type = type;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction)
    {
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
    {
        var info = GetSelectionPreviewInfo();
        if (info != null)
        {
            return new SelectionPreviewState(info);
        }
        return new NormalCursorState(cursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
    {
        return (this, cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        _nowPosition = nowPosition;
        return (visibleCursor, cursorArea.Move(nowPosition));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        var offset = new Position(_nowPosition.X - _startPosition.X, _nowPosition.Y - _startPosition.Y);
        var nextPosition = new Position(_originalArea.X + offset.X, _originalArea.Y + offset.Y);
        var originalArea = _type == SelectionPreviewType.CutAndMove ? _originalArea : (PictureArea?)null;
        return new SelectionPreviewInfo(_pixels, nextPosition, _type, originalArea);
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

    public DrawingSession Commit(DrawingSession session)
    {
        return session;
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session;
    }
}
