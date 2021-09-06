using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.ImageTransfers
{
    public class RGBToneImageTransfer : IImageTransfer
    {
        public void Transfer(Bitmap from, Graphics to, Size size)
        {
            var dest = new Bitmap(from.Width, from.Height);
            try
            {
                BitmapData destBitmapData = dest.LockBits(
                    new Rectangle(Point.Empty, dest.Size),
                    ImageLockMode.WriteOnly, dest.PixelFormat);
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

                        for (int y = 0; y < destBitmapData.Height; y++)
                        {
                            for (int x = 0; x < destBitmapData.Width; x++)
                            {
                                int pos = x * 4 + destBitmapData.Stride * y;
                                destPixels[pos + 0] = srcPixels[pos + 0];
                                destPixels[pos + 1] = srcPixels[pos + 1];
                                destPixels[pos + 2] = srcPixels[pos + 2];
                                destPixels[pos + 3] = 255;
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
                    dest.UnlockBits(destBitmapData);
                }

                to.PixelOffsetMode = PixelOffsetMode.Half;
                to.InterpolationMode = InterpolationMode.NearestNeighbor;

                to.DrawImage(dest, new Rectangle(0, 0, size.Width, size.Height));
            }
            finally
            {
                dest.Dispose();
            }
        }
    }
}
