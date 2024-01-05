using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Application.PaintLayers
{
    public class PaintGridLayer : IPaintLayer
    {
        private readonly MagnifiedSize paintAreaSize;
        private readonly MagnifiedSize gridSize;

        public PaintGridLayer(MagnifiedSize paintAreaSize, MagnifiedSize gridSize)
        {
            this.paintAreaSize = paintAreaSize;
            this.gridSize = gridSize;
        }

        public void Paint(Graphics destination)
        {
            int w = gridSize.Width;
            int h = gridSize.Height;
            Pen p = new(Color.Black, 1)
            {
                DashStyle = DashStyle.Dash
            };
            for (int i = w; i < paintAreaSize.Width; i += w)
            {
                destination.DrawLine(p, i, 0, i, paintAreaSize.Height);
            }
            for (int i = h; i < paintAreaSize.Height; i += h)
            {
                destination.DrawLine(p, 0, i, paintAreaSize.Width, i);
            }
        }

        public Picture Painted(Picture destination)
        {
            return destination;
        }
    }
}