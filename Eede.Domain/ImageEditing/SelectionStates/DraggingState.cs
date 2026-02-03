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
    private readonly PictureArea? _clearArea;

    public DraggingState(Picture pixels, PictureArea originalArea, Position startPosition, SelectionPreviewType type = SelectionPreviewType.CutAndMove, PictureArea? clearArea = null)
    {
        _pixels = pixels;
        _originalArea = originalArea;
        _startPosition = startPosition;
        _nowPosition = startPosition;
        _type = type;
        _clearArea = clearArea ?? (type == SelectionPreviewType.CutAndMove ? originalArea : null);
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
        return new SelectionPreviewInfo(_pixels, nextPosition, _type, _clearArea);
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

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        var info = GetSelectionPreviewInfo();
        if (info != null && blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
        {
            info = new SelectionPreviewInfo(
                info.Pixels.ApplyTransparency(backgroundColor),
                info.Position,
                info.Type,
                info.OriginalArea);
        }
        return session.UpdatePreviewContent(info).CommitPreview(blender);
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session.UpdatePreviewContent(GetSelectionPreviewInfo()).CancelDrawing();
    }
}
