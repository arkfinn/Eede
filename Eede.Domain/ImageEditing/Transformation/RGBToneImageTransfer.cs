using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Transformation;

public class RGBToneImageTransfer : IImageTransfer
{
    public Picture Transfer(Picture src, Magnification magnification)
    {
        // GPUスケーリング移行のため、ドメイン層での拡大は行わない。引数の magnification は無視して等倍で処理する。
        int toStride = src.Stride;
        int destWidth = src.Width;
        int destHeight = src.Height;
        byte[] destPixels = new byte[toStride * destHeight];

        System.ReadOnlySpan<byte> srcSpan = src.AsSpan();
        for (int i = 0; i < srcSpan.Length; i += 4)
        {
            destPixels[i + 0] = srcSpan[i + 0];
            destPixels[i + 1] = srcSpan[i + 1];
            destPixels[i + 2] = srcSpan[i + 2];
            destPixels[i + 3] = 255;
        }
        return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
    }
}