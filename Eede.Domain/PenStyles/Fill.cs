using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Eede.Positions;

namespace Eede.PenStyles
{
    public class Fill : IPenStyle
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
