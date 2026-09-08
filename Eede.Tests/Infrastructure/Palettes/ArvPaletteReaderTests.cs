#nullable enable
using System;
using System.IO;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes;

[TestFixture]
public class ArvPaletteReaderTests
{
    private static byte[] CreateValidArvHeader(char imageFlag = 'N', char paletteFlag = 'R')
    {
        byte[] data = new byte[40]; // 16(dummy) + 6(flags) + 2(dummy) + 4(size) + 12(dummy) = 40
        // arvHeaderFlags at offset 16..21
        data[16] = (byte)imageFlag;
        data[17] = (byte)paletteFlag;
        return data;
    }

    [Test]
    public void Read_ThrowsArgumentNullException_WhenStreamIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ArvPaletteReader.Read(null!));
    }

    [TestCase(10)]
    [TestCase(20)]
    [TestCase(23)]
    public void Read_ReturnsNull_WhenStreamIsTooShortForHeader(int length)
    {
        using MemoryStream ms = new(new byte[length]);
        var result = ArvPaletteReader.Read(ms);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Read_ReturnsNull_WhenPaletteFlagIsNotR()
    {
        byte[] header = CreateValidArvHeader('N', 'N');
        using MemoryStream ms = new(header);
        var result = ArvPaletteReader.Read(ms);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Read_ReturnsValidPalette_WhenValidArvPaletteDataProvided()
    {
        byte[] header = CreateValidArvHeader('N', 'R');

        // Palette data: 1 byte (length: 2 + 16*6 = 98) + 1 byte (dummy) + 96 bytes (16 colors * 6 bytes)
        byte paletteLength = (byte)(2 + (16 * 6));
        byte[] palettePayload = new byte[1 + 1 + (16 * 6)];
        palettePayload[0] = paletteLength;
        palettePayload[1] = 0; // skip byte

        for (int i = 0; i < 16; i++)
        {
            int offset = 2 + (i * 6);
            palettePayload[offset + 0] = (byte)i;        // Red (0..15)
            palettePayload[offset + 1] = 0;             // dummy
            palettePayload[offset + 2] = (byte)(15 - i); // Green (15..0)
            palettePayload[offset + 3] = 0;             // dummy
            palettePayload[offset + 4] = (byte)(i / 2);  // Blue (0..7)
            palettePayload[offset + 5] = 0;             // dummy
        }

        using MemoryStream ms = new();
        ms.Write(header);
        ms.Write(palettePayload);
        ms.Position = 0;

        var palette = ArvPaletteReader.Read(ms);

        Assert.That(palette, Is.Not.Null);
        // 検証: Color 0 (R=0*17=0, G=15*17=255, B=0)
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Alpha, Is.EqualTo(255));
        Assert.That(c0.Red, Is.EqualTo(0));
        Assert.That(c0.Green, Is.EqualTo(255));
        Assert.That(c0.Blue, Is.EqualTo(0));

        // 検証: Color 15 (R=15*17=255, G=0, B=7*17=119)
        var c15 = palette.Fetch(15);
        Assert.That(c15.Alpha, Is.EqualTo(255));
        Assert.That(c15.Red, Is.EqualTo(255));
        Assert.That(c15.Green, Is.EqualTo(0));
        Assert.That(c15.Blue, Is.EqualTo(119));

        // 16色以降は透明 (0,0,0,0)
        var c16 = palette.Fetch(16);
        Assert.That(c16.Alpha, Is.EqualTo(0));
    }

    [Test]
    public void Read_SkipsImageData_WhenImageFlagIsI()
    {
        byte[] header = CreateValidArvHeader('I', 'R');

        // Image data length (uint16) + dummy image data
        ushort imageLength = 10; // 2(length) + 8(data)
        byte[] imagePayload = new byte[imageLength];
        imagePayload[0] = (byte)(imageLength & 0xFF);
        imagePayload[1] = (byte)((imageLength >> 8) & 0xFF);

        // Palette data
        byte paletteLength = (byte)(2 + (16 * 6));
        byte[] palettePayload = new byte[1 + 1 + (16 * 6)];
        palettePayload[0] = paletteLength;
        palettePayload[1] = 0;
        for (int i = 0; i < 16; i++)
        {
            int offset = 2 + (i * 6);
            palettePayload[offset] = 15;     // R: 255
            palettePayload[offset + 2] = 15; // G: 255
            palettePayload[offset + 4] = 15; // B: 255
        }

        using MemoryStream ms = new();
        ms.Write(header);
        ms.Write(imagePayload);
        ms.Write(palettePayload);
        ms.Position = 0;

        var palette = ArvPaletteReader.Read(ms);

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Red, Is.EqualTo(255));
    }

    [Test]
    public void Read_ReturnsNull_WhenPaletteBytesAreTruncated()
    {
        byte[] header = CreateValidArvHeader('N', 'R');
        // パレット長 98 を宣言するが、実際のデータが途中で切れている
        byte[] truncatedPalette = [98, 0, 1, 2, 3];

        using MemoryStream ms = new();
        ms.Write(header);
        ms.Write(truncatedPalette);
        ms.Position = 0;

        var result = ArvPaletteReader.Read(ms);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Read_RestoresStreamPosition_WhenStreamCanSeek()
    {
        byte[] header = CreateValidArvHeader('N', 'N');
        using MemoryStream ms = new(header);
        ms.Position = 5;

        _ = ArvPaletteReader.Read(ms);

        Assert.That(ms.Position, Is.EqualTo(5));
    }
}
