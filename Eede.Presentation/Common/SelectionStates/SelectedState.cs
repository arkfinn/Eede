using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates;

internal class SelectedState : ISelectionState
{
    private readonly Selection _selection;

    public SelectedState(Selection selection)
    {
        _selection = selection;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Func<Picture> getPicture)
    {
        if (_selection.Contains(cursorArea.RealPosition))
        {
            var picture = getPicture?.Invoke();
            if (picture != null)
            {
                var content = new SelectionContent(picture.CutOut(_selection.Area), _selection);
                return new DraggingState(content, cursorArea.RealPosition, picture);
            }
        }
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction, ICommand pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize)
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
}
