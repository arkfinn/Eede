using Eede.Domain.Pictures;
using System.Drawing;

namespace Eede.Application.PaintLayers
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

        public Picture Painted(Picture destination)
        {
            return destination; 
        }
    }
}