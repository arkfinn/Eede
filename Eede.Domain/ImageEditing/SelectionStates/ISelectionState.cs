using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.SelectionStates;

public record SelectionPreviewInfo(Picture Pixels, Position Position);

public interface ISelectionState
{
    ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, Action<Position>? pullAction, Func<Picture> getPicture, Action<Picture>? updateAction);
    ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, Action<Picture>? picturePushAction, Action<Picture>? pictureUpdateAction);
    (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, Action<Picture>? pictureUpdateAction);
    (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize);
    (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, Action<Picture>? picturePushAction);
    SelectionPreviewInfo? GetSelectionPreviewInfo();
    SelectionCursor GetCursor(Position mousePosition);
    PictureArea? GetSelectingArea();
}

public enum SelectionCursor
{
    Default,
    Move
}