using System;

namespace Eede.Domain.Pictures.Actions
{
    internal class PictureAction
    {
        public byte[] Process(Picture src, Func<int, int> processX, Func<int, int> processY)
        {
            byte[] destPixels = new byte[src.Length];
            var height = src.Height;
            var width = src.Width;

            for (int y = 0; y < height; y++)
            {
                int baseY = src.Stride * y;
                int srcY = processY(y);
                int offsetY = src.Stride * srcY;

                for (int x = 0; x < width; x++)
                {
                    int pos = (x * 4) + baseY;
                    int offsetX = processX(x);
                    int fromPos = (offsetX * 4) + offsetY;

                    destPixels[pos + 0] = src[fromPos + 0];
                    destPixels[pos + 1] = src[fromPos + 1];
                    destPixels[pos + 2] = src[fromPos + 2];
                    destPixels[pos + 3] = src[fromPos + 3];
                }
            }
            return destPixels;
        }
    }
}
