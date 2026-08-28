#nullable enable
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing.Blending;

public class AlphaImageBlender : ImageBlenderBase
{
    protected override void ExecuteBlend(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromStride, int toStride, int startX, int startY, int maxX, int maxY, Position toPosition)
    {
        for (int y = startY; y < maxY; y++)
        {
            int toPos = (startX * 4) + (toStride * y);
            int fromPos = ((startX - toPosition.X) * 4) + (fromStride * (y - toPosition.Y));

            for (int x = startX; x < maxX; x++)
            {
                // 転送元がアルファ0なら転送しない
                byte fromA = fromSpan[fromPos + 3];
                if (fromA == 0)
                {
                    toPos += 4;
                    fromPos += 4;
                    continue;
                }

                // 転送先がアルファ0なら無条件で転送
                byte toA = toPixels[toPos + 3];
                if (toA == 0)
                {
                    toPixels[toPos + 0] = fromSpan[fromPos + 0];
                    toPixels[toPos + 1] = fromSpan[fromPos + 1];
                    toPixels[toPos + 2] = fromSpan[fromPos + 2];
                    toPixels[toPos + 3] = fromA;
                    toPos += 4;
                    fromPos += 4;
                    continue;
                }

                // それ以外の場合、アルファ値・カラーを合成する
                int exactOutA = fromA * 255 + toA * 255 - fromA * toA;
                toPixels[toPos + 3] = (byte)((exactOutA + 127) / 255);

                if (exactOutA == 0)
                {
                    toPos += 4;
                    fromPos += 4;
                    continue;
                }

                int fromA255 = fromA * 255;
                int toAFactor = toA * (255 - fromA);

                toPixels[toPos + 0] = (byte)((fromSpan[fromPos + 0] * fromA255 + toPixels[toPos + 0] * toAFactor + (exactOutA >> 1)) / exactOutA);
                toPixels[toPos + 1] = (byte)((fromSpan[fromPos + 1] * fromA255 + toPixels[toPos + 1] * toAFactor + (exactOutA >> 1)) / exactOutA);
                toPixels[toPos + 2] = (byte)((fromSpan[fromPos + 2] * fromA255 + toPixels[toPos + 2] * toAFactor + (exactOutA >> 1)) / exactOutA);

                toPos += 4;
                fromPos += 4;
            }
        }
    }
}
