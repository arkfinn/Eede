#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.Blending;

public abstract class ImageBlenderBase : IImageBlender
{
    public Picture Blend(Picture from, Picture to)
    {
        return Blend(from, to, new Position(0, 0));
    }

    public Picture Blend(Picture from, Picture to, Position toPosition)
    {
        byte[] toPixels = to.CloneImage();

        int startY = Math.Max(0, toPosition.Y);
        int startX = Math.Max(0, toPosition.X);
        int maxY = Math.Min(toPosition.Y + from.Height, to.Height);
        int maxX = Math.Min(toPosition.X + from.Width, to.Width);

        ReadOnlySpan<byte> fromSpan = from.AsSpan();

        ExecuteBlend(fromSpan, toPixels, from.Stride, to.Stride, startX, startY, maxX, maxY, toPosition);

        return Picture.Create(to.Size, toPixels);
    }

    protected abstract void ExecuteBlend(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromStride, int toStride, int startX, int startY, int maxX, int maxY, Position toPosition);
}
