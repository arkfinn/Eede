﻿using System.Drawing;

namespace Eede.Domain.PaintLayers
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