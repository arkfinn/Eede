using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Domain.ImageBlenders
{
    public class AlphaImageBlender : IImageBlender
    {
        public PictureData Blend(PictureData from, PictureData to)
        {
            return Blend(from, to, new Position(0, 0));
        }

        public PictureData Blend(PictureData from, PictureData to, Position toPosition)
        {
            var toPixels = to.ImageData.Clone() as byte[];
            var fromPixels = from.ImageData;

            var maxY = Math.Min(toPosition.Y + from.Height, to.Height);
            var maxX = Math.Min(toPosition.X + from.Width, to.Width);

            for (int y = toPosition.Y; y < maxY; y++)
            {
                for (int x = toPosition.X; x < maxX; x++)
                {
                    int toPos = x * 4 + to.Stride * y;
                    int fromPos = (x - toPosition.X) * 4 + from.Stride * (y - toPosition.Y);

                    // 転送元がアルファ0なら転送しない
                    byte fromA = fromPixels[fromPos + 3];
                    if (fromA == 0)
                    {
                        continue;
                    }
                    // 転送先がアルファ0なら無条件で転送
                    byte toA = toPixels[toPos + 3];
                    if (toA == 0)
                    {
                        toPixels[toPos + 0] = fromPixels[fromPos + 0];
                        toPixels[toPos + 1] = fromPixels[fromPos + 1];
                        toPixels[toPos + 2] = fromPixels[fromPos + 2];
                        toPixels[toPos + 3] = fromA;
                        continue;
                    }
                    // それ以外の場合、アルファ値・カラーを合成する
                    decimal fromAlpha = Decimal.Divide(fromA, 255);
                    decimal toAlpha = Decimal.Divide(toA, 255);
                    decimal alpha = fromAlpha + toAlpha - (fromAlpha * toAlpha);
                    toPixels[toPos + 3] = (byte)(Decimal.Add(Decimal.Multiply(alpha, 255), 0.5m));
                    if (alpha == 0)
                    {
                        continue;
                    }
                    decimal blendedAlpha = toAlpha * (1 - fromAlpha);
                    toPixels[toPos + 0] = (byte)(Decimal.Add(Decimal.Divide((toPixels[toPos + 0] * blendedAlpha) + (fromPixels[fromPos + 0] * fromAlpha), alpha), 0.5m));
                    toPixels[toPos + 1] = (byte)(Decimal.Add(Decimal.Divide((toPixels[toPos + 1] * blendedAlpha) + (fromPixels[fromPos + 1] * fromAlpha), alpha), 0.5m));
                    toPixels[toPos + 2] = (byte)(Decimal.Add(Decimal.Divide((toPixels[toPos + 2] * blendedAlpha) + (fromPixels[fromPos + 2] * fromAlpha), alpha), 0.5m));
                }
            }

            return new PictureData(to.Size, toPixels);
        }
    }
}