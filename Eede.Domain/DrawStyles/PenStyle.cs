using Eede.Domain.ImageBlenders;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Domain.DrawStyles
{
    // AlphaOnlyBrushみたいなので実現できるんじゃないの

    public class PenStyle
    {
        public IImageBlender Blender;
        public Color Color = Color.Black;
        public int Width = 1;

        public PenStyle(Pen pen, IImageBlender blender)
        {
            Blender = blender;
        }

        public Pen PreparePen()
        {
            return new Pen(Color)
            {
                Width = Width,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
        }
    }
}