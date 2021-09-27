using Eede.Domain.Positions;

namespace Eede.PenStyles
{
    public class FreeCurve : IPenStyle
    {
        public void DrawBegin(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
            aBitmap.DrawPoint(pen, positions.Now);
        }

        public void Drawing(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
            if (aBitmap.IsInnerBitmap(positions.Now))
            {
                aBitmap.DrawLine(pen, positions.Last, positions.Now);
            }
            else if (aBitmap.IsInnerBitmap(positions.Last))
            {
                aBitmap.DrawLine(pen, positions.Now, positions.Last);
            }
        }

        public void DrawEnd(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
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