using Eede.Domain.ImageBlenders;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede
{
    // AlphaOnlyBrushみたいなので実現できるんじゃないの

    public class PenCase
    {
        public IImageBlender Blender;
        public Color Color = Color.Black;
        public int Width = 1;

        public PenCase(Pen pen, IImageBlender blender)
        {
            Blender = blender;
        }

        public Pen PreparePen()
        {
            var p = new Pen(Color);
            p.Width = Width;
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;

            return p;
        }
    }
}