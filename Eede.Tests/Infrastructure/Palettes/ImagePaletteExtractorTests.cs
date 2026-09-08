#nullable enable
using System;
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
    private Picture _dummyPicture = null!;

    [SetUp]
    public void SetUp()
    {
        _extractor = new ImagePaletteExtractor();
        byte[] pixels =
        [
            0, 0, 255, 255,   0, 0, 255, 255,
            0, 0, 255, 255,   0, 0, 255, 255
        ];
        _dummyPicture = Picture.Create(new PictureSize(2, 2), pixels);
    }

    [Test]
    public void ExtractAsync_ThrowsArgumentNullException_WhenStreamIsNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _extractor.ExtractAsync(null!, _dummyPicture, ".png"));
    }

    [Test]
    public void ExtractAsync_ThrowsArgumentNullException_WhenPictureIsNull()
    {
        using MemoryStream ms = new();
        Assert.ThrowsAsync<ArgumentNullException>(() => _extractor.ExtractAsync(ms, null!, ".png"));
    }

    [Test]
    public async Task ExtractAsync_DirectColorPngUnder256Colors_ReturnsScannedPalette()
    {
        using MemoryStream ms = new([0x00]);

        var palette = await _extractor.ExtractAsync(ms, _dummyPicture, ".png");

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

    [Test]
    public async Task ExtractAsync_ArvExtension_ExtractsArvPalette()
    {
        byte[] header = new byte[40];
        header[16] = (byte)'N';
        header[17] = (byte)'R';

        byte paletteLength = (byte)(2 + (16 * 6));
        byte[] palettePayload = new byte[1 + 1 + (16 * 6)];
        palettePayload[0] = paletteLength;
        for (int i = 0; i < 16; i++)
        {
            int offset = 2 + (i * 6);
            palettePayload[offset] = 15;     // R: 255
            palettePayload[offset + 2] = 0;  // G: 0
            palettePayload[offset + 4] = 0;  // B: 0
        }

        using MemoryStream ms = new();
        ms.Write(header);
        ms.Write(palettePayload);
        ms.Position = 0;

        var palette = await _extractor.ExtractAsync(ms, _dummyPicture, ".arv");

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Red, Is.EqualTo(255));
    }

    [Test]
    public async Task ExtractAsync_NoExtensionWithPngSignature_DetectsPngAndScans()
    {
        // 先頭8バイトに PNG シグネチャを設定
        byte[] pngSig = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        using MemoryStream ms = new(pngSig);

        // 拡張子空文字
        var palette = await _extractor.ExtractAsync(ms, _dummyPicture, "");

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Red, Is.EqualTo(255));
    }

    [Test]
    public async Task ExtractAsync_OtherExtension_ScansDirectColors()
    {
        using MemoryStream ms = new();

        var palette = await _extractor.ExtractAsync(ms, _dummyPicture, ".bmp");

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Red, Is.EqualTo(255));
    }

    [Test]
    public async Task ExtractAsync_WhenStreamThrowsIOException_ReturnsNullSafely()
    {
        var faultyStream = new FaultyStream();

        var palette = await _extractor.ExtractAsync(faultyStream, _dummyPicture, ".png");

        Assert.That(palette, Is.Null);
    }

    private class FaultyStream : MemoryStream
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new IOException("Simulated I/O failure");
        }
    }
}
