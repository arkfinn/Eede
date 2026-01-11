using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Application.Common.SelectionStates
{
    public class RegionSelectingState : ISelectionState
    {
        private HalfBoxArea _cursorArea;
        private HalfBoxArea _selectingArea;

        public RegionSelectingState(HalfBoxArea cursorArea, HalfBoxArea selectingArea)
        {
            _cursorArea = cursorArea;
            _selectingArea = selectingArea;
        }

        public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand pullAction, Func<Picture> getPicture, ICommand updateAction)
        {
            return this;
        }

        public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand picturePushAction, ICommand pictureUpdateAction)
        {
            return this;
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand pictureUpdateAction)
        {
            return (this, cursorArea);
        }

        public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
        {
            _selectingArea = _selectingArea.ResizeToLocation(nowPosition);
            return (visibleCursor, _selectingArea);
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction)
        {
            SelectedState newSelectionState = EndRegionSelection();
            return (newSelectionState, _selectingArea);
        }

        public SelectionPreviewInfo GetSelectionPreviewInfo()
        {
            return null;
        }

        public SelectionCursor GetCursor(Position mousePosition)
        {
            return SelectionCursor.Default;
        }

        private SelectedState EndRegionSelection()
        {
            var area = _selectingArea.CreateRealArea(_selectingArea.BoxSize);
            return new SelectedState(new Selection(area));
        }
    }
}
