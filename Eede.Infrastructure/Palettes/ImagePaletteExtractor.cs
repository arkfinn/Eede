#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Eede.Application.Palettes;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;

namespace Eede.Infrastructure.Palettes;

/// <summary>
/// 画像ストリームおよびデコード済み Picture からパレットを抽出する能動アクター。
/// </summary>
public class ImagePaletteExtractor : IImagePaletteExtractor
{
    public Task<Palette?> ExtractAsync(Stream stream, Picture picture, string extension)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(picture);

        string ext = (extension ?? string.Empty).ToLowerInvariant();

        try
        {
            if (ext == ".arv")
            {
                var arvPalette = ArvPaletteReader.Read(stream);
                return Task.FromResult(arvPalette);
            }

            if (ext == ".png")
            {
                // 1. まずインデックスカラー PNG (PLTE チャンク) を試行
                var pngPalette = PngPaletteReader.Read(stream);
                if (pngPalette != null)
                {
                    return Task.FromResult<Palette?>(pngPalette);
                }

                // 2. インデックスカラーでない場合はダイレクトカラーピクセルを走査
                var scannedPalette = DirectColorPaletteScanner.Scan(picture);
                return Task.FromResult(scannedPalette);
            }

            // その他の画像形式（BMP等）はピクセル走査
            var defaultScanned = DirectColorPaletteScanner.Scan(picture);
            return Task.FromResult(defaultScanned);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            System.Diagnostics.Trace.WriteLine($"[ImagePaletteExtractor] Failed to extract palette: {ex.Message}");
            return Task.FromResult<Palette?>(null);
        }
    }
}
