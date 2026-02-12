#nullable enable
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates
{
    public class ResizingState : ISelectionState
    {
        private readonly Picture OriginalPixels;
        private readonly PictureArea OriginalArea;
        private readonly Position StartPosition;
        private readonly SelectionHandle Handle;
        private readonly IImageResampler Resampler;
        private readonly SelectionPreviewType Type;
        private readonly PictureArea? ClearArea;

        private Position NowPosition;

        public ResizingState(Picture originalPixels, PictureArea originalArea, Position startPosition, SelectionHandle handle, IImageResampler resampler, SelectionPreviewType type = SelectionPreviewType.CutAndMove, PictureArea? clearArea = null)
        {
            OriginalPixels = originalPixels;
            OriginalArea = originalArea;
            StartPosition = startPosition;
            NowPosition = startPosition;
            Handle = handle;
            Resampler = resampler;
            Type = type;
            ClearArea = clearArea ?? (type == SelectionPreviewType.CutAndMove ? originalArea : null);
        }

        public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction, int handleSize = 8)
        {
            return this;
        }

        public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
        {
            var info = GetSelectionPreviewInfo();
            if (info != null)
            {
                return new SelectionPreviewState(info, OriginalPixels);
            }
            return new NormalCursorState(cursorArea);
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
        {
            return (this, cursorArea);
        }

        private bool _isShifted;

        public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, bool isShift, PictureSize canvasSize)
        {
            NowPosition = nowPosition;
            _isShifted = isShift;
            return (visibleCursor, cursorArea.Move(nowPosition));
        }

        public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
        {
            return (this, cursorArea);
        }

        public SelectionPreviewInfo? GetSelectionPreviewInfo()
        {
            var resizer = new ResizingSelection(OriginalArea, Handle);
            var newArea = resizer.Resize(StartPosition, NowPosition, _isShifted);

            var newPixels = Resampler.Resize(OriginalPixels, newArea.Size);

            return new SelectionPreviewInfo(newPixels, newArea.Position, Type, ClearArea, OriginalPixels);
        }

        public SelectionCursor GetCursor(Position mousePosition, int handleSize = 8)
        {
            // 操作中は常にリサイズカーソルを表示すべき
            return GetCursorForHandle(Handle);
        }

        public PictureArea? GetSelectingArea()
        {
            var info = GetSelectionPreviewInfo();
            return info == null ? null : new PictureArea(info.Position, info.Pixels.Size);
        }

        public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
        {
            var info = GetSelectionPreviewInfo();
            if (info != null && blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
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
            return session.UpdatePreviewContent(GetSelectionPreviewInfo()).CancelDrawing();
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
}