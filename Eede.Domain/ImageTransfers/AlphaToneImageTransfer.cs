using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    public class AlphaToneImageTransfer : IImageTransfer
    {
        public PictureData Transfer(Bitmap from, MagnifiedSize size)
        {
            var src = PictureData.CreateBuffer(from);

            var srcPixels = src;
            var toStride = size.Magnification.Magnify(src.Stride);
            byte[] destPixels = new byte[toStride * size.Height];
            int destWidth = size.Width;
            int destHeight = size.Height;

            for (int y = 0; y < destHeight; y++)
            {
                int srcOffset = src.Stride * size.Magnification.Minify(y);
                int destOffset = y * toStride;

                for (int x = 0; x < destWidth; x++)
                {
                    int pos = x * 4 + destOffset;
                    int srcX = size.Magnification.Minify(x);
                    int fromPos = srcX * 4 + srcOffset;
                    var alpha = srcPixels[fromPos + 3];

                    destPixels[pos + 0] = alpha;
                    destPixels[pos + 1] = alpha;
                    destPixels[pos + 2] = alpha;
                    destPixels[pos + 3] = 255;
                }
            }
            return PictureData.Create(new PictureSize(size.Width, size.Height), destPixels);
            //PictureDataをreturnするようにしたい
            //using var dest = PictureData.CreateBitmap(PictureData.Create(new PictureSize(size.Width, size.Height), destPixels));
            //to.PixelOffsetMode = PixelOffsetMode.Half;
            //to.InterpolationMode = InterpolationMode.NearestNeighbor;
            //to.DrawImage(dest, new Rectangle(0, 0, size.Width, size.Height));
        }
    }
}