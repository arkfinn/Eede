using Eede.Domain.SharedKernel;

namespace Eede.Domain.Selections
{
    public static class SelectionHandleDetector
    {
        public static SelectionHandle? Detect(PictureArea area, Position position, int handleSize)
        {
            // 四隅
            if (IsHit(position, area.X, area.Y, handleSize)) return SelectionHandle.TopLeft;
            if (IsHit(position, area.X + area.Width, area.Y, handleSize)) return SelectionHandle.TopRight;
            if (IsHit(position, area.X, area.Y + area.Height, handleSize)) return SelectionHandle.BottomLeft;
            if (IsHit(position, area.X + area.Width, area.Y + area.Height, handleSize)) return SelectionHandle.BottomRight;

            // 四辺
            if (IsHit(position, area.X + area.Width / 2, area.Y, handleSize)) return SelectionHandle.Top;
            if (IsHit(position, area.X + area.Width / 2, area.Y + area.Height, handleSize)) return SelectionHandle.Bottom;
            if (IsHit(position, area.X, area.Y + area.Height / 2, handleSize)) return SelectionHandle.Left;
            if (IsHit(position, area.X + area.Width, area.Y + area.Height / 2, handleSize)) return SelectionHandle.Right;

            return null;
        }

        private static bool IsHit(Position pos, int targetX, int targetY, int size)
        {
            int half = size / 2;
            return pos.X >= targetX - half && pos.X <= targetX + half &&
                   pos.Y >= targetY - half && pos.Y <= targetY + half;
        }
    }
}
