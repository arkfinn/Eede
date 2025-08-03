using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Domain.ImageBlenders
{
    public class RGBOnlyImageBlender : IImageBlender
    {
        public Picture Blend(Picture from, Picture to)
        {
            return Blend(from, to, new Position(0, 0));
        }

        public Picture Blend(Picture from, Picture to, Position toPosition)
        {
            byte[] toPixels = to.CloneImage();

            int maxY = Math.Min(toPosition.Y + from.Height, to.Height);
            int maxX = Math.Min(toPosition.X + from.Width, to.Width);

            for (int y = toPosition.Y; y < maxY; y++)
            {
                for (int x = toPosition.X; x < maxX; x++)
                {
                    int toPos = (x * 4) + (to.Stride * y);
                    int fromPos = ((x - toPosition.X) * 4) + (from.Stride * (y - toPosition.Y));

                    ReadOnlySpan<byte> fromSpan = from.AsSpan();
                    toPixels[toPos + 0] = fromSpan[fromPos + 0];
                    toPixels[toPos + 1] = fromSpan[fromPos + 1];
                    toPixels[toPos + 2] = fromSpan[fromPos + 2];
                }
            }

            return Picture.Create(to.Size, toPixels);
        }
    }
}