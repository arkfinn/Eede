using Eede.Domain.Positions;

namespace Eede.Domain.DrawStyles
{
    // 実装内容を再検討する
    public class Fill //: IPenStyle
    {
        public void DrawBegin(AlphaPicture aBitmap, PenStyle pen, PositionHistory positions, bool isShift)
        {
            aBitmap.Fill(positions.Now, pen.PreparePen().Color);
        }

        public void Drawing(AlphaPicture aBitmap, PenStyle pen, PositionHistory positions, bool isShift)
        {
        }

        public void DrawEnd(AlphaPicture aBitmap, PenStyle pen, PositionHistory positions, bool isShift)
        {
        }
    }
}