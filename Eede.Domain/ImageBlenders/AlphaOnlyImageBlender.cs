using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Domain.ImageBlenders
{
    public class AlphaOnlyImageBlender : IImageBlender
    {
        public Picture Blend(Picture from, Picture to)
        {
            return Blend(from, to, new Position(0, 0));
        }

        public Picture Blend(Picture from, Picture to, Position toPosition)
        {
            var toPixels = to.CloneImage();

            var maxY = Math.Min(toPosition.Y + from.Height, to.Height);
            var maxX = Math.Min(toPosition.X + from.Width, to.Width);

            for (int y = toPosition.Y; y < maxY; y++)
            {
                for (int x = toPosition.X; x < maxX; x++)
                {
                    int toPos = x * 4 + to.Stride * y;
                    int fromPos = (x - toPosition.X) * 4 + from.Stride * (y - toPosition.Y);

                    toPixels[toPos + 3] = from[fromPos + 3];
                }
            }
            return Picture.Create(to.Size, toPixels);
        }
    }
}