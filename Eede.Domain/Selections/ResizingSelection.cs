using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.Selections;

public class ResizingSelection
{
    private readonly PictureArea OriginalArea;
    private readonly SelectionHandle Handle;

    public ResizingSelection(PictureArea originalArea, SelectionHandle handle)
    {
        OriginalArea = originalArea;
        Handle = handle;
    }

    public PictureArea Resize(Position startPosition, Position currentPosition, bool keepAspectRatio)
    {
        int dx = currentPosition.X - startPosition.X;
        int dy = currentPosition.Y - startPosition.Y;

        // 1. 各ハンドルごとの「変形前の矩形の座標」を取得
        int left = OriginalArea.X;
        int top = OriginalArea.Y;
        int right = OriginalArea.X + OriginalArea.Width;
        int bottom = OriginalArea.Y + OriginalArea.Height;

        // 2. ドラッグ量に応じて座標を更新
        switch (Handle)
        {
            case SelectionHandle.TopLeft:
                left += dx; top += dy; break;
            case SelectionHandle.Top:
                top += dy; break;
            case SelectionHandle.TopRight:
                right += dx; top += dy; break;
            case SelectionHandle.Right:
                right += dx; break;
            case SelectionHandle.BottomRight:
                right += dx; bottom += dy; break;
            case SelectionHandle.Bottom:
                bottom += dy; break;
            case SelectionHandle.BottomLeft:
                left += dx; bottom += dy; break;
            case SelectionHandle.Left:
                left += dx; break;
        }

        // 3. 幅と高さの計算
        int newWidth = right - left;
        int newHeight = bottom - top;

        // 4. 反転（負の値）防止 & 最小サイズ制約
        if (newWidth < 1)
        {
            newWidth = 1;
            // 左側を動かしていたなら left を修正、右側なら right を修正
            if (Handle == SelectionHandle.TopLeft || Handle == SelectionHandle.Left || Handle == SelectionHandle.BottomLeft)
            {
                left = right - 1;
            }
            else
            {
                right = left + 1;
            }
        }
        if (newHeight < 1)
        {
            newHeight = 1;
            if (Handle == SelectionHandle.TopLeft || Handle == SelectionHandle.Top || Handle == SelectionHandle.TopRight)
            {
                top = bottom - 1;
            }
            else
            {
                bottom = top + 1;
            }
        }

        // 5. アスペクト比維持
        if (keepAspectRatio && IsCornerHandle(Handle))
        {
            double originalRatio = (double)OriginalArea.Width / OriginalArea.Height;
            double currentRatio = (double)newWidth / newHeight;

            if (currentRatio > originalRatio)
            {
                // 横長になりすぎている -> 高さを増やす (大きい辺に合わせる)
                newHeight = (int)Math.Round(newWidth / originalRatio);
            }
            else
            {
                // 縦長になりすぎている -> 幅を増やす
                newWidth = (int)Math.Round(newHeight * originalRatio);
            }

            // 調整後のサイズに合わせて座標を再調整 (Anchorは動かさない)
            if (Handle == SelectionHandle.TopLeft)
            {
                left = right - newWidth;
                top = bottom - newHeight;
            }
            else if (Handle == SelectionHandle.TopRight)
            {
                right = left + newWidth;
                top = bottom - newHeight;
            }
            else if (Handle == SelectionHandle.BottomLeft)
            {
                left = right - newWidth;
                bottom = top + newHeight;
            }
            else if (Handle == SelectionHandle.BottomRight)
            {
                right = left + newWidth;
                bottom = top + newHeight;
            }
        }

        return new PictureArea(new Position(left, top), new PictureSize(newWidth, newHeight));
    }

    private bool IsCornerHandle(SelectionHandle handle)
    {
        return handle == SelectionHandle.TopLeft ||
               handle == SelectionHandle.TopRight ||
               handle == SelectionHandle.BottomLeft ||
               handle == SelectionHandle.BottomRight;
    }
}
