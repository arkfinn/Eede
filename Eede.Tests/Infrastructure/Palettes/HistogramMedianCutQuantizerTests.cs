#nullable enable
using System;
using System.Collections.Generic;
using Eede.Application.Palettes;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Palettes;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes;

[TestFixture]
public class HistogramMedianCutQuantizerTests
{
    [Test]
    public void Scan_UnderOrEqual256Colors_ReturnsExactMatchPalette()
    {
        byte[] pixels = new byte[100 * 4];
        for (int i = 0; i < 100; i++)
        {
            pixels[i * 4 + 0] = (byte)i;
            pixels[i * 4 + 1] = (byte)(i * 2);
            pixels[i * 4 + 2] = (byte)(255 - i);
            pixels[i * 4 + 3] = 255;
        }
        var picture = Picture.Create(new PictureSize(10, 10), pixels);

        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.IsRepresentative, Is.False);
        Assert.That(result.Value.Palette.Fetch(0), Is.EqualTo(new ArgbColor(255, 255, 0, 0)));
    }

    [Test]
    public void Scan_Over256Colors_ReturnsRepresentative256Palette_AndAllColorsExistInSourceImage()
    {
        // 32x32 = 1024ピクセル、各ピクセルが異なる24bitカラー（RGB空間全体に分散）
        int width = 32;
        int height = 32;
        byte[] pixels = new byte[width * height * 4];
        HashSet<ArgbColor> sourceColors = [];

        int idx = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte r = (byte)((x * 8) + (y % 8));
                byte g = (byte)((y * 8) + (x % 8));
                byte b = (byte)(((x + y) * 4) & 0xFF);
                byte a = 255;

                pixels[idx * 4 + 0] = b;
                pixels[idx * 4 + 1] = g;
                pixels[idx * 4 + 2] = r;
                pixels[idx * 4 + 3] = a;
                sourceColors.Add(new ArgbColor(a, r, g, b));
                idx++;
            }
        }
        var picture = Picture.Create(new PictureSize(width, height), pixels);

        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.IsRepresentative, Is.True);

        Palette palette = result.Value.Palette;
        HashSet<ArgbColor> extractedUnique = [];
        for (int i = 0; i < Palette.MAX_LENGTH; i++)
        {
            ArgbColor c = palette.Fetch(i);
            Assert.That(c.Alpha, Is.EqualTo(255), $"Slot {i} should be opaque");
            Assert.That(sourceColors.Contains(c), Is.True, $"Slot {i} color {c} must exist in original image (In-Image Dominant Guarantee)");
            extractedUnique.Add(c);
        }

        Assert.That(extractedUnique.Count, Is.EqualTo(256), "256色のユニークな代表色が抽出されること");
    }

    [Test]
    public void Scan_Over256ColorsWithTransparentPixel_ReservesIndex0ForTransparent()
    {
        // 300ピクセル: 先頭が完全透明(A=0)、残り299ピクセルが異なるRGB555色
        int totalPixels = 300;
        byte[] pixels = new byte[totalPixels * 4];
        HashSet<ArgbColor> sourceColors = [];

        // Index 0: 完全透明 (RGBにゴミ値があっても (0,0,0,0) になること)
        pixels[0] = 123;
        pixels[1] = 45;
        pixels[2] = 67;
        pixels[3] = 0;

        for (int i = 1; i < totalPixels; i++)
        {
            byte r = (byte)(((i % 8) * 32) + (i % 7));
            byte g = (byte)((((i / 8) % 8) * 32) + (i % 5));
            byte b = (byte)((((i / 64) % 8) * 32) + (i % 3));
            pixels[i * 4 + 0] = b;
            pixels[i * 4 + 1] = g;
            pixels[i * 4 + 2] = r;
            pixels[i * 4 + 3] = 255;
            sourceColors.Add(new ArgbColor(255, r, g, b));
        }

        var picture = Picture.Create(new PictureSize(30, 10), pixels);
        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.IsRepresentative, Is.True);

        Palette palette = result.Value.Palette;
        Assert.That(palette.Fetch(0), Is.EqualTo(new ArgbColor(0, 0, 0, 0)), "Index 0 は完全透明 (0,0,0,0) に予約されること");

        for (int i = 1; i < Palette.MAX_LENGTH; i++)
        {
            ArgbColor c = palette.Fetch(i);
            Assert.That(sourceColors.Contains(c), Is.True, $"Slot {i} color {c} must exist in source image");
        }
    }

    [Test]
    public void Scan_RedAndBlueDominantFullColor_DoesNotSwapRedAndBlue()
    {
        // 300ピクセル: 前半100ピクセルが純赤(255,0,0)、次の100ピクセルが純青(0,0,255)、残り300ピクセルが様々な中間色
        int totalPixels = 500;
        byte[] pixels = new byte[totalPixels * 4];

        for (int i = 0; i < 100; i++)
        {
            pixels[i * 4 + 0] = 0;   // B
            pixels[i * 4 + 1] = 0;   // G
            pixels[i * 4 + 2] = 255; // R
            pixels[i * 4 + 3] = 255; // A
        }
        for (int i = 100; i < 200; i++)
        {
            pixels[i * 4 + 0] = 255; // B
            pixels[i * 4 + 1] = 0;   // G
            pixels[i * 4 + 2] = 0;   // R
            pixels[i * 4 + 3] = 255; // A
        }
        for (int i = 200; i < totalPixels; i++)
        {
            int v = i - 200;
            pixels[i * 4 + 0] = (byte)((v * 37) & 0xFF);
            pixels[i * 4 + 1] = (byte)((v * 73) & 0xFF);
            pixels[i * 4 + 2] = (byte)((v * 19) & 0xFF);
            pixels[i * 4 + 3] = 255;
        }

        var picture = Picture.Create(new PictureSize(50, 10), pixels);
        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);

        Assert.That(result, Is.Not.Null);
        Palette palette = result!.Value.Palette;

        bool hasPureRed = false;
        bool hasPureBlue = false;
        for (int i = 0; i < Palette.MAX_LENGTH; i++)
        {
            var c = palette.Fetch(i);
            if (c.EqualsArgb(new ArgbColor(255, 255, 0, 0))) hasPureRed = true;
            if (c.EqualsArgb(new ArgbColor(255, 0, 0, 255))) hasPureBlue = true;
        }

        Assert.That(hasPureRed, Is.True, "最頻出の純赤 (255, 0, 0) が保持されること");
        Assert.That(hasPureBlue, Is.True, "最頻出の純青 (0, 0, 255) が保持されること");
    }

    [Test]
    public void Scan_DegenerateRgb555Gradient257Colors_TerminatesAndBackfillsTo256ExactColors()
    {
        // 下位3bitの違いを含みRGB555では33ビンに縮退する257色画像
        byte[] pixels = new byte[272 * 4];
        HashSet<ArgbColor> sourceColors = [];
        for (int i = 0; i < 257; i++)
        {
            byte b = (byte)(i & 0xFF);
            byte g = (byte)((i >> 8) & 0xFF);
            byte r = 0;
            byte a = 255;
            pixels[i * 4 + 0] = b;
            pixels[i * 4 + 1] = g;
            pixels[i * 4 + 2] = r;
            pixels[i * 4 + 3] = a;
            sourceColors.Add(new ArgbColor(a, r, g, b));
        }
        // 残りのピクセルは先頭色と同じ不透明色
        for (int i = 257; i < 272; i++)
        {
            pixels[i * 4 + 3] = 255;
        }

        var picture = Picture.Create(new PictureSize(17, 16), pixels);
        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.IsRepresentative, Is.True);

        HashSet<ArgbColor> uniqueExtracted = [];
        for (int i = 0; i < Palette.MAX_LENGTH; i++)
        {
            var c = result.Value.Palette.Fetch(i);
            Assert.That(sourceColors.Contains(c), Is.True, $"Slot {i} color {c} must exist in source");
            uniqueExtracted.Add(c);
        }
        Assert.That(uniqueExtracted.Count, Is.EqualTo(256), "RGB555縮退時にもバックフィルにより256色の実在色が揃うこと");
    }

    [Test]
    public void Scan_AllLowAlphaFringe257Colors_SafelyBackfillsFromUniqueColors()
    {
        // 全ピクセルが 1 <= A < 16 で257色ある極端なエッジケース
        byte[] pixels = new byte[272 * 4];
        for (int i = 0; i < 257; i++)
        {
            pixels[i * 4 + 0] = (byte)(i & 0xFF);
            pixels[i * 4 + 1] = (byte)((i >> 8) & 0xFF);
            pixels[i * 4 + 2] = 10;
            pixels[i * 4 + 3] = 8; // A = 8 (< 16)
        }
        for (int i = 257; i < 272; i++)
        {
            pixels[i * 4 + 2] = 10;
            pixels[i * 4 + 3] = 8;
        }

        var picture = Picture.Create(new PictureSize(17, 16), pixels);
        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.IsRepresentative, Is.True);
        Assert.That(result.Value.Palette.Fetch(0).Alpha, Is.EqualTo(8));
    }

    [Test]
    public void Scan_2000x2000FullColorPicture_CompletesFastWithZeroLohAllocation()
    {
        int width = 2000;
        int height = 2000;
        byte[] pixels = new byte[width * height * 4];

        // イラスト調の領域（連続同色ラン＋多様なグラデーション）を生成
        for (int y = 0; y < height; y++)
        {
            byte r = (byte)(y & 0xFF);
            byte g = (byte)((y >> 2) & 0xFF);
            int rowOffset = y * width * 4;
            for (int x = 0; x < width; x++)
            {
                int idx = rowOffset + (x * 4);
                pixels[idx + 0] = (byte)((x >> 3) & 0xFF); // B (8px単位のラン)
                pixels[idx + 1] = g;                       // G
                pixels[idx + 2] = r;                       // R
                pixels[idx + 3] = 255;                     // A
            }
        }

        var picture = Picture.Create(new PictureSize(width, height), pixels);

        // JIT ウォームアップ & ArrayPool プール初期化
        _ = DirectColorPaletteScanner.Scan(picture);

        long beforeBytes = GC.GetAllocatedBytesForCurrentThread();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        PaletteExtractionResult? result = DirectColorPaletteScanner.Scan(picture);
        sw.Stop();
        long afterBytes = GC.GetAllocatedBytesForCurrentThread();
        long allocated = afterBytes - beforeBytes;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.IsRepresentative, Is.True);
        Assert.That(allocated, Is.LessThan(85000), $"LOHアロケーション(>=85,000B)が発生しないこと (実測: {allocated} bytes)");
        Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(60.0), $"2000x2000走査が高速に完了すること (実測: {sw.Elapsed.TotalMilliseconds:F2} ms)");
    }
}
