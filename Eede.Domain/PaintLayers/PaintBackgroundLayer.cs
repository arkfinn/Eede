using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.PaintLayers
{

    public class PaintBackgroundLayer : IPaintLayer
    {

        private readonly ICanvasBackgroundService Background;

        public PaintBackgroundLayer(ICanvasBackgroundService background)
        {
            Background = background;
        }

        public void Paint(Graphics destination)
        {
            Background.PaintBackground(destination);
        }
    }
}
