using Eede.Domain.PaintLayers;
using System.Drawing;

namespace Eede.Services
{
    internal class CanvasBackgroundService : ICanvasBackgroundService
    {
        public static CanvasBackgroundService Instance { get; } = new CanvasBackgroundService();

        private CanvasBackgroundService()
        {
        }

        public void PaintBackground(Graphics g)
        {
            using (var tb = new TextureBrush(Properties.Resources.InvisibleBackground))
            {
                g.FillRectangle(tb, g.VisibleClipBounds);
            }
        }
    }
}