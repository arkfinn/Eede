#nullable enable
using System.IO;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Palettes;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes;

[TestFixture]
public class ImagePaletteExtractorTests
{
    private ImagePaletteExtractor _extractor = null!;

    [SetUp]
    public void SetUp()
    {
        _extractor = new ImagePaletteExtractor();
    }

    [Test]
    public async Task ExtractAsync_DirectColorPngUnder256Colors_ReturnsScannedPalette()
    {
        // 2x2 赤色画像
        byte[] pixels =
        [
            0, 0, 255, 255,   0, 0, 255, 255,
            0, 0, 255, 255,   0, 0, 255, 255
        ];
        var picture = Picture.Create(new PictureSize(2, 2), pixels);

        // ダミーの空ストリーム（Truecolorを想定）
        using MemoryStream ms = new([0x00]);

        var palette = await _extractor.ExtractAsync(ms, picture, ".png");

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Red, Is.EqualTo(255));
    }

    [Test]
    public async Task ExtractAsync_DirectColorOver256Colors_ReturnsNull()
    {
        byte[] pixels = new byte[272 * 4];
        for (int i = 0; i < 257; i++)
        {
            pixels[i * 4 + 0] = (byte)(i & 0xFF);
            pixels[i * 4 + 1] = (byte)((i >> 8) & 0xFF);
            pixels[i * 4 + 2] = 0;
            pixels[i * 4 + 3] = 255;
        }
        var picture = Picture.Create(new PictureSize(17, 16), pixels);

        using MemoryStream ms = new();

        var palette = await _extractor.ExtractAsync(ms, picture, ".png");

        Assert.That(palette, Is.Null);
    }
}
