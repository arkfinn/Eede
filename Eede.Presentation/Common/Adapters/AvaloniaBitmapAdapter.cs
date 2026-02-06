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
            return CreateBitmapFromPixelData(picture.AsSpan(), picture.Width, picture.Height);
        }

        public Bitmap ConvertToPremultipliedBitmap(Picture picture)
        {
            return CreatePremultipliedBitmapFromPixelData(picture.AsSpan(), picture.Width, picture.Height);
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

        private static WriteableBitmap CreateBitmapFromPixelData(ReadOnlySpan<byte> rgbPixelData, int width, int height)
        {
            Vector dpi = new(96, 96);
            WriteableBitmap bitmap = new(new PixelSize(width, height), dpi, PixelFormat.Bgra8888);
            using (ILockedFramebuffer frameBuffer = bitmap.Lock())
            {
                unsafe
                {
                    Span<byte> dest = new((void*)frameBuffer.Address, rgbPixelData.Length);
                    rgbPixelData.CopyTo(dest);
                }
            }
            return bitmap;
        }

        private static WriteableBitmap CreatePremultipliedBitmapFromPixelData(ReadOnlySpan<byte> rgbPixelData, int width, int height)
        {
            Vector dpi = new(96, 96);
            WriteableBitmap bitmap = new(new PixelSize(width, height), dpi, PixelFormat.Bgra8888);
            using (ILockedFramebuffer frameBuffer = bitmap.Lock())
            {
                unsafe
                {
                    Span<byte> dest = new((void*)frameBuffer.Address, rgbPixelData.Length);
                    rgbPixelData.CopyTo(dest);
                    for (int i = 0; i < dest.Length; i += 4)
                    {
                        byte a = dest[i + 3];
                        if (a == 255) continue;
                        if (a == 0)
                        {
                            dest[i + 0] = 0;
                            dest[i + 1] = 0;
                            dest[i + 2] = 0;
                            continue;
                        }
                        double factor = a / 255.0;
                        dest[i + 0] = (byte)Math.Round(dest[i + 0] * factor);
                        dest[i + 1] = (byte)Math.Round(dest[i + 1] * factor);
                        dest[i + 2] = (byte)Math.Round(dest[i + 2] * factor);
                    }
                }
            }
            return bitmap;
        }
    }
}
