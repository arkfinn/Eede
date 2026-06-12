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

        for (int y = startY; y < maxY; y++)
        {
            int toPos = (startX * 4) + (to.Stride * y);
            int fromPos = ((startX - toPosition.X) * 4) + (from.Stride * (y - toPosition.Y));

            for (int x = startX; x < maxX; x++)
            {
                BlendInternal(fromSpan, toPixels, fromPos, toPos);
                toPos += 4;
                fromPos += 4;
            }
        }
        return Picture.Create(to.Size, toPixels);
    }

    protected abstract void BlendInternal(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromPos, int toPos);
}
