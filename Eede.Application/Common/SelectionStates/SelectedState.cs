using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Application.Common.SelectionStates;

public class SelectedState : ISelectionState
{
    public readonly Selection Selection;

    public SelectedState(Selection selection)
    {
        Selection = selection;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand pullAction, Func<Picture> getPicture, ICommand updateAction)
    {
        if (Selection.Contains(mousePosition))
        {
            var picture = getPicture?.Invoke();
            if (picture != null)
            {
                var content = new SelectionContent(picture.CutOut(Selection.Area), Selection);

                // 元のピクセルを消去
                var clearedPicture = picture.Clear(Selection.Area);
                updateAction?.Execute(clearedPicture);

                return new DraggingState(content, mousePosition, picture);
            }
        }

        // 範囲外なら既存の Pull 処理を実行して選択解除
        pullAction?.Execute(cursorArea.RealPosition);
        return new NormalCursorState(cursorArea);
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand picturePushAction, ICommand pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand pictureUpdateAction)
    {
        // 範囲外で右クリックされたら選択解除して新しい選択を開始
        HalfBoxArea selectingArea = HalfBoxArea.Create(nowPosition, minCursorSize);
        return (new RegionSelectingState(cursorArea, selectingArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        HalfBoxArea newCursorArea = cursorArea.Move(nowPosition);
        return (newVisibleCursor, newCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo GetSelectionPreviewInfo()
    {
        return null;
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return Selection.Contains(mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return Selection.Area;
    }
}
