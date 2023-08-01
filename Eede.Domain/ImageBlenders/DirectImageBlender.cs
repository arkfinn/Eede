using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.ImageBlenders
{
    public class DirectImageBlender : IImageBlender
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

            for (int y = toPosition.Y; y < maxY; y++)
            {
                for (int x = toPosition.X; x < maxX; x++)
                {
                    int pos = x * 4 + dest.Stride * y;
                    int srcPos = (x - toPosition.X) * 4 + src.Stride * (y - toPosition.Y);
                    destPixels[pos + 0] = src.ImageData[srcPos + 0];
                    destPixels[pos + 1] = src.ImageData[srcPos + 1];
                    destPixels[pos + 2] = src.ImageData[srcPos + 2];
                    destPixels[pos + 3] = src.ImageData[srcPos + 3];
                }
            }
            return new PictureData(dest.Size, destPixels);
        }
    }
}