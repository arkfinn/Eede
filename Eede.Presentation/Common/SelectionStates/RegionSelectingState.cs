using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
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

        public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Func<Picture> getPicture)
        {
            return this;
        }

        public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction, ICommand pictureUpdateAction)
        {
            return this;
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

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction)
        {
            SelectedState newSelectionState = EndRegionSelection();
            return (newSelectionState, _cursorArea);
        }

        public SelectionPreviewInfo GetSelectionPreviewInfo()
        {
            return null;
        }

        private SelectedState EndRegionSelection()
        {
            var area = _cursorArea.CreateRealArea(_cursorArea.BoxSize);
            return new SelectedState(new Selection(area));
        }
    }
}
