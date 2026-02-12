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
        var dataObject = new DataObject();

        // 1. 標準のBitmapオブジェクトとしてセット
        dataObject.Set("Bitmap", bitmap);

        // 2. PNG形式としてバイナリをセット（互換性向上）
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms);
            dataObject.Set("PNG", ms.ToArray());
        }

        await clipboard.SetDataObjectAsync(dataObject);
        System.Diagnostics.Debug.WriteLine("Copy: Set Bitmap and PNG data.");
    }

    public async Task<Picture?> GetPictureAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return null;

        var formats = (await clipboard.GetFormatsAsync()).ToList();
        System.Diagnostics.Debug.WriteLine($"Paste: Clipboard formats: {string.Join(", ", formats)}");

        // 1. 既知の画像フォーマットを順に試す
        foreach (var format in ImageFormats)
        {
            if (formats.Contains(format, StringComparer.OrdinalIgnoreCase))
            {
                var picture = await TryGetFromFormat(clipboard, format);
                if (picture != null) return picture;
            }
        }

        // 2. ファイルドロップ形式を試す
        if (formats.Contains(DataFormats.Files))
        {
            try
            {
                var data = await clipboard.GetDataAsync(DataFormats.Files);
                IEnumerable<IStorageItem>? items = data as IEnumerable<IStorageItem>;
                if (items == null && data is IDataObject dataObject)
                {
                    items = dataObject.GetFiles();
                }

                var firstFile = items?.OfType<IStorageFile>().FirstOrDefault();
                if (firstFile != null)
                {
                    using (var stream = await firstFile.OpenReadAsync())
                    {
                        var bitmap = new Bitmap(stream);
                        return AvaloniaBitmapAdapter.StaticConvertToPicture(bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Paste: Files format error: {ex.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine("Paste: No image data found.");
        return null;
    }

    private async Task<Picture?> TryGetFromFormat(AvaloniaIClipboard clipboard, string format)
    {
        try
        {
            object? data = await clipboard.GetDataAsync(format);
            if (data == null)
            {
                System.Diagnostics.Debug.WriteLine($"Paste: GetDataAsync('{format}') returned null.");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Paste: GetDataAsync('{format}') type: {data.GetType().FullName}");

            if (data is Bitmap bitmap)
            {
                return AvaloniaBitmapAdapter.StaticConvertToPicture(bitmap);
            }

            if (data is Stream stream)
            {
                return AvaloniaBitmapAdapter.StaticConvertToPicture(new Bitmap(stream));
            }

            if (data is byte[] bytes)
            {
                using (var ms = new MemoryStream(bytes))
                {
                    return AvaloniaBitmapAdapter.StaticConvertToPicture(new Bitmap(ms));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Paste: Error in format '{format}': {ex.Message}");
        }
        return null;
    }

    public async Task<bool> HasPictureAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return false;

        var formats = await clipboard.GetFormatsAsync();
        if (formats == null) return false;

        return formats.Any(f => ImageFormats.Contains(f, StringComparer.OrdinalIgnoreCase) || f == DataFormats.Files);
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
