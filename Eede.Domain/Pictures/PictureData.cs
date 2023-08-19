using System;
using System.Drawing.Imaging;
using System.Drawing;
using Eede.Domain.Positions;

namespace Eede.Domain.Pictures
{
    public class PictureData
    {
        private const int COLOR_32BIT = 4;

        public static PictureData CreateBuffer(Bitmap bitmap)
        {
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            byte[] bytes = new byte[Math.Abs(bmpData.Stride) * bitmap.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(bmpData);
            return Create(new PictureSize(bitmap.Width, bitmap.Height), bytes);
        }

        public static Bitmap CreateBitmap(PictureData data)
        {
            var dst = new Bitmap(data.Width, data.Height, PixelFormat.Format32bppArgb);
            var bmpData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.ReadWrite, dst.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(data.ImageData, 0, bmpData.Scan0, data.Length);
            dst.UnlockBits(bmpData);
            return dst;
        }

        public static PictureData Create(PictureSize size, byte[] imageData)
        {
            var stride = size.Width * COLOR_32BIT;
            if (stride * size.Height != imageData.Length)
            {
                throw new ArgumentException($"(width:{size.Width}, height:{size.Height}) != length:{imageData.Length}");
            }

            return new PictureData(size, imageData, stride);
        }

        public static PictureData CreateEmpty(PictureSize size)
        {
            return Create(size, new byte[size.Width * COLOR_32BIT * size.Height]);
        }

        public readonly PictureSize Size;
        private readonly byte[] ImageData;
        public readonly int Stride;

        private PictureData(PictureSize size, byte[] imageData, int stride)
        {
            Size = size;
            ImageData = imageData;
            Stride = stride;
        }

        public int Width => Size.Width;
        public int Height => Size.Height;
        public int Length => ImageData.Length;

        public byte this[int i]
        {
            get { return ImageData[i]; }
        }

        public byte[] CloneImage()
        {
            return ImageData.Clone() as byte[];
        }

        public ArgbColor PickColor(Position pos)
        {
            var index = pos.X * COLOR_32BIT + this.Stride * pos.Y;
            return new ArgbColor(
                this[index + 3],
                this[index + 2],
                this[index + 1],
                this[index]);
        }

        public PictureData CutOut(int x, int y, int width, int height)
        {
            var destinationStride = width * COLOR_32BIT;
            var destinationX = x * COLOR_32BIT;
            byte[] cutImageData = new byte[destinationStride * height];

            for (int i = 0; i < height; i++)
            {
                int sourceStartIndex = destinationX + (y + i) * this.Stride;
                int destinationStartIndex = i * destinationStride;
                Array.Copy(ImageData, sourceStartIndex, cutImageData, destinationStartIndex, destinationStride);
            }

            return PictureData.Create(new PictureSize(width, height), cutImageData);
        }
    }
}
