using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Transformation;

public class AlphaToneImageTransfer : IImageTransfer
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
                byte alpha = src.AsSpan()[fromPos + 3];

                destPixels[pos + 0] = alpha;
                destPixels[pos + 1] = alpha;
                destPixels[pos + 2] = alpha;
                destPixels[pos + 3] = 255;
            }
        }
        return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
    }
}