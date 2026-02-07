using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Transformation;

public class DirectImageTransfer : IImageTransfer
{
    public Picture Transfer(Picture src, Magnification magnification)
    {
        // GPUスケーリング移行のため、ドメイン層での拡大は行わない。引数の magnification は無視して等倍で処理する。
        int toStride = src.Stride;
        int destWidth = src.Width;
        int destHeight = src.Height;
        byte[] destPixels = new byte[toStride * destHeight];

        System.ReadOnlySpan<byte> srcSpan = src.AsSpan();
        srcSpan.CopyTo(destPixels);

        return Picture.Create(new PictureSize(destWidth, destHeight), destPixels);
    }
}
