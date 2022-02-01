using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application.PaintLayers
{
    public
    class PaintBufferLayer : IPaintLayer
    {
        private readonly MagnifiedSize PaintSize;
        private readonly Picture Source;
        private readonly IImageTransfer ImageTransfer;

        public PaintBufferLayer(MagnifiedSize paintSize, Picture source, IImageTransfer imageTransfer)
        {
            PaintSize = paintSize;
            Source = source;
            ImageTransfer = imageTransfer;
        }

        public void Paint(Graphics destination)
        {
            if (Source == null) return;
            Source.Transfer(ImageTransfer, destination, PaintSize.ToSize());
        }
    }
}