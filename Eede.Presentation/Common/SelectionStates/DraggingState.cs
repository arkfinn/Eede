using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Common.SelectionStates;

internal class DraggingState : ISelectionState
{
    private readonly SelectionContent _content;
    private readonly Position _startPosition;
    private Position _currentPosition;
    private readonly Picture _originalPicture;

    public DraggingState(SelectionContent content, Position startPosition, Picture originalPicture)
    {
        _content = content;
        _startPosition = startPosition;
        _currentPosition = startPosition;
        _originalPicture = originalPicture;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Func<Picture> getPicture)
    {
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, ICommand picturePushAction, ICommand pictureUpdateAction)
    {
        var blender = new DirectImageBlender();
        var empty = Picture.CreateEmpty(_content.OriginalSelection.Area.Size);
        var pictureAfterClear = blender.Blend(empty, _originalPicture, _content.OriginalSelection.Area.Position);

        var finalArea = GetCurrentArea();
        var finalPicture = blender.Blend(_content.Image, pictureAfterClear, finalArea.Position);

        pictureUpdateAction?.Execute(finalPicture);

        return new SelectedState(new Selection(finalArea));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize)
    {
        return (this, cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        _currentPosition = nowPosition;
        // ドラッグ中はカーソル自体は移動させるが、選択範囲のプレビューは別で表示する
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
        return new SelectionPreviewInfo(_content.Image, GetCurrentArea().Position);
    }

    private PictureArea GetCurrentArea()
    {
        int deltaX = _currentPosition.X - _startPosition.X;
        int deltaY = _currentPosition.Y - _startPosition.Y;
        var originalArea = _content.OriginalSelection.Area;
        return new PictureArea(
            new Position(originalArea.X + deltaX, originalArea.Y + deltaY),
            originalArea.Size);
    }
}
