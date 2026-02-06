using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public interface ISelectionState
{
    ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction, int handleSize = 4);
    ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction);
    (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction);
    (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, bool isShift, PictureSize canvasSize);
    (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction);
    SelectionPreviewInfo? GetSelectionPreviewInfo();
    SelectionCursor GetCursor(Position mousePosition, int handleSize = 4);
    PictureArea? GetSelectingArea();

    DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor);
    DrawingSession Cancel(DrawingSession session);
}

public enum SelectionCursor
{
    Default,
    Move,
    SizeNWSE,
    SizeNESW,
    SizeNS,
    SizeWE
}
