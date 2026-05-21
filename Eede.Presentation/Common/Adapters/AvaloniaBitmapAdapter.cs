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
                        bool isPremultiplied = wb.AlphaFormat == AlphaFormat.Premul;

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

                                    byte a = s[3];
                                    byte r = isRgba ? s[0] : s[2];
                                    byte g = s[1];
                                    byte b = isRgba ? s[2] : s[0];

                                    // ビットマップが乗算済みの場合は非乗算に戻す
                                    if (isPremultiplied && a > 0 && a < 255)
                                    {
                                        r = (byte)Math.Min(255, (r * 255) / a);
                                        g = (byte)Math.Min(255, (g * 255) / a);
                                        b = (byte)Math.Min(255, (b * 255) / a);
                                    }

                                    d[0] = b;
                                    d[1] = g;
                                    d[2] = r;
                                    d[3] = a;
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                finally
                {
                    pinnedArray.Free();
                }

                // For non-writeable bitmaps, we don't easily know the format, 
                // but we assume standard platform behavior.
            }
            return Picture.Create(new PictureSize(width, height), pixels);
        }

        private static bool IsRgba(PixelFormat format)
        {
            var s = format.ToString() ?? "";
            return format == PixelFormat.Rgba8888 || s.Contains("Rgba8888") || s.Contains("RGBA8888");
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
                    bool shouldPremultiply = alphaFormat == AlphaFormat.Premul;

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

                                byte b = s[0];
                                byte g = s[1];
                                byte r = s[2];
                                byte a = s[3];

                                if (shouldPremultiply && a < 255)
                                {
                                    b = (byte)((b * a) / 255);
                                    g = (byte)((g * a) / 255);
                                    r = (byte)((r * a) / 255);
                                }

                                if (isRgba)
                                {
                                    d[0] = r; d[1] = g; d[2] = b; d[3] = a;
                                }
                                else
                                {
                                    d[0] = b; d[1] = g; d[2] = r; d[3] = a;
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
