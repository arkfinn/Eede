using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System;
using System.Runtime.InteropServices;

namespace Eede.Presentation.Common.Adapters;

public class PictureBitmapAdapter
{
    // ModelからViewModelへの変換メソッド
    public static Bitmap ConvertToBitmap(Picture picture)
    {
        byte[] src = picture.CloneImage();
        return CreateBitmapFromPixelData(src, picture.Width, picture.Height);
    }

    public static Bitmap ConvertToPremultipliedBitmap(Picture picture)
    {
        byte[] src = picture.CloneImage();
        return CreatePremultipliedBitmapFromPixelData(src, picture.Width, picture.Height);
    }

    // ViewModelからModelへの変換メソッド
    public static Picture ConvertToPicture(Bitmap bitmap)
    {
        int stride = bitmap.PixelSize.Width * 4;
        byte[] pixels = new byte[stride * bitmap.PixelSize.Height];
        GCHandle pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        bitmap.CopyPixels(new PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height), pointer, pixels.Length, stride);
        pinnedArray.Free();
        return Picture.Create(new PictureSize(bitmap.PixelSize.Width, bitmap.PixelSize.Height), pixels);
    }


    private static WriteableBitmap CreateBitmapFromPixelData(byte[] rgbPixelData, int width, int height)
    {
        Vector dpi = new(96, 96);
        WriteableBitmap bitmap = new(new PixelSize(width, height), dpi, PixelFormat.Bgra8888);
        using (ILockedFramebuffer frameBuffer = bitmap.Lock())
        {
            // rgbPixelData をフレームバッファへコピー
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
            // rgbPixelData をフレームバッファへコピー
            Marshal.Copy(rgbPixelData, 0, frameBuffer.Address, rgbPixelData.Length);

            // unsafe ブロックでロックされたメモリに直接アクセス（.NET Core 3.1 以降なら Span で簡潔に）
            unsafe
            {
                Span<byte> pixels = new((void*)frameBuffer.Address, rgbPixelData.Length);
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    // B, G, R, A の順番なので、A を取り出して計算
                    byte a = pixels[i + 3];
                    double factor = a / 255.0;
                    // B, G, R に対して α による乗算を適用（Math.Round で丸め）
                    pixels[i + 0] = (byte)Math.Round(pixels[i + 0] * factor);
                    pixels[i + 1] = (byte)Math.Round(pixels[i + 1] * factor);
                    pixels[i + 2] = (byte)Math.Round(pixels[i + 2] * factor);
                    // A はそのまま
                }
            }

        }
        return bitmap;
    }

}
