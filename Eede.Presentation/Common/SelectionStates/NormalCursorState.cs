using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal class NormalCursorState(HalfBoxArea cursorArea, HalfBoxArea selectingArea) : ISelectionState
    {
        private HalfBoxArea CursorArea = cursorArea;
        private HalfBoxArea SelectingArea = selectingArea;

        public HalfBoxArea GetCurrentArea()
        {
            return CursorArea;
        }

        public void HandlePointerLeftButtonPressed(ICommand picturePullAction)
        {
            picturePullAction?.Execute(CursorArea.RealPosition);
        }

        public ISelectionState HandlePointerRightButtonPressed(Position nowPosition, PictureSize minCursorSize)
        {
            SelectingArea = HalfBoxArea.Create(nowPosition, minCursorSize);
            return BeginRegionSelection();

        }

        public (bool, HalfBoxArea) HandlePointerMoved(bool visibleCursor, Position nowPosition, PictureSize canvasSize)
        {
            bool newVisibleCursor = canvasSize.Contains(nowPosition);
            CursorArea = CursorArea.Move(nowPosition);
            return (newVisibleCursor, CursorArea);
        }

        public (HalfBoxArea, ISelectionState) HandlePointerRightButtonReleased(ICommand PicturePushAction)
        {
            return (CursorArea, this);
        }

        private RegionSelectingState BeginRegionSelection()
        {
            return new RegionSelectingState(CursorArea, SelectingArea);
        }
    }
}
