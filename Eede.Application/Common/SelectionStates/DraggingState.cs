using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Application.Common.SelectionStates;

public class DraggingState : ISelectionState
{
    public readonly Picture OriginalPicture;
    private readonly SelectionContent _content;
    private readonly Position _startPosition;
    private Position _currentPosition;
    private readonly bool _isFloating;

    public DraggingState(SelectionContent content, Position startPosition, Picture originalPicture, bool isFloating = false)
    {
        _content = content;
        _startPosition = startPosition;
        _currentPosition = startPosition;
        OriginalPicture = originalPicture;
        _isFloating = isFloating;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand pullAction, Func<Picture> getPicture, ICommand updateAction)
    {
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand picturePushAction, ICommand pictureUpdateAction)
    {
        _currentPosition = mousePosition;
        
        var blender = new DirectImageBlender();
        Picture pictureAfterClear;
        if (_isFloating)
        {
            pictureAfterClear = OriginalPicture;
        }
        else
        {
            var empty = Picture.CreateEmpty(_content.OriginalSelection.Area.Size);
            pictureAfterClear = blender.Blend(empty, OriginalPicture, _content.OriginalSelection.Area.Position);
        }

        var finalArea = GetCurrentArea();
        var finalPicture = blender.Blend(_content.Image, pictureAfterClear, finalArea.Position);

        pictureUpdateAction?.Execute(finalPicture);

        return new SelectedState(new Selection(finalArea));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand pictureUpdateAction)
    {
        pictureUpdateAction?.Execute(OriginalPicture);
        return (new SelectedState(_content.OriginalSelection), cursorArea);
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

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return SelectionCursor.Move;
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
