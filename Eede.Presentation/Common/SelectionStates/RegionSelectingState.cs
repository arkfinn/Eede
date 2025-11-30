using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal class RegionSelectingState : ISelectionState
    {
        private HalfBoxArea _cursorArea;
        private HalfBoxArea _selectingArea;

        public RegionSelectingState(HalfBoxArea cursorArea, HalfBoxArea selectingArea)
        {
            _cursorArea = cursorArea;
            _selectingArea = selectingArea;
        }

        public void HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, ICommand picturePullAction)
        {
            // 何もしない
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize)
        {
            return (this, cursorArea);
        }

        public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
        {
            _selectingArea = _selectingArea.ResizeToLocation(nowPosition);
            return (visibleCursor, _selectingArea);
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand PicturePushAction)
        {
            PictureArea area = _selectingArea.CreateRealArea(_selectingArea.BoxSize);
            PicturePushAction?.Execute(area);
            _cursorArea = _selectingArea; // 選択範囲をCursorAreaに設定
            NormalCursorState newSelectionState = EndRegionSelection();

            return (newSelectionState, _cursorArea);
        }

        private NormalCursorState EndRegionSelection()
        {
            return new NormalCursorState(_cursorArea);
        }
    }
}
