using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal class NormalCursorState : ISelectionState
    {
        private HalfBoxArea _selectingArea;
        private HalfBoxArea _cursorArea;

        public NormalCursorState(HalfBoxArea initialSelectingArea)
        {
            _selectingArea = initialSelectingArea;
        }

        public void HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, ICommand picturePullAction)
        {
            picturePullAction?.Execute(cursorArea.RealPosition);
            _cursorArea = cursorArea;
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize)
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

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction)
        {
            return (this, cursorArea);
        }

        private RegionSelectingState BeginRegionSelection(HalfBoxArea cursorArea, HalfBoxArea selectingArea)
        {
            return new RegionSelectingState(cursorArea, selectingArea);
        }
    }
}
