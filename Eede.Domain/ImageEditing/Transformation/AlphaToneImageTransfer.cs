using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Transformation;

public class AlphaToneImageTransfer : IImageTransfer
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
            byte alpha = srcSpan[i + 3];
            destPixels[i + 0] = alpha;
            destPixels[i + 1] = alpha;
            destPixels[i + 2] = alpha;
            destPixels[i + 3] = 255;
        }
        return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
    }
}