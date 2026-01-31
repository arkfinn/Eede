using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Application.Common.SelectionStates;

public class FloatingSelectionState : ISelectionState
{
    private readonly Picture _pixels;
    private Position _position;
    private readonly Picture _originalPicture;

    public FloatingSelectionState(Picture pixels, Position position, Picture originalPicture)
    {
        _pixels = pixels;
        _position = position;
        _originalPicture = originalPicture;
    }

    private bool Contains(Position mousePosition)
    {
        return _position.X <= mousePosition.X && mousePosition.X < _position.X + _pixels.Width &&
               _position.Y <= mousePosition.Y && mousePosition.Y < _position.Y + _pixels.Height;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand pullAction, Func<Picture> getPicture, ICommand updateAction)
    {
        if (Contains(mousePosition))
        {
            var area = new PictureArea(_position, _pixels.Size);
            var selection = new Selection(area);
            var content = new SelectionContent(_pixels, selection);
            return new DraggingState(content, mousePosition, _originalPicture, isFloating: true);
        }

        // 範囲外クリックで確定
        var blender = new DirectImageBlender();
        var finalPicture = blender.Blend(_pixels, _originalPicture, _position);
        updateAction?.Execute(finalPicture);
        
        pullAction?.Execute(cursorArea.RealPosition);
        return new NormalCursorState(cursorArea);
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand picturePushAction, ICommand pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand pictureUpdateAction)
    {
        // 右クリックでも確定？ それともキャンセル？ 
        // 既存のSelectedStateはRegionSelectingStateに移行するので、ここでも確定して解除するのが良さそう
        var blender = new DirectImageBlender();
        var finalPicture = blender.Blend(_pixels, _originalPicture, _position);
        pictureUpdateAction?.Execute(finalPicture);

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
        return new SelectionPreviewInfo(_pixels, _position);
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return Contains(mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return new PictureArea(_position, _pixels.Size);
    }
}
