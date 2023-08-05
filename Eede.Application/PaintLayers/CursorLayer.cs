using Eede.Domain.DrawStyles;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Sizes;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Application.PaintLayers
{
    public class CursorLayer : IPaintLayer
    {
        private readonly MagnifiedSize PaintSize;
        private readonly Picture Source;
        private readonly PenStyle PenStyle;
        private readonly Position Position;
        private readonly IImageTransfer ImageTransfer;

        public CursorLayer(MagnifiedSize paintSize, Picture source, PenStyle penStyle, Position position, IImageTransfer imageTransfer)
        {
            PaintSize = paintSize;
            Source = source;
            PenStyle = penStyle;
            Position = position;
            ImageTransfer = imageTransfer;
        }

        public void Paint(Graphics destination)
        {
            var drawer = new Drawer(Source, PenStyle);
            using var cursor = drawer.DrawPoint(Position);
            var data = cursor.Transfer(ImageTransfer, PaintSize.Magnification);
            using var dest = PictureData.CreateBitmap(data);
            destination.PixelOffsetMode = PixelOffsetMode.Half;
            destination.InterpolationMode = InterpolationMode.NearestNeighbor;
            destination.DrawImage(dest, new Point(0, 0));
        }
    }
}