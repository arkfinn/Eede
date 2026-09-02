#nullable enable
using System;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Pictures;

[TestFixture]
public class SkiaSharpPictureCodecTests
{
    private SkiaSharpPictureCodec _codec = null!;

    [SetUp]
    public void SetUp()
    {
        _codec = new SkiaSharpPictureCodec();
    }

    [Test]
    public void EncodeAndDecode_1x1Picture_OpaquePixel_MaintainsExactValues()
    {
        var color = new ArgbColor(255, 120, 200, 80);
        var picture = CreatePicture(1, 1, (0, 0, color));

        byte[] pngBytes = _codec.EncodeToPng(picture);
        Assert.That(pngBytes, Is.Not.Null.And.Not.Empty);

        Picture decoded = _codec.DecodeFromPng(pngBytes);

        Assert.That(decoded.Width, Is.EqualTo(1));
        Assert.That(decoded.Height, Is.EqualTo(1));
        Assert.That(decoded.PickColor(new Position(0, 0)), Is.EqualTo(color));
        Assert.That(decoded.AsSpan().SequenceEqual(picture.AsSpan()), Is.True);
    }

    [Test]
    public void EncodeAndDecode_1x1Picture_TranslucentPixel_MaintainsExactValues()
    {
        // 半透明ピクセル（非乗算アルファの保持確認）
        var color = new ArgbColor(128, 200, 100, 50);
        var picture = CreatePicture(1, 1, (0, 0, color));

        byte[] pngBytes = _codec.EncodeToPng(picture);
        Picture decoded = _codec.DecodeFromPng(pngBytes);

        Assert.That(decoded.Width, Is.EqualTo(1));
        Assert.That(decoded.Height, Is.EqualTo(1));
        Assert.That(decoded.PickColor(new Position(0, 0)), Is.EqualTo(color));
        Assert.That(decoded.AsSpan().SequenceEqual(picture.AsSpan()), Is.True);
    }

    [Test]
    public void EncodeAndDecode_1x1Picture_FullyTransparentPixel_MaintainsAlphaZero()
    {
        var color = new ArgbColor(0, 0, 0, 0);
        var picture = CreatePicture(1, 1, (0, 0, color));

        byte[] pngBytes = _codec.EncodeToPng(picture);
        Picture decoded = _codec.DecodeFromPng(pngBytes);

        Assert.That(decoded.Width, Is.EqualTo(1));
        Assert.That(decoded.Height, Is.EqualTo(1));
        Assert.That(decoded.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0));
    }

    [Test]
    public void EncodeAndDecode_16x16Picture_VariedPixels_MaintainsAllPixelsExactly()
    {
        var pixels = new (int x, int y, ArgbColor color)[16 * 16];
        int idx = 0;
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                byte a = (byte)(y * 16 + 15);
                byte r = (byte)(x * 16);
                byte g = (byte)(y * 16);
                byte b = (byte)((x + y) * 8);
                pixels[idx++] = (x, y, new ArgbColor(a, r, g, b));
            }
        }
        var picture = CreatePicture(16, 16, pixels);

        byte[] pngBytes = _codec.EncodeToPng(picture);
        Picture decoded = _codec.DecodeFromPng(pngBytes);

        Assert.That(decoded.Width, Is.EqualTo(16));
        Assert.That(decoded.Height, Is.EqualTo(16));
        Assert.That(decoded.AsSpan().SequenceEqual(picture.AsSpan()), Is.True);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                var expected = picture.PickColor(new Position(x, y));
                var actual = decoded.PickColor(new Position(x, y));
                Assert.That(actual, Is.EqualTo(expected), $"Pixel mismatch at ({x}, {y})");
            }
        }
    }

    [Test]
    public void EncodeAndDecode_32x32Picture_Checkerboard_MaintainsAllPixelsExactly()
    {
        var colorA = new ArgbColor(255, 240, 240, 240);
        var colorB = new ArgbColor(200, 40, 80, 160);

        var pixels = new (int x, int y, ArgbColor color)[32 * 32];
        int idx = 0;
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                var color = ((x / 4) + (y / 4)) % 2 == 0 ? colorA : colorB;
                pixels[idx++] = (x, y, color);
            }
        }
        var picture = CreatePicture(32, 32, pixels);

        byte[] pngBytes = _codec.EncodeToPng(picture);
        Picture decoded = _codec.DecodeFromPng(pngBytes);

        Assert.That(decoded.Width, Is.EqualTo(32));
        Assert.That(decoded.Height, Is.EqualTo(32));
        Assert.That(decoded.AsSpan().SequenceEqual(picture.AsSpan()), Is.True);
    }

    [Test]
    public void EncodeToPng_ProducesValidPngSignature()
    {
        var picture = CreatePicture(2, 2, (0, 0, new ArgbColor(255, 255, 0, 0)));
        byte[] pngBytes = _codec.EncodeToPng(picture);

        byte[] expectedHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.That(pngBytes.Length, Is.GreaterThan(8));
        Assert.That(pngBytes.AsSpan(0, 8).SequenceEqual(expectedHeader), Is.True);
    }

    [Test]
    public void EncodeToPng_ThrowsOnNullPicture()
    {
        Assert.Throws<ArgumentNullException>(() => _codec.EncodeToPng(null!));
    }

    [Test]
    public void DecodeFromPng_ThrowsOnNullBytes()
    {
        Assert.Throws<ArgumentNullException>(() => _codec.DecodeFromPng(null!));
    }

    [Test]
    public void DecodeFromPng_ThrowsOnEmptyBytes()
    {
        var ex = Assert.Throws<ArgumentException>(() => _codec.DecodeFromPng([]));
        Assert.That(ex!.ParamName, Is.EqualTo("pngBytes"));
    }

    [Test]
    public void DecodeFromPng_ThrowsOnCorruptedData()
    {
        byte[] corruptedData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        Assert.Throws<ArgumentException>(() => _codec.DecodeFromPng(corruptedData));
    }

    [Test]
    public void DecodeFromPng_ThrowsOnNonPngHeader()
    {
        // JPEG header FF D8 FF
        byte[] fakeJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];
        Assert.Throws<ArgumentException>(() => _codec.DecodeFromPng(fakeJpeg));
    }

    [Test]
    public void DecodeFromPng_ThrowsOnValidHeaderWithCorruptedBody()
    {
        // Valid PNG header but corrupted body
        byte[] fakePng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00];
        Assert.Throws<ArgumentException>(() => _codec.DecodeFromPng(fakePng));
    }

    private static Picture CreatePicture(int width, int height, params (int x, int y, ArgbColor color)[] pixels)
    {
        byte[] data = new byte[width * height * 4];
        foreach (var (x, y, color) in pixels)
        {
            int index = (x + y * width) * 4;
            data[index] = color.Blue;
            data[index + 1] = color.Green;
            data[index + 2] = color.Red;
            data[index + 3] = color.Alpha;
        }
        return Picture.Create(new PictureSize(width, height), data);
    }
}
