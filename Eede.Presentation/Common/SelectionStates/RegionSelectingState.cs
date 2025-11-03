using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.SharedKernel;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates
{
    internal class RegionSelectingState(HalfBoxArea cursorArea, HalfBoxArea selectingArea) : ISelectionState
    {
        private HalfBoxArea CursorArea = cursorArea;
        private HalfBoxArea SelectingArea = selectingArea;

        public HalfBoxArea GetCurrentArea()
        {
            return SelectingArea;
        }

        public void HandlePointerLeftButtonPressed(ICommand picturePullAction)
        {
            // 選択中の左クリック - 選択をキャンセルまたは確定する場合の処理
            // 現在のコードでは何もしない
        }

        public ISelectionState HandlePointerRightButtonPressed(Position nowPosition, PictureSize minCursorSize)
        {
            return this; // すでに選択中なので、状態を変えない
        }

        public (bool, HalfBoxArea) HandlePointerMoved(bool visibleCursor, Position nowPosition, PictureSize canvasSize)
        {
            SelectingArea = SelectingArea.ResizeToLocation(nowPosition);
            // 戻り値は受け取った時点から変更しない
            return (visibleCursor, CursorArea);
        }

        public (HalfBoxArea, ISelectionState) HandlePointerRightButtonReleased(ICommand PicturePushAction)
        {
            PictureArea area = SelectingArea.CreateRealArea(SelectingArea.BoxSize);
            PicturePushAction?.Execute(area);
            CursorArea = SelectingArea;
            NormalCursorState newSelectionState = EndRegionSelection();

            return (CursorArea, newSelectionState);
        }

        private NormalCursorState EndRegionSelection()
        {
            return new NormalCursorState(CursorArea, SelectingArea);
        }
    }
}
