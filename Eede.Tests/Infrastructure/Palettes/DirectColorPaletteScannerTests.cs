#nullable enable
using System;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Palettes;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes;

[TestFixture]
public class DirectColorPaletteScannerTests
{
    [Test]
    public void Scan_SingleColorPicture_ReturnsPaletteWithSingleColor()
    {
        // 2x2 の赤色単色画像
        byte[] pixels =
        [
            0, 0, 255, 255,   0, 0, 255, 255,
            0, 0, 255, 255,   0, 0, 255, 255
        ];
        var picture = Picture.Create(new PictureSize(2, 2), pixels);

        var palette = DirectColorPaletteScanner.Scan(picture);

        Assert.That(palette, Is.Not.Null);
        var firstColor = palette!.Fetch(0);
        Assert.That(firstColor.Red, Is.EqualTo(255));
        Assert.That(firstColor.Green, Is.EqualTo(0));
        Assert.That(firstColor.Blue, Is.EqualTo(0));
        Assert.That(firstColor.Alpha, Is.EqualTo(255));

        // 残りのスロットは透明黒
        var secondColor = palette.Fetch(1);
        Assert.That(secondColor.Alpha, Is.EqualTo(0));
    }

    [Test]
    public void Scan_Exactly256Colors_ReturnsPalette()
    {
        // 16x16 = 256ピクセルで全ピクセル異なる色
        byte[] pixels = new byte[256 * 4];
        for (int i = 0; i < 256; i++)
        {
            pixels[i * 4 + 0] = (byte)i;       // B
            pixels[i * 4 + 1] = 0;             // G
            pixels[i * 4 + 2] = 0;             // R
            pixels[i * 4 + 3] = 255;           // A
        }
        var picture = Picture.Create(new PictureSize(16, 16), pixels);

        var palette = DirectColorPaletteScanner.Scan(picture);

        Assert.That(palette, Is.Not.Null);
        for (int i = 0; i < 256; i++)
        {
            var color = palette!.Fetch(i);
            Assert.That(color.Blue, Is.EqualTo((byte)i));
            Assert.That(color.Alpha, Is.EqualTo(255));
        }
    }

    [Test]
    public void Scan_257Colors_ReturnsNullDueToEarlyExit()
    {
        // 257 ピクセル（257色）
        // 17x16 = 272 ピクセル
        byte[] pixels = new byte[272 * 4];
        for (int i = 0; i < 257; i++)
        {
            pixels[i * 4 + 0] = (byte)(i & 0xFF);
            pixels[i * 4 + 1] = (byte)((i >> 8) & 0xFF);
            pixels[i * 4 + 2] = 0;
            pixels[i * 4 + 3] = 255;
        }
        var picture = Picture.Create(new PictureSize(17, 16), pixels);

        var palette = DirectColorPaletteScanner.Scan(picture);

        Assert.That(palette, Is.Null);
    }

    [Test]
    public void Scan_MultipleTransparentColorsWithDifferentRgb_NormalizedToOneColor()
    {
        // 完全透明 (A=0) で RGB が異なるピクセル群
        byte[] pixels =
        [
            255, 0, 0, 0,      // 青 (A=0)
            0, 255, 0, 0,      // 緑 (A=0)
            0, 0, 255, 0,      // 赤 (A=0)
            255, 255, 255, 255 // 白 (A=255)
        ];
        var picture = Picture.Create(new PictureSize(2, 2), pixels);

        var palette = DirectColorPaletteScanner.Scan(picture);

        Assert.That(palette, Is.Not.Null);
        // 最初の透明色は (0,0,0,0) に正規化されて 1 つ目
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Alpha, Is.EqualTo(0));
        Assert.That(c0.Red, Is.EqualTo(0));
        Assert.That(c0.Green, Is.EqualTo(0));
        Assert.That(c0.Blue, Is.EqualTo(0));

        // 2つ目は白
        var c1 = palette.Fetch(1);
        Assert.That(c1.Alpha, Is.EqualTo(255));
        Assert.That(c1.Red, Is.EqualTo(255));
        Assert.That(c1.Green, Is.EqualTo(255));
        Assert.That(c1.Blue, Is.EqualTo(255));

        // 3つ目は未使用スロット（透明）
        var c2 = palette.Fetch(2);
        Assert.That(c2.Alpha, Is.EqualTo(0));
    }

    [Test]
    public void Scan_MaintainsFirstSeenOrder()
    {
        // 赤、緑、青の順に出現
        byte[] pixels =
        [
            0, 0, 255, 255,    // 赤
            0, 255, 0, 255,    // 緑
            255, 0, 0, 255,    // 青
            0, 0, 255, 255     // 再び赤（重複）
        ];
        var picture = Picture.Create(new PictureSize(2, 2), pixels);

        var palette = DirectColorPaletteScanner.Scan(picture);

        Assert.That(palette, Is.Not.Null);
        Assert.That(palette!.Fetch(0).Red, Is.EqualTo(255), "0番は赤");
        Assert.That(palette.Fetch(1).Green, Is.EqualTo(255), "1番は緑");
        Assert.That(palette.Fetch(2).Blue, Is.EqualTo(255), "2番は青");
    }
}
