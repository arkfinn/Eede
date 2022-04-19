﻿using Eede.Domain.Positions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.ImageBlenders
{
    public class RGBOnlyImageBlender : IImageBlender
    {
        public void Blend(Bitmap from, Bitmap to)
        {
            Blend(from, to, new Position(0, 0));
        }

        public void Blend(Bitmap from, Bitmap to, Position toPosition)
        {
            BitmapData destBitmapData = to.LockBits(
                    new Rectangle(Point.Empty, to.Size),
                    ImageLockMode.WriteOnly, to.PixelFormat);
            try
            {
                BitmapData srcBitmapData = from.LockBits(
                        new Rectangle(Point.Empty, from.Size),
                        ImageLockMode.WriteOnly, from.PixelFormat);
                try
                {
                    byte[] srcPixels = new byte[srcBitmapData.Stride * from.Height];
                    System.Runtime.InteropServices.Marshal.Copy(srcBitmapData.Scan0, srcPixels, 0, srcPixels.Length);

                    byte[] destPixels = new byte[destBitmapData.Stride * destBitmapData.Height];
                    System.Runtime.InteropServices.Marshal.Copy(destBitmapData.Scan0, destPixels, 0, destPixels.Length);

                    var maxY = Math.Min(toPosition.Y + from.Height, destBitmapData.Height);
                    var maxX = Math.Min(toPosition.X + from.Width, destBitmapData.Width);

                    for (int y = 0; y < maxY; y++)
                    {
                        for (int x = 0; x < maxX; x++)
                        {
                            int pos = x * 4 + destBitmapData.Stride * y;
                            destPixels[pos + 0] = srcPixels[pos + 0];
                            destPixels[pos + 1] = srcPixels[pos + 1];
                            destPixels[pos + 2] = srcPixels[pos + 2];
                        }
                    }
                    System.Runtime.InteropServices.Marshal.Copy(destPixels, 0, destBitmapData.Scan0, destPixels.Length);
                }
                finally
                {
                    from.UnlockBits(srcBitmapData);
                }
            }
            finally
            {
                to.UnlockBits(destBitmapData);
            }
        }
    }
}