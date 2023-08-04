using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    public class RGBToneImageTransfer : IImageTransfer
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

                    destPixels[pos + 0] = srcPixels[fromPos + 0];
                    destPixels[pos + 1] = srcPixels[fromPos + 1];
                    destPixels[pos + 2] = srcPixels[fromPos + 2];
                    destPixels[pos + 3] = 255;
                }
            }
            return PictureData.Create(new PictureSize(size.Width, size.Height), destPixels);
        }
    }
}