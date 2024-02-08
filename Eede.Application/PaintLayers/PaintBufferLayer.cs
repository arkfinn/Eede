using Eede.Application.Pictures;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;
using System.Drawing.Drawing2D;

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
            if (Source == null)
            {
                return;
            }

            Picture data = Source.Transfer(ImageTransfer, PaintSize.Magnification);

            using Bitmap dest = BitmapConverter.Convert(data);
            destination.PixelOffsetMode = PixelOffsetMode.Half;
            destination.InterpolationMode = InterpolationMode.NearestNeighbor;
            destination.DrawImage(dest, new Point(0, 0));
        }

        public Picture Painted(Picture destination)
        {
            if (Source == null)
            {
                return destination;
            }
            return Source.Transfer(ImageTransfer, PaintSize.Magnification);
        }
    }
}