using Eede.Domain.ImageTransfers;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application.PaintLayers
{
    public
    class PaintBufferLayer : IPaintLayer
    {
        private readonly MagnifiedSize PaintSize;
        private readonly Bitmap Source;
        private readonly IImageTransfer ImageTransfer;

        public PaintBufferLayer(MagnifiedSize paintSize, Bitmap source, IImageTransfer imageTransfer)
        {
            PaintSize = paintSize;
            Source = source;
            ImageTransfer = imageTransfer;
        }

        public void Paint(Graphics destination)
        {
            if (Source == null) return;
            ImageTransfer.Transfer(Source, destination, PaintSize.ToSize());
        }
    }
}