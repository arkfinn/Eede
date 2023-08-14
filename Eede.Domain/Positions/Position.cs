using System.Drawing;

namespace Eede.Domain.Positions
{
    public class Position
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            var position = obj as Position;
            return position != null &&
                   X == position.X &&
                   Y == position.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public bool IsInnerOf(Size size)
        {
            if (X < 0) return false;
            if (Y < 0) return false;
            if (X >= size.Width) return false;
            if (Y >= size.Height) return false;
            return true;
        }
    }
}