using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;
using System.Runtime.InteropServices;

namespace Eede.Presentation.Common.Adapters
{
    public class AvaloniaBitmapAdapter : IBitmapAdapter<Bitmap>
    {
        public Bitmap ConvertToBitmap(Picture picture)
        {
            byte[] src = picture.CloneImage();
            return CreateBitmapFromPixelData(src, picture.Width, picture.Height);
        }

        public Bitmap ConvertToPremultipliedBitmap(Picture picture)
        {
            byte[] src = picture.CloneImage();
            return CreatePremultipliedBitmapFromPixelData(src, picture.Width, picture.Height);
        }

        public Picture ConvertToPicture(Bitmap bitmap)
        {
            int width = bitmap.PixelSize.Width;
            int height = bitmap.PixelSize.Height;
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];

            if (bitmap is WriteableBitmap wb)
            {
                using (var lockBuffer = wb.Lock())
                {
                    Marshal.Copy(lockBuffer.Address, pixels, 0, pixels.Length);
                }
            }
            else
            {
                GCHandle pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try
                {
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    bitmap.CopyPixels(new PixelRect(0, 0, width, height), pointer, pixels.Length, stride);
                }
                finally
                {
                    pinnedArray.Free();
                }
            }
            return Picture.Create(new PictureSize(width, height), pixels);
        }

        private static WriteableBitmap CreateBitmapFromPixelData(byte[] rgbPixelData, int width, int height)
        {
            Vector dpi = new(96, 96);
            WriteableBitmap bitmap = new(new PixelSize(width, height), dpi, PixelFormat.Bgra8888);
            using (ILockedFramebuffer frameBuffer = bitmap.Lock())
            {
                Marshal.Copy(rgbPixelData, 0, frameBuffer.Address, rgbPixelData.Length);
            }
            return bitmap;
        }

        private static WriteableBitmap CreatePremultipliedBitmapFromPixelData(byte[] rgbPixelData, int width, int height)
        {
            Vector dpi = new(96, 96);
            WriteableBitmap bitmap = new(new PixelSize(width, height), dpi, PixelFormat.Bgra8888);
            using (ILockedFramebuffer frameBuffer = bitmap.Lock())
            {
                Marshal.Copy(rgbPixelData, 0, frameBuffer.Address, rgbPixelData.Length);
                unsafe
                {
                    Span<byte> pixels = new((void*)frameBuffer.Address, rgbPixelData.Length);
                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        byte a = pixels[i + 3];
                        double factor = a / 255.0;
                        pixels[i + 0] = (byte)Math.Round(pixels[i + 0] * factor);
                        pixels[i + 1] = (byte)Math.Round(pixels[i + 1] * factor);
                        pixels[i + 2] = (byte)Math.Round(pixels[i + 2] * factor);
                    }
                }
            }
            return bitmap;
        }
    }
}
