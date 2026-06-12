#nullable enable
using System;

namespace Eede.Domain.ImageEditing.Blending;

public class AlphaOnlyImageBlender : ImageBlenderBase
{
    protected override void BlendInternal(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromPos, int toPos)
    {
        toPixels[toPos + 3] = fromSpan[fromPos + 3];
    }
}
