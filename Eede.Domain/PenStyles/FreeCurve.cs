using Eede.Domain.Positions;

namespace Eede.PenStyles
{
    public class FreeCurve : IPenStyle
    {
        public AlphaPicture DrawStart(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
            return aBitmap.DrawPoint(pen, positions.Now);
        }

        public AlphaPicture Drawing(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
            if (aBitmap.IsInnerBitmap(positions.Now))
            {
                return aBitmap.DrawLine(pen, positions.Last, positions.Now);
            }
            if (aBitmap.IsInnerBitmap(positions.Last))
            {
                return aBitmap.DrawLine(pen, positions.Now, positions.Last);
            }
            return aBitmap;
        }

        public AlphaPicture DrawEnd(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
            return aBitmap;
        }

        //private void DrawFreeCurve(PaintableBox target, Point beginPos, Point endPos)
        //{
        //    using (Graphics g = Graphics.FromImage(target.Buffer.Bmp))
        //    {
        //        g.DrawLine(target.Pen, beginPos, endPos);
        //                   }
        //}
    }
}