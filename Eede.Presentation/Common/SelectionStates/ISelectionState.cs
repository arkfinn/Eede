using Eede.Domain.ImageEditing;
using Eede.Domain.Pictures;
using Eede.Domain.SharedKernel;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal interface ISelectionState
    {
        HalfBoxArea GetCurrentArea();
        void HandlePointerLeftButtonPressed(ICommand picturePullAction);
        ISelectionState HandlePointerRightButtonPressed(Position nowPosition, PictureSize minCursorSize);
        (bool, HalfBoxArea) HandlePointerMoved(bool visibleCursor, Position nowPosition, PictureSize canvasSize);
        (HalfBoxArea, ISelectionState) HandlePointerRightButtonReleased(ICommand PicturePushAction);
    }
}
