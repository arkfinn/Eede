using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Eede.Application.Infrastructure;
using Eede.Domain.ImageEditing;
using Eede.Presentation.Common.Adapters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaIClipboard = Avalonia.Input.Platform.IClipboard;

namespace Eede.Presentation.Common.Adapters;

#nullable enable

public class AvaloniaClipboard : IClipboard
{
    // 一般的な画像形式の識別子
    private static readonly string[] ImageFormats = { "PNG", "Bitmap", "DeviceIndependentBitmap", "TIFF" };

    public async Task CopyAsync(Picture picture)
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return;

        var bitmap = AvaloniaBitmapAdapter.StaticConvertToBitmap(picture);
        var item = new DataTransferItem();

        // 1. 標準のBitmapオブジェクトとしてセット
        item.Set(DataFormat.Bitmap, bitmap);

        // 2. PNG形式としてバイナリをセット（互換性向上）
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms);
            item.Set(DataFormat.CreateBytesPlatformFormat("PNG"), ms.ToArray());
        }

        var dataTransfer = new DataTransfer();
        dataTransfer.Add(item);

        await clipboard.SetDataAsync(dataTransfer);
        System.Diagnostics.Debug.WriteLine("Copy: Set Bitmap and PNG data.");
    }

    public async Task<Picture?> GetPictureAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return null;

        var dataTransfer = await clipboard.TryGetDataAsync();
        if (dataTransfer == null) return null;

        // 1. 既知の画像フォーマットを順に試す
        var bitmap = await dataTransfer.TryGetBitmapAsync();
        if (bitmap != null)
        {
            return AvaloniaBitmapAdapter.StaticConvertToPicture(bitmap);
        }

        // 2. ファイルドロップ形式を試す
        try
        {
            var items = await dataTransfer.TryGetFilesAsync();
            if (items != null)
            {
                var firstFile = items.OfType<IStorageFile>().FirstOrDefault();
                if (firstFile != null)
                {
                    using (var stream = await firstFile.OpenReadAsync())
                    {
                        var bitmapFromFile = new Bitmap(stream);
                        return AvaloniaBitmapAdapter.StaticConvertToPicture(bitmapFromFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Paste: Files format error: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("Paste: No image data found.");
        return null;
    }

    public async Task<bool> HasPictureAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return false;

        var dataTransfer = await clipboard.TryGetDataAsync();
        if (dataTransfer == null) return false;

        return dataTransfer.Contains(DataFormat.Bitmap) || dataTransfer.Contains(DataFormat.File);
    }

    private AvaloniaIClipboard? GetClipboard()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }
        return null;
    }
}
