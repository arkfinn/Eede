using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.ImageBlenders
{
    public class RGBOnlyImageBlender : IImageBlender
    {
        public void Blend(Bitmap from, Bitmap to)
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

                    for (int y = 0; y < destBitmapData.Height; y++)
                    {
                        for (int x = 0; x < destBitmapData.Width; x++)
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
