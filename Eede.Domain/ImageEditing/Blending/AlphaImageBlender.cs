#nullable enable
using System;

namespace Eede.Domain.ImageEditing.Blending;

public class AlphaImageBlender : ImageBlenderBase
{
    protected override void BlendInternal(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromPos, int toPos)
    {
        // 転送元がアルファ0なら転送しない
        byte fromA = fromSpan[fromPos + 3];
        if (fromA == 0)
        {
            return;
        }

        // 転送先がアルファ0なら無条件で転送
        byte toA = toPixels[toPos + 3];
        if (toA == 0)
        {
            toPixels[toPos + 0] = fromSpan[fromPos + 0];
            toPixels[toPos + 1] = fromSpan[fromPos + 1];
            toPixels[toPos + 2] = fromSpan[fromPos + 2];
            toPixels[toPos + 3] = fromA;
            return;
        }

        // それ以外の場合、アルファ値・カラーを合成する
        float fromAlpha = fromA / 255f;
        float toAlpha = toA / 255f;
        float alpha = fromAlpha + toAlpha - (fromAlpha * toAlpha);
        toPixels[toPos + 3] = (byte)(alpha * 255f + 0.5f);

        if (alpha == 0)
        {
            return;
        }

        float blendedAlpha = toAlpha * (1f - fromAlpha);
        toPixels[toPos + 0] = (byte)(((toPixels[toPos + 0] * blendedAlpha) + (fromSpan[fromPos + 0] * fromAlpha)) / alpha + 0.5f);
        toPixels[toPos + 1] = (byte)(((toPixels[toPos + 1] * blendedAlpha) + (fromSpan[fromPos + 1] * fromAlpha)) / alpha + 0.5f);
        toPixels[toPos + 2] = (byte)(((toPixels[toPos + 2] * blendedAlpha) + (fromSpan[fromPos + 2] * fromAlpha)) / alpha + 0.5f);
    }
}
