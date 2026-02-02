using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectedState : ISelectionState
{
    private readonly Selection _selection;

    public SelectedState(Selection selection)
    {
        _selection = selection;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        if (_selection.Contains(mousePosition))
        {
            var picture = getPicture();
            var cutPicture = picture.CutOut(_selection.Area);
            if (blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
            {
                cutPicture = MakeTransparent(cutPicture, backgroundColor);
            }
            // Coordinator側でBufferを更新するため、ここではupdateActionを呼ばない
            return new DraggingState(cutPicture, _selection.Area, mousePosition);
        }
        return new NormalCursorState(cursorArea);
    }

    private Picture MakeTransparent(Picture picture, Eede.Domain.Palettes.ArgbColor transparentColor)
    {
        byte[] pixels = picture.CloneImage();
        for (int i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i] == transparentColor.Blue &&
                pixels[i + 1] == transparentColor.Green &&
                pixels[i + 2] == transparentColor.Red &&
                pixels[i + 3] == transparentColor.Alpha)
            {
                pixels[i + 3] = 0;
            }
        }
        return Picture.Create(picture.Size, pixels);
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
    {
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        HalfBoxArea newCursorArea = cursorArea.Move(nowPosition);
        return (newVisibleCursor, newCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return null;
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        return _selection.Contains(mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return _selection.Area;
    }

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender)
    {
        return session;
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session;
    }
}
