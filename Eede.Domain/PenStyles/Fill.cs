using Eede.Domain.Positions;

namespace Eede.PenStyles
{
    // 実装内容を再検討する
    public class Fill //: IPenStyle
    {
        public void DrawBegin(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
            aBitmap.Fill(positions.Now, pen.PreparePen().Color);
        }

        public void Drawing(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
        }

        public void DrawEnd(AlphaPicture aBitmap, PenCase pen, PositionHistory positions, bool isShift)
        {
        }
    }
}