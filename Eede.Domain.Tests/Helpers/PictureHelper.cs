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
            using Bitmap bitmap = new(path);
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixels = new byte[width * 4 * height];
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int sourceStride = bmpData.Stride;
                IntPtr sourcePtr = bmpData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(sourcePtr + y * sourceStride, pixels, y * width * 4, width * 4);
                }
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
            return Picture.Create(new PictureSize(width, height), pixels);
        }

        public static void WriteBitmap(string path, Picture picture)
        {
            int width = picture.Width;
            int height = picture.Height;
            using Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                int destStride = bmpData.Stride;
                IntPtr destPtr = bmpData.Scan0;
                byte[] pixels = picture.CloneImage();
                for (int y = 0; y < height; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixels, y * width * 4, destPtr + y * destStride, width * 4);
                }
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
            bitmap.Save(path);
        }
    }
}
