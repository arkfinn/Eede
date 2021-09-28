﻿using Eede.Domain.ImageTransfers;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application.PaintLayers
{
    public
    class PaintBufferLayer : IPaintLayer
    {
        private readonly IPaintLayer UpperLayer;
        private readonly MagnifiedSize PaintSize;
        private readonly Bitmap Source;
        private readonly IImageTransfer ImageTransfer;

        public PaintBufferLayer(IPaintLayer upperLayer, MagnifiedSize paintSize, Bitmap source, IImageTransfer imageTransfer)
        {
            UpperLayer = upperLayer;
            PaintSize = paintSize;
            Source = source;
            ImageTransfer = imageTransfer;
        }

        public void Paint(Graphics destination)
        {
            if (UpperLayer != null) UpperLayer.Paint(destination);
            if (Source == null) return;
            ImageTransfer.Transfer(Source, destination, PaintSize.ToSize());
        }
    }
}