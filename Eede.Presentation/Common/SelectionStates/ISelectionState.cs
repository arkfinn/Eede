using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal interface ISelectionState
    {
        void HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, ICommand picturePullAction);
        (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize);
        (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize);
        (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction);
    }
}
