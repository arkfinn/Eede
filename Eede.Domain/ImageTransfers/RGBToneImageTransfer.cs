using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Eede.Domain.ImageTransfers
{
    public class RGBToneImageTransfer : IImageTransfer
    {
        public void Transfer(Bitmap from, Graphics to, MagnifiedSize size)
        {
            var src = PictureData.CreateBuffer(from);
            using (var dest = new Bitmap(from.Width, from.Height))
            {
                BitmapData destBitmapData = dest.LockBits(
                    new Rectangle(Point.Empty, dest.Size),
                    ImageLockMode.WriteOnly, dest.PixelFormat);
                var srcPixels = src;
                try
                {
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
                    dest.UnlockBits(destBitmapData);
                }

                to.PixelOffsetMode = PixelOffsetMode.Half;
                to.InterpolationMode = InterpolationMode.NearestNeighbor;

                to.DrawImage(dest, new Rectangle(0, 0, size.Width, size.Height));
            }
        }
    }
}