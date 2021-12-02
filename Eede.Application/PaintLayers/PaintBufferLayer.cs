using Eede.Domain.ImageTransfers;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application.PaintLayers
{
    public
    class PaintBufferLayer : IPaintLayer
    {
        private readonly MagnifiedSize PaintSize;
        private readonly AlphaPicture Source;
        private readonly IImageTransfer ImageTransfer;

        public PaintBufferLayer(MagnifiedSize paintSize, AlphaPicture source, IImageTransfer imageTransfer)
        {
            PaintSize = paintSize;
            Source = source;
            ImageTransfer = imageTransfer;
        }

        public void Paint(Graphics destination)
        {
            if (Source == null) return;
            ImageTransfer.Transfer(Source.Bmp, destination, PaintSize.ToSize());
        }
    }
}