using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Services
{
    class CanvasBackgroundService : ICanvasBackgroundService
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
