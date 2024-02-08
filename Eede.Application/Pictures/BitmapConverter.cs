using Eede.Domain.Pictures;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Application.Pictures
{
    public class BitmapConverter
    {

        public static Bitmap Convert(Picture picture)
        {
            Bitmap dst = new(picture.Width, picture.Height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.ReadWrite, dst.PixelFormat);
            byte[] data = picture.CloneImage();
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            dst.UnlockBits(bmpData);
            return dst;
        }

        public static Picture ConvertBack(Bitmap bitmap)
        {
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            byte[] bytes = new byte[Math.Abs(bmpData.Stride) * bitmap.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(bmpData);
            return Picture.Create(new PictureSize(bitmap.Width, bitmap.Height), bytes);
        }

    }
}
