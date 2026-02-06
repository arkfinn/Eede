using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Domain.Selections;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class RegionSelectingState : ISelectionState
{
    private Position _startPosition;
    private Position _nowPosition;
    private PictureSize _minSize;
    private bool _isShifted;

    public RegionSelectingState(Position startPosition, Position nowPosition, PictureSize minSize)
    {
        _startPosition = startPosition;
        _nowPosition = nowPosition;
        _minSize = minSize;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction, int handleSize = 8)
    {
        return this;
    }

    public ISelectionState HandlePointerLeftButtonReleased(HalfBoxArea cursorArea, Position mousePosition, ICommand? picturePushAction, ICommand? pictureUpdateAction)
    {
        return this;
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonPressed(HalfBoxArea cursorArea, Position nowPosition, PictureSize minCursorSize, ICommand? pictureUpdateAction)
    {
        return (this, cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, bool isShift, PictureSize canvasSize)
    {
        _nowPosition = nowPosition;
        _isShifted = isShift;
        bool newVisibleCursor = canvasSize.Contains(nowPosition);
        HalfBoxArea newCursorArea = cursorArea.Move(nowPosition);
        return (newVisibleCursor, newCursorArea);
    }

    public (ISelectionState, HalfBoxArea) HandlePointerRightButtonReleased(HalfBoxArea cursorArea, ICommand? picturePushAction)
    {
        var selectingArea = GetSelectingArea();
        if (selectingArea.HasValue)
        {
            // 自由範囲選択が成立している場合
            return (new SelectedState(new Selection(selectingArea.Value)), cursorArea);
        }
        // ドラッグ距離が短く、クリックとみなされる場合
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public SelectionPreviewInfo? GetSelectionPreviewInfo()
    {
        return null;
    }

    public SelectionCursor GetCursor(Position mousePosition, int handleSize = 8)
    {
        return SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        Position targetPosition = _nowPosition;
        if (_isShifted)
        {
            int deltaX = _nowPosition.X - _startPosition.X;
            int deltaY = _nowPosition.Y - _startPosition.Y;
            int size = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));
            targetPosition = new Position(
                _startPosition.X + (deltaX >= 0 ? size : -size),
                _startPosition.Y + (deltaY >= 0 ? size : -size));
        }

        // マウスの生座標から暫定的な矩形を作成
        var rawArea = PictureArea.FromPosition(_startPosition, targetPosition, new PictureSize(int.MaxValue, int.MaxValue));

        // グリッドサイズ（スナップ単位）を算出（通常は 16x16）
        int gridSizeW = _minSize.Width / 2;
        int gridSizeH = _minSize.Height / 2;

        // 矩形の各端点をグリッドにスナップさせる
        int left = Snap(rawArea.X, gridSizeW);
        int top = Snap(rawArea.Y, gridSizeH);
        // 右端と下端は、選択範囲を切り上げる形でスナップさせる
        int right = Snap(rawArea.X + rawArea.Width + gridSizeW - 1, gridSizeW);
        int bottom = Snap(rawArea.Y + rawArea.Height + gridSizeH - 1, gridSizeH);

        // 最小サイズを確保しつつ、スナップ後のサイズを決定
        int width = Math.Max(_minSize.Width, right - left);
        int height = Math.Max(_minSize.Height, bottom - top);

        return new PictureArea(new Position(left, top), new PictureSize(width, height));
    }

    private int Snap(int value, int gridSize)
    {
        if (gridSize <= 0) return value;
        int remainder = value % gridSize;
        return value - remainder;
    }

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender, Eede.Domain.Palettes.ArgbColor backgroundColor)
    {
        return session;
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        return session;
    }
}
