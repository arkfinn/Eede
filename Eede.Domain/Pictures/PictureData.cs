using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures
{
    public class PictureData
    {

        public static PictureData CreateBuffer(Bitmap bitmap)
        {
            // ソースのBitmapから必要な情報を取得します
            int width = bitmap.Width;
            int height = bitmap.Height;
            PixelFormat format = bitmap.PixelFormat;
            int stride = width * 4;

            // ソースのBitmapの画像データを取得します
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
            System.IntPtr scan0 = bmpData.Scan0;
            int dataSize = stride * height;
            byte[] imageData = new byte[dataSize];
            System.Runtime.InteropServices.Marshal.Copy(scan0, imageData, 0, dataSize);
            bitmap.UnlockBits(bmpData);
            return new PictureData(new PictureSize(width, height), imageData);
        }

        public static Bitmap CreateBitmap(PictureData data)
        {
            var stride = data.Width * 4;

            // IntPtrに画像データをマーシャリングします
            System.IntPtr scan0 = System.Runtime.InteropServices.Marshal.AllocHGlobal(data.Length);
            System.Runtime.InteropServices.Marshal.Copy(data.ImageData, 0, scan0, data.Length);
            try
            {
                return new Bitmap(data.Width, data.Height, stride, PixelFormat.Format32bppArgb, scan0);
            }
            finally
            {
                // メモリリークを防ぐため、IntPtrのメモリを解放します
                // System.Runtime.InteropServices.Marshal.FreeHGlobal(scan0);
            }
        }

        public readonly PictureSize Size;
        public readonly byte[] ImageData;
        public readonly int Stride;

        public PictureData(PictureSize size, byte[] imageData)
        {
            var stride = size.Width * 4;
            if (stride * size.Height != imageData.Length)
            {
                throw new ArgumentException($"(width:{size.Width}, height:{size.Height}) != length:{imageData.Length}");
            }
            Size = size;
            ImageData = imageData;
            Stride = stride;
        }

        public int Width => Size.Width;
        public int Height => Size.Height;
        public int Length => ImageData.Length;
    }
}
