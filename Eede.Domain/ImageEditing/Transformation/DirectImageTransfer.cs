using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Transformation;

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

                System.ReadOnlySpan<byte> srcSpan = src.AsSpan();
                destPixels[pos + 0] = srcSpan[fromPos + 0];
                destPixels[pos + 1] = srcSpan[fromPos + 1];
                destPixels[pos + 2] = srcSpan[fromPos + 2];
                destPixels[pos + 3] = srcSpan[fromPos + 3];
            }
        }
        return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
    }
}
