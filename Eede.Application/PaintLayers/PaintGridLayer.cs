using Eede.Domain.Sizes;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Application.PaintLayers
{
    public class PaintGridLayer : IPaintLayer
    {
        private readonly IPaintLayer layer;
        private readonly MagnifiedSize paintAreaSize;
        private readonly MagnifiedSize gridSize;

        public PaintGridLayer(IPaintLayer layer, MagnifiedSize paintAreaSize, MagnifiedSize gridSize)
        {
            this.layer = layer;
            this.paintAreaSize = paintAreaSize;
            this.gridSize = gridSize;
        }

        public void Paint(Graphics destination)
        {
            if (layer != null) layer.Paint(destination);
            var w = gridSize.Width;
            var h = gridSize.Height;
            var p = new Pen(Color.Black, 1);
            p.DashStyle = DashStyle.Dash;
            for (int i = w; i < paintAreaSize.Width; i = i + w)
            {
                destination.DrawLine(p, i, 0, i, paintAreaSize.Height);
            }
            for (int i = h; i < paintAreaSize.Height; i = i + h)
            {
                destination.DrawLine(p, 0, i, paintAreaSize.Width, i);
            }
        }
    }
}