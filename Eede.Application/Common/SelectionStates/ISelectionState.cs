using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Application.Common.SelectionStates
{
    public record SelectionPreviewInfo(Picture Pixels, Position Position);

    public interface ISelectionState
    {
        ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand pullAction, Func<Picture> getPicture, ICommand updateAction);
        ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand picturePushAction, ICommand pictureUpdateAction);
        (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand pictureUpdateAction);
        (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize);
        (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction);
        SelectionPreviewInfo GetSelectionPreviewInfo();
        SelectionCursor GetCursor(Position mousePosition);
        PictureArea? GetSelectingArea();
    }

    public enum SelectionCursor
    {
        Default,
        Move
    }
}
