using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class FloatingSelectionState : ISelectionState
{
    private readonly Picture _pixels;
    private Position _position;
    private readonly Picture _basePicture;

    public FloatingSelectionState(Picture pixels, Position position, Picture basePicture)
    {
        _pixels = pixels;
        _position = position;
        _basePicture = basePicture;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction)
    {
        // フローティング状態からドラッグ開始
        return new DraggingState(_pixels, new PictureArea(_position, _pixels.Size), mousePosition);
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
    {
        // 右クリックで確定して通常状態へ
        pictureUpdateAction?.Execute(_basePicture.Blend(new Blending.DirectImageBlender(), _pixels, _position));
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        return (visibleCursor, cursorArea.Move(nowPosition));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return new SelectionPreviewInfo(_pixels, _position);
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return SelectionCursor.Move;
    }

    public PictureArea? GetSelectingArea()
    {
        return new PictureArea(_position, _pixels.Size);
    }
}
