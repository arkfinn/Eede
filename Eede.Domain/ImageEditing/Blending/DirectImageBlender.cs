#nullable enable
using System;

namespace Eede.Domain.ImageEditing.Blending;

public class DirectImageBlender : ImageBlenderBase
{
    protected override void BlendInternal(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromPos, int toPos)
    {
        toPixels[toPos + 0] = fromSpan[fromPos + 0];
        toPixels[toPos + 1] = fromSpan[fromPos + 1];
        toPixels[toPos + 2] = fromSpan[fromPos + 2];
        toPixels[toPos + 3] = fromSpan[fromPos + 3];
    }
}
