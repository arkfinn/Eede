using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectionPreviewState : ISelectionState
{
    private readonly SelectionPreviewInfo _info;
    private readonly Picture _sourcePixels;

    public SelectionPreviewState(SelectionPreviewInfo info, Picture sourcePixels = null)
    {
        _info = info;
        _sourcePixels = sourcePixels ?? info.Pixels;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction)
    {
        var currentArea = new PictureArea(_info.Position, _info.Pixels.Size);

        var handle = SelectionHandleDetector.Detect(currentArea, mousePosition, 6);
        if (handle.HasValue)
        {
            return new ResizingState(_sourcePixels, currentArea, mousePosition, handle.Value, new NearestNeighborResampler());
        }

        if (Contains(currentArea, mousePosition))
        {
            return new DraggingState(_info.Pixels, currentArea, mousePosition, _info.Type, _info.OriginalArea);
        }

        return new NormalCursorState(cursorArea);
    }

    private bool Contains(PictureArea area, Position position)
    {
        return position.X >= area.X && position.X < area.X + area.Width &&
               position.Y >= area.Y && position.Y < area.Y + area.Height;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
    {
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, bool isShift, PictureSize canvasSize)
    {
        return (visibleCursor, cursorArea.Move(nowPosition));
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        return (this, cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return _info;
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        var currentArea = new PictureArea(_info.Position, _info.Pixels.Size);

        var handle = SelectionHandleDetector.Detect(currentArea, mousePosition, 6);
        if (handle.HasValue)
        {
            return GetCursorForHandle(handle.Value);
        }

        return Contains(currentArea, mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return new PictureArea(_info.Position, _info.Pixels.Size);
    }

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        var info = _info;
        if (blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
        {
            info = new SelectionPreviewInfo(
                info.Pixels.ApplyTransparency(backgroundColor),
                info.Position,
                info.Type,
                info.OriginalArea);
        }
        return session.UpdatePreviewContent(info).CommitPreview(blender);
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session.UpdatePreviewContent(_info).CancelDrawing();
    }

    private SelectionCursor GetCursorForHandle(SelectionHandle handle)
    {
        return handle switch
        {
            SelectionHandle.TopLeft => SelectionCursor.SizeNWSE,
            SelectionHandle.BottomRight => SelectionCursor.SizeNWSE,
            SelectionHandle.TopRight => SelectionCursor.SizeNESW,
            SelectionHandle.BottomLeft => SelectionCursor.SizeNESW,
            SelectionHandle.Top => SelectionCursor.SizeNS,
            SelectionHandle.Bottom => SelectionCursor.SizeNS,
            SelectionHandle.Left => SelectionCursor.SizeWE,
            SelectionHandle.Right => SelectionCursor.SizeWE,
            _ => SelectionCursor.Default
        };
    }
}