using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.Tests.Helpers
{
    internal class PictureHelper
    {
        public static Picture ReadBitmap(string path)
        {
            // .net8.0-windowsを指定してる限り当面windows依存とする
            Bitmap bitmap = new(path);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            byte[] bytes = new byte[Math.Abs(bmpData.Stride) * bitmap.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(bmpData);
            return Picture.Create(new PictureSize(bitmap.Width, bitmap.Height), bytes);
        }

        public static void WriteBitmap(string path, Picture picture)
        {
            // .net8.0-windowsを指定してる限り当面windows依存とする
            Bitmap bitmap = new(picture.Width, picture.Height);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            byte[] bytes = picture.CloneImage();
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
            bitmap.UnlockBits(bmpData);
            bitmap.Save(path);
        }
    }
}
