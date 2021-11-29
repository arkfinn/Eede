using Eede.Domain.Positions;

namespace Eede.Domain.PenStyles
{
    public class FreeCurve : IPenStyle
    {
        public AlphaPicture DrawStart(DrawingMaterial material, PositionHistory positionHistory, bool isShift)
        {
            return material.DrawingPicture.DrawPoint(material.PenCase, positionHistory.Now);
        }

        public AlphaPicture Drawing(DrawingMaterial material, PositionHistory positionHistory, bool isShift)
        {
            if (material.DrawingPicture.Contains(positionHistory.Now))
            {
                return material.DrawingPicture.DrawLine(material.PenCase, positionHistory.Last, positionHistory.Now);
            }
            if (material.DrawingPicture.Contains(positionHistory.Last))
            {
                return material.DrawingPicture.DrawLine(material.PenCase, positionHistory.Now, positionHistory.Last);
            }
            return material.DrawingPicture;
        }

        public AlphaPicture DrawEnd(DrawingMaterial material, PositionHistory positionHistory, bool isShift)
        {
            return material.DrawingPicture;
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