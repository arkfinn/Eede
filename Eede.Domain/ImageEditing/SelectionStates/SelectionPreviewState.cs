using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectionPreviewState : ISelectionState
{
    private readonly Picture _pixels;
    private readonly Position _position;

    public SelectionPreviewState(Picture pixels, Position position)
    {
        _pixels = pixels;
        _position = position;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction)
    {
        // プレビュー範囲内をクリックしたら再ドラッグ開始
        var currentArea = new PictureArea(_position, _pixels.Size);
        if (Contains(currentArea, mousePosition))
        {
            return new DraggingState(_pixels, currentArea, mousePosition);
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
        // 右クリックでキャンセル
        // Cancel は Coordinator から呼ばれる想定だが、ここで遷移先を返す必要もある。
        return (new NormalCursorState(cursorArea), cursorArea);
    }

    public (bool, HalfBoxArea) HandlePointerMoved(HalfBoxArea cursorArea, bool visibleCursor, Position nowPosition, PictureSize canvasSize)
    {
        return (canvasSize.Contains(nowPosition), cursorArea.Move(nowPosition));
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
        var currentArea = new PictureArea(_position, _pixels.Size);
        return Contains(currentArea, mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return new PictureArea(_position, _pixels.Size);
    }

    public DrawingSession Commit(DrawingSession session)
    {
        // プレビュー内容を確定する
        // 現在のバッファ（穴あき状態）をベースにプレビュー画像を合成
        var basePicture = session.CurrentPicture;
        var blended = basePicture.Blend(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), _pixels, _position);
        
        // プレビューを消して、新しい画像を履歴に追加
        return session.UpdatePreviewContent(null).Push(blended, GetSelectingArea());
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        // 一時的な「穴あき」状態をキャンセルし、プレビューも破棄する
        return session.CancelDrawing().UpdatePreviewContent(null);
    }
}
