#nullable enable
using System;
using System.Buffers.Binary;
using System.IO;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Palettes;

[TestFixture]
public class PngPaletteReaderTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static byte[] BuildChunk(string type, byte[] data)
    {
        byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        byte[] chunk = new byte[8 + data.Length + 4];
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(0, 4), (uint)data.Length);
        typeBytes.CopyTo(chunk, 4);
        data.CopyTo(chunk, 8);
        // CRC 4バイトは0埋め
        return chunk;
    }

    [Test]
    public void Read_IndexedColorPng_ReturnsPaletteWithPlteColors()
    {
        // IHDR: 1x1, 8bit, ColorType=3 (Indexed)
        byte[] ihdrData = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdrData.AsSpan(0, 4), 1); // width
        BinaryPrimitives.WriteUInt32BigEndian(ihdrData.AsSpan(4, 4), 1); // height
        ihdrData[8] = 8; // bit depth
        ihdrData[9] = 3; // ColorType 3 = Indexed

        // PLTE: 2色 (赤, 緑)
        byte[] plteData =
        [
            255, 0, 0,  // 赤
            0, 255, 0   // 緑
        ];

        // IDAT: ダミー
        byte[] idatData = [0x78, 0x9C, 0x63, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01];

        using MemoryStream ms = new();
        ms.Write(PngSignature);
        ms.Write(BuildChunk("IHDR", ihdrData));
        ms.Write(BuildChunk("PLTE", plteData));
        ms.Write(BuildChunk("IDAT", idatData));
        ms.Write(BuildChunk("IEND", []));
        ms.Position = 0;

        var palette = PngPaletteReader.Read(ms);

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Red, Is.EqualTo(255));
        Assert.That(c0.Green, Is.EqualTo(0));
        Assert.That(c0.Blue, Is.EqualTo(0));
        Assert.That(c0.Alpha, Is.EqualTo(255));

        var c1 = palette.Fetch(1);
        Assert.That(c1.Red, Is.EqualTo(0));
        Assert.That(c1.Green, Is.EqualTo(255));
        Assert.That(c1.Blue, Is.EqualTo(0));
        Assert.That(c1.Alpha, Is.EqualTo(255));

        // 未使用スロットは透明
        var c2 = palette.Fetch(2);
        Assert.That(c2.Alpha, Is.EqualTo(0));
    }

    [Test]
    public void Read_IndexedColorPngWithTrns_AppliesAlphaValues()
    {
        byte[] ihdrData = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdrData.AsSpan(0, 4), 1);
        BinaryPrimitives.WriteUInt32BigEndian(ihdrData.AsSpan(4, 4), 1);
        ihdrData[8] = 8;
        ihdrData[9] = 3; // Indexed

        byte[] plteData =
        [
            255, 0, 0, // 赤
            0, 255, 0  // 緑
        ];

        // tRNS: インデックス0は完全透明(0)、インデックス1は半透明(128)
        byte[] trnsData = [0, 128];

        using MemoryStream ms = new();
        ms.Write(PngSignature);
        ms.Write(BuildChunk("IHDR", ihdrData));
        ms.Write(BuildChunk("PLTE", plteData));
        ms.Write(BuildChunk("tRNS", trnsData));
        ms.Write(BuildChunk("IDAT", []));
        ms.Write(BuildChunk("IEND", []));
        ms.Position = 0;

        var palette = PngPaletteReader.Read(ms);

        Assert.That(palette, Is.Not.Null);
        var c0 = palette!.Fetch(0);
        Assert.That(c0.Alpha, Is.EqualTo(0), "インデックス0のアルファ値は0");
        Assert.That(c0.Red, Is.EqualTo(255));

        var c1 = palette.Fetch(1);
        Assert.That(c1.Alpha, Is.EqualTo(128), "インデックス1のアルファ値は128");
        Assert.That(c1.Green, Is.EqualTo(255));
    }

    [Test]
    public void Read_TruecolorPng_ReturnsNull()
    {
        // ColorType = 6 (RGBA)
        byte[] ihdrData = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdrData.AsSpan(0, 4), 1);
        BinaryPrimitives.WriteUInt32BigEndian(ihdrData.AsSpan(4, 4), 1);
        ihdrData[8] = 8;
        ihdrData[9] = 6; // Truecolor with Alpha

        using MemoryStream ms = new();
        ms.Write(PngSignature);
        ms.Write(BuildChunk("IHDR", ihdrData));
        ms.Write(BuildChunk("IDAT", []));
        ms.Position = 0;

        var palette = PngPaletteReader.Read(ms);

        Assert.That(palette, Is.Null, "Truecolor PNG は null を返す");
    }

    [Test]
    public void Read_InvalidOrTruncatedStream_ReturnsNullWithoutThrowing()
    {
        using MemoryStream ms = new([0x00, 0x01, 0x02]);
        var palette = PngPaletteReader.Read(ms);
        Assert.That(palette, Is.Null);
    }
}
