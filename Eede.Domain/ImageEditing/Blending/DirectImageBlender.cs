#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.Blending;

public class DirectImageBlender : ImageBlenderBase
{
    protected override void ExecuteBlend(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromStride, int toStride, int startX, int startY, int maxX, int maxY, Position toPosition)
    {
        for (int y = startY; y < maxY; y++)
        {
            int toPos = (startX * 4) + (toStride * y);
            int fromPos = ((startX - toPosition.X) * 4) + (fromStride * (y - toPosition.Y));

            for (int x = startX; x < maxX; x++)
            {
                toPixels[toPos + 0] = fromSpan[fromPos + 0];
                toPixels[toPos + 1] = fromSpan[fromPos + 1];
                toPixels[toPos + 2] = fromSpan[fromPos + 2];
                toPixels[toPos + 3] = fromSpan[fromPos + 3];
                toPos += 4;
                fromPos += 4;
            }
        }
    }
}
