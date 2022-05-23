using Eede.Domain.Positions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.ImageBlenders
{
    public class AlphaImageBlender : IImageBlender
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

                    for (int y = toPosition.Y; y < maxY; y++)
                    {
                        for (int x = toPosition.X; x < maxX; x++)
                        {
                            int pos = x * 4 + destBitmapData.Stride * y;
                            int srcPos = (x - toPosition.X) * 4 + srcBitmapData.Stride * (y - toPosition.Y);
                            // 転送元がアルファ0なら転送しない
                            if (srcPixels[srcPos + 3] == 0)
                            {
                                continue;
                            }
                            // 転送先がアルファ0なら無条件で転送
                            if (destPixels[pos + 3] == 0)
                            {
                                destPixels[pos + 0] = srcPixels[srcPos + 0];
                                destPixels[pos + 1] = srcPixels[srcPos + 1];
                                destPixels[pos + 2] = srcPixels[srcPos + 2];
                                destPixels[pos + 3] = srcPixels[srcPos + 3];
                                continue;
                            }
                            // それ以外の場合、アルファ値・カラーを合成する
                            decimal srcAlpha = Decimal.Divide(srcPixels[srcPos + 3], 255);
                            decimal destAlpha = Decimal.Divide(destPixels[pos + 3], 255);
                            decimal alpha = srcAlpha + destAlpha - (srcAlpha * destAlpha);
                            destPixels[pos + 3] = (byte)(Decimal.Add(Decimal.Multiply(alpha, 255), 0.5m));
                            if (alpha == 0)
                            {
                                continue;
                            }
                            destPixels[pos + 0] = (byte)(Decimal.Add(Decimal.Divide((destPixels[pos + 0] * destAlpha * (1 - srcAlpha)) + (srcPixels[srcPos + 0] * srcAlpha), alpha), 0.5m));
                            destPixels[pos + 1] = (byte)(Decimal.Add(Decimal.Divide((destPixels[pos + 1] * destAlpha * (1 - srcAlpha)) + (srcPixels[srcPos + 1] * srcAlpha), alpha), 0.5m));
                            destPixels[pos + 2] = (byte)(Decimal.Add(Decimal.Divide((destPixels[pos + 2] * destAlpha * (1 - srcAlpha)) + (srcPixels[srcPos + 2] * srcAlpha), alpha), 0.5m));
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