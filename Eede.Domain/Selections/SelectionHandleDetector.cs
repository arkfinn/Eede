using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.Selections
{
    public static class SelectionHandleDetector
    {
        public static SelectionHandle? Detect(PictureArea area, Position position, int handleSize)
        {
            // 領域が小さい場合、中央部分を空けるためにハンドルサイズを制限する
            int minDimension = Math.Min(area.Width, area.Height);
            int adjustedHandleSize = handleSize;
            
            // 少なくとも中央の 1/3 は移動用に空ける
            if (adjustedHandleSize * 2 > minDimension * 2 / 3) 
            {
                adjustedHandleSize = Math.Max(1, minDimension / 3);
            }

            // 四隅
            if (IsHit(position, area.X, area.Y, adjustedHandleSize)) return SelectionHandle.TopLeft;
            if (IsHit(position, area.X + area.Width, area.Y, adjustedHandleSize)) return SelectionHandle.TopRight;
            if (IsHit(position, area.X, area.Y + area.Height, adjustedHandleSize)) return SelectionHandle.BottomLeft;
            if (IsHit(position, area.X + area.Width, area.Y + area.Height, adjustedHandleSize)) return SelectionHandle.BottomRight;

            // 四辺
            if (IsHit(position, area.X + area.Width / 2, area.Y, adjustedHandleSize)) return SelectionHandle.Top;
            if (IsHit(position, area.X + area.Width / 2, area.Y + area.Height, adjustedHandleSize)) return SelectionHandle.Bottom;
            if (IsHit(position, area.X, area.Y + area.Height / 2, adjustedHandleSize)) return SelectionHandle.Left;
            if (IsHit(position, area.X + area.Width, area.Y + area.Height / 2, adjustedHandleSize)) return SelectionHandle.Right;

            return null;
        }

        private static bool IsHit(Position pos, int targetX, int targetY, int size)
        {
            int tolerance = size / 2;
            return pos.X >= targetX - tolerance && pos.X <= targetX + tolerance &&
                   pos.Y >= targetY - tolerance && pos.Y <= targetY + tolerance;
        }
    }
}
