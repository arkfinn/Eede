using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using System;
using System.Windows.Input;

namespace Eede.Domain.ImageEditing.SelectionStates;

public class SelectionPreviewState : ISelectionState
{
    private readonly SelectionPreviewInfo _info;

    public SelectionPreviewState(SelectionPreviewInfo info)
    {
        _info = info;
    }

    public ISelectionState HandlePointerLeftButtonPressed(HalfBoxArea cursorArea, Position mousePosition, ICommand? pullAction, Func<Picture> getPicture, ICommand? updateAction)
    {
        // プレビュー範囲内をクリックしたら再ドラッグ開始
        var currentArea = new PictureArea(_info.Position, _info.Pixels.Size);
        if (Contains(currentArea, mousePosition))
        {
            return new DraggingState(_info.Pixels, currentArea, mousePosition, _info.Type);
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
        return _info;
    }

    public SelectionCursor GetCursor(Position mousePosition)
    {
        var currentArea = new PictureArea(_info.Position, _info.Pixels.Size);
        return Contains(currentArea, mousePosition) ? SelectionCursor.Move : SelectionCursor.Default;
    }

    public PictureArea? GetSelectingArea()
    {
        return new PictureArea(_info.Position, _info.Pixels.Size);
    }

    public DrawingSession Commit(DrawingSession session, Eede.Domain.ImageEditing.Blending.IImageBlender blender)
    {
        // 自分が持っている最新の情報をセッションに反映してから確定する
        return session.UpdatePreviewContent(_info).CommitPreview(blender);
    }

    public DrawingSession Cancel(DrawingSession session)
    {
        // プレビュー状態を反映した上でキャンセル（破棄）を実行する
        return session.UpdatePreviewContent(_info).CancelDrawing();
    }
}
