using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Transformation;

public class RGBToneImageTransfer : IImageTransfer
{
    public Picture Transfer(Picture src, Magnification magnification)
    {
        float factor = magnification.Value;
        int destWidth = magnification.Magnify(src.Width);
        int destHeight = magnification.Magnify(src.Height);
        int toStride = destWidth * 4;
        byte[] destPixels = new byte[toStride * destHeight];

        System.ReadOnlySpan<byte> srcSpan = src.AsSpan();
        int srcStride = src.Stride;

        for (int y = 0; y < destHeight; y++)
        {
            int srcY = (int)(y / factor);
            for (int x = 0; x < destWidth; x++)
            {
                int srcX = (int)(x / factor);
                int srcIdx = (srcY * srcStride) + (srcX * 4);
                int destIdx = (y * toStride) + (x * 4);

                destPixels[destIdx + 0] = srcSpan[srcIdx + 0];
                destPixels[destIdx + 1] = srcSpan[srcIdx + 1];
                destPixels[destIdx + 2] = srcSpan[srcIdx + 2];
                destPixels[destIdx + 3] = 255;
            }
        }
        return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
    }
}