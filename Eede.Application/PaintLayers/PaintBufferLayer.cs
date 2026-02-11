using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;

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

        public Picture Painted(Picture destination)
        {
            return Source == null ? destination : Source.Transfer(ImageTransfer, new Magnification(1));
        }
    }
}
