using Eede.Application.PaintLayers;
using System.Drawing;

namespace Eede.Presentation.Services
{
    internal class CanvasBackgroundService : ICanvasBackgroundService
    {
        public static CanvasBackgroundService Instance { get; } = new CanvasBackgroundService();

        private CanvasBackgroundService()
        {
        }

        public void PaintBackground(Graphics g)
        {
            //using TextureBrush tb = new(Properties.Resources.InvisibleBackground);
            //g.FillRectangle(tb, g.VisibleClipBounds);
        }
    }
}