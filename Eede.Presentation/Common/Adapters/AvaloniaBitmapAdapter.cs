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
        public Bitmap ConvertToBitmap(Picture picture) => StaticConvertToBitmap(picture);

        public Bitmap ConvertToPremultipliedBitmap(Picture picture) => StaticConvertToPremultipliedBitmap(picture);

        public Picture ConvertToPicture(Bitmap bitmap) => StaticConvertToPicture(bitmap);

        public static Bitmap StaticConvertToBitmap(Picture picture)
        {
            return CreateBitmapFromPixelData(picture.AsSpan(), picture.Width, picture.Height, AlphaFormat.Unpremul);
        }

        public static Bitmap StaticConvertToPremultipliedBitmap(Picture picture)
        {
            return CreateBitmapFromPixelData(picture.AsSpan(), picture.Width, picture.Height, AlphaFormat.Premul);
        }

        public static Picture StaticConvertToPicture(Bitmap bitmap)
        {
            int width = bitmap.PixelSize.Width;
            int height = bitmap.PixelSize.Height;
            byte[] pixels = new byte[width * 4 * height];

            if (bitmap is WriteableBitmap wb)
            {
                using (var lockBuffer = wb.Lock())
                {
                    unsafe
                    {
                        byte* srcBase = (byte*)lockBuffer.Address;
                        int srcStride = lockBuffer.RowBytes;
                        bool isRgba = IsRgba(lockBuffer.Format);
                        int destStride = width * 4;

                        fixed (byte* destBase = pixels)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                byte* sRow = srcBase + y * srcStride;
                                byte* dRow = destBase + y * destStride;

                                for (int x = 0; x < width; x++)
                                {
                                    byte* s = sRow + x * 4;
                                    byte* d = dRow + x * 4;

                                    if (isRgba)
                                    {
                                        d[0] = s[2]; // B <- R
                                        d[1] = s[1]; // G <- G
                                        d[2] = s[0]; // R <- B
                                        d[3] = s[3]; // A <- A
                                    }
                                    else
                                    {
                                        d[0] = s[0];
                                        d[1] = s[1];
                                        d[2] = s[2];
                                        d[3] = s[3];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                GCHandle pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try
                {
                    bitmap.CopyPixels(new PixelRect(0, 0, width, height), pinnedArray.AddrOfPinnedObject(), pixels.Length, width * 4);
                }
                finally
                {
                    pinnedArray.Free();
                }
            }
            return Picture.Create(new PictureSize(width, height), pixels);
        }

        private static bool IsRgba(PixelFormat format)
        {
            string s = format.ToString();
            return s.Contains("Rgba8888") || s.Contains("RGBA8888");
        }

        private static WriteableBitmap CreateBitmapFromPixelData(ReadOnlySpan<byte> bgraPixelData, int width, int height, AlphaFormat alphaFormat)
        {
            var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, alphaFormat);
            using (var lockBuffer = bitmap.Lock())
            {
                unsafe
                {
                    byte* destBase = (byte*)lockBuffer.Address;
                    int destStride = lockBuffer.RowBytes;
                    bool isRgba = IsRgba(lockBuffer.Format);
                    int srcStride = width * 4;

                    fixed (byte* srcBase = bgraPixelData)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            byte* sRow = srcBase + y * srcStride;
                            byte* dRow = destBase + y * destStride;

                            for (int x = 0; x < width; x++)
                            {
                                byte* s = sRow + x * 4;
                                byte* d = dRow + x * 4;

                                if (isRgba)
                                {
                                    d[0] = s[2]; // R <- R
                                    d[1] = s[1]; // G <- G
                                    d[2] = s[0]; // B <- B
                                    d[3] = s[3]; // A <- A
                                }
                                else
                                {
                                    d[0] = s[0];
                                    d[1] = s[1];
                                    d[2] = s[2];
                                    d[3] = s[3];
                                }
                            }
                        }
                    }
                }
            }
            return bitmap;
        }
    }
}
