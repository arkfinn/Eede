using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Domain.Pictures;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Eede.Presentation.Common.Adapters;

public class PictureBitmapAdapter
{
    // ModelからViewModelへの変換メソッド
    public Bitmap ConvertToBitmap(Picture picture)
    {
        byte[] src = picture.CloneImage();
        return CreateBitmapFromPixelData(src, picture.Width, picture.Height);
    }

    // ViewModelからModelへの変換メソッド
    public Picture ConvertToPicture(Bitmap bitmap)
    {
        int stride = bitmap.PixelSize.Width * 4;
        byte[] pixels = new byte[stride * bitmap.PixelSize.Height];
        GCHandle pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        bitmap.CopyPixels(new PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height), pointer, pixels.Length, stride);
        pinnedArray.Free();
        return Picture.Create(new PictureSize(bitmap.PixelSize.Width, bitmap.PixelSize.Height), pixels);
    }

    private WriteableBitmap CreateBitmapFromPixelData(byte[] rgbPixelData, int width, int height)
    {
        Vector dpi = new(96, 96);
        WriteableBitmap bitmap = new(new PixelSize(width, height), dpi, PixelFormat.Bgra8888);
        using (var frameBuffer = bitmap.Lock())
        {
            Marshal.Copy(rgbPixelData, 0, frameBuffer.Address, rgbPixelData.Length);
        }
        return bitmap;
    }
}
