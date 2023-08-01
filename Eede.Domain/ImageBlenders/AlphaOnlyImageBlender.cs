using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

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
            var dest = to;
            var destPixels = dest.ImageData.Clone() as byte[];

            var src = from;
            // 変換対象のカラー画像の情報をバイト列へ書き出す

            var maxY = Math.Min(toPosition.Y + src.Height, dest.Height);
            var maxX = Math.Min(toPosition.X + src.Width, dest.Width);

            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    int pos = x * 4 + dest.Stride * y;
                    destPixels[pos + 3] = src.ImageData[pos + 3];
                }
            }
            return new PictureData(dest.Size, destPixels);
        }
    }
}