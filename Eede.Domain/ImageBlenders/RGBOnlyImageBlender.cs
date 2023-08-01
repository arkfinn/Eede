using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.ImageBlenders
{
    public class RGBOnlyImageBlender : IImageBlender
    {
        public PictureData Blend(PictureData from, PictureData to)
        {
            return Blend(from, to, new Position(0, 0));
        }

        public PictureData Blend(PictureData from, PictureData to, Position toPosition)
        {
            var dest = to;
            var destPixels = dest.ImageData.Clone() as byte[];

            var src = from;

            var maxY = Math.Min(toPosition.Y + src.Height, dest.Height);
            var maxX = Math.Min(toPosition.X + src.Width, dest.Width);

            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    int pos = x * 4 + dest.Stride * y;
                    destPixels[pos + 0] = src.ImageData[pos + 0];
                    destPixels[pos + 1] = src.ImageData[pos + 1];
                    destPixels[pos + 2] = src.ImageData[pos + 2];
                }
            }

            return new PictureData(dest.Size, destPixels);
        }
    }
}