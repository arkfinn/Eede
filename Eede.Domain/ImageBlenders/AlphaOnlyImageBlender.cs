using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Domain.ImageBlenders
{
    public class AlphaOnlyImageBlender : IImageBlender
    {
        public PictureData Blend(PictureData from, PictureData to)
        {
            return Blend(from, to, new Position(0, 0));
        }

        public PictureData Blend(PictureData from, PictureData to, Position toPosition)
        {
            var toPixels = to.ImageData.Clone() as byte[];

            var maxY = Math.Min(toPosition.Y + from.Height, to.Height);
            var maxX = Math.Min(toPosition.X + from.Width, to.Width);

            for (int y = toPosition.Y; y < maxY; y++)
            {
                for (int x = toPosition.X; x < maxX; x++)
                {
                    int toPos = x * 4 + to.Stride * y;
                    int fromPos = (x - toPosition.X) * 4 + from.Stride * (y - toPosition.Y);

                    toPixels[toPos + 3] = from.ImageData[fromPos + 3];
                }
            }
            return new PictureData(to.Size, toPixels);
        }
    }
}