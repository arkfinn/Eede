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
    private static readonly DataFormat<byte[]>[] ImageFormats = new[]
    {
        DataFormat.CreateBytesPlatformFormat("PNG"),
        DataFormat.CreateBytesPlatformFormat("DeviceIndependentBitmap"),
        DataFormat.CreateBytesPlatformFormat("TIFF")
    };

    public async Task CopyAsync(Picture picture)
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return;

        var bitmap = AvaloniaBitmapAdapter.StaticConvertToBitmap(picture);
        var item = new DataTransferItem();

        // 1. 標準 of Bitmap
        item.Set(DataFormat.Bitmap, bitmap);

        // 2. PNG
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

        // 2. カスタムフォーマット（PNG等）を試す
        foreach (var format in ImageFormats)
        {
            if (dataTransfer.Contains(format))
            {
                try
                {
                    var bytes = await dataTransfer.TryGetValueAsync(format);
                    if (bytes != null && bytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(bytes))
                        {
                            var bmp = new Bitmap(ms);
                            return AvaloniaBitmapAdapter.StaticConvertToPicture(bmp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Paste: Format error: {ex.Message}");
                }
            }
        }

        // 3. ファイルドロップ形式を試す
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

        // Windows 環境における、遅延レンダリングされた Snipping Tool や PrintScreen 等の画像データのネイティブ fallback 処理
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var tcs = new TaskCompletionSource<Picture?>();
                var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        if (System.Windows.Forms.Clipboard.ContainsImage())
                        {
                            using (var img = System.Windows.Forms.Clipboard.GetImage())
                            {
                                if (img != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        ms.Position = 0;
                                        var avaloniaBmp = new Avalonia.Media.Imaging.Bitmap(ms);
                                        tcs.SetResult(AvaloniaBitmapAdapter.StaticConvertToPicture(avaloniaBmp));
                                        return;
                                    }
                                }
                            }
                        }
                        tcs.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                var nativePic = await tcs.Task;
                if (nativePic != null)
                {
                    return nativePic;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows native clipboard fallback failed: {ex.Message}");
            }
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

        bool hasBitmap = false;
        try
        {
            var bitmap = await dataTransfer.TryGetBitmapAsync();
            hasBitmap = (bitmap != null);
        }
        catch {}

        bool containsFile = dataTransfer.Contains(DataFormat.File);

        bool containsCustom = false;
        foreach (var format in ImageFormats)
        {
            if (dataTransfer.Contains(format))
            {
                containsCustom = true;
                break;
            }
        }

        bool result = hasBitmap || containsFile || containsCustom;
        if (!result && OperatingSystem.IsWindows())
        {
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        tcs.SetResult(System.Windows.Forms.Clipboard.ContainsImage());
                    }
                    catch
                    {
                        tcs.SetResult(false);
                    }
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                result = await tcs.Task;
            }
            catch {}
        }

        return result;
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
