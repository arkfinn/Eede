#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.Blending;

public class DirectImageBlender : IImageBlender
{
    public Picture Blend(Picture from, Picture to)
    {
        return Blend(from, to, new Position(0, 0));
    }

    public Picture Blend(Picture from, Picture to, Position toPosition)
    {
        byte[] toPixels = to.CloneImage();
        BlendInternal(from, to, toPosition, toPixels);
        return Picture.Create(to.Size, toPixels);
    }

    public Picture Blend(System.Collections.Generic.IEnumerable<(Picture Picture, Position Position)> sources, Picture destination)
    {
        byte[] toPixels = destination.CloneImage();
        foreach (var source in sources)
        {
            BlendInternal(source.Picture, destination, source.Position, toPixels);
        }
        return Picture.Create(destination.Size, toPixels);
    }

    private void BlendInternal(Picture from, Picture to, Position toPosition, byte[] toPixels)
    {
        int startY = Math.Max(0, toPosition.Y);
        int startX = Math.Max(0, toPosition.X);
        int maxY = Math.Min(toPosition.Y + from.Height, to.Height);
        int maxX = Math.Min(toPosition.X + from.Width, to.Width);

        ReadOnlySpan<byte> fromSpan = from.AsSpan();
        for (int y = startY; y < maxY; y++)
        {
            for (int x = startX; x < maxX; x++)
            {
                int toPos = (x * 4) + (to.Stride * y);
                int fromPos = ((x - toPosition.X) * 4) + (from.Stride * (y - toPosition.Y));
                toPixels[toPos + 0] = fromSpan[fromPos + 0];
                toPixels[toPos + 1] = fromSpan[fromPos + 1];
                toPixels[toPos + 2] = fromSpan[fromPos + 2];
                toPixels[toPos + 3] = fromSpan[fromPos + 3];
            }
        }
    }
}