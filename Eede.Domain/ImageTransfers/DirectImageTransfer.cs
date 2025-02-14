using Eede.Domain.Pictures;
using Eede.Domain.Scales;

namespace Eede.Domain.ImageTransfers
{
    public class DirectImageTransfer : IImageTransfer
    {
        public Picture Transfer(Picture src, Magnification magnification)
        {
            int toStride = magnification.Magnify(src.Stride);
            int destWidth = magnification.Magnify(src.Width);
            int destHeight = magnification.Magnify(src.Height);
            byte[] destPixels = new byte[toStride * destHeight];

            for (int y = 0; y < destHeight; y++)
            {
                int srcOffset = src.Stride * magnification.Minify(y);
                int destOffset = y * toStride;

                for (int x = 0; x < destWidth; x++)
                {
                    int pos = (x * 4) + destOffset;
                    int srcX = magnification.Minify(x);
                    int fromPos = (srcX * 4) + srcOffset;

                    destPixels[pos + 0] = src[fromPos + 0];
                    destPixels[pos + 1] = src[fromPos + 1];
                    destPixels[pos + 2] = src[fromPos + 2];
                    destPixels[pos + 3] = src[fromPos + 3];
                }
            }
            return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
        }
    }
}
