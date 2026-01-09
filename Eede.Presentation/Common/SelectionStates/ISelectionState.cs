using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal record SelectionPreviewInfo(Picture Pixels, Position Position);

    internal interface ISelectionState
    {
        ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Func<Picture> getPicture);
        ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction, ICommand pictureUpdateAction);
        (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize);
        (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize);
        (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction);
        SelectionPreviewInfo GetSelectionPreviewInfo();
    }
}
