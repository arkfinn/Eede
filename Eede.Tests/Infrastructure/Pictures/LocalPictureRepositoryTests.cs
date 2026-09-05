#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Pictures;

[TestFixture]
public class LocalPictureRepositoryTests
{
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "EedeLocalPictureRepositoryTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    [Test]
    public void SaveAsync_ThrowsNotImplementedException()
    {
        var repository = new LocalPictureRepository();
        var dummyPicture = Picture.Create(new PictureSize(1, 1), new byte[4]);
        var filePath = new FilePath(Path.Combine(_tempDirectory, "test.png"));

        Assert.ThrowsAsync<NotImplementedException>(async () =>
        {
            await repository.SaveAsync(dummyPicture, filePath);
        });
    }

    [TestCase(".png")]
    [TestCase(".bmp")]
    [TestCase(".jpg")]
    [TestCase(".gif")]
    public void LoadAsync_WithUnsupportedExtension_ThrowsNotSupportedException(string extension)
    {
        var repository = new LocalPictureRepository();
        var filePath = new FilePath(Path.Combine(_tempDirectory, $"unsupported{extension}"));

        var ex = Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await repository.LoadAsync(filePath);
        });

        Assert.That(ex!.Message, Is.EqualTo($"Extension {extension} is not supported in Infrastructure layer yet."));
    }

    [Test]
    public void LoadAsync_WithNonExistentArvFile_ThrowsFileNotFoundException()
    {
        var repository = new LocalPictureRepository();
        var filePath = new FilePath(Path.Combine(_tempDirectory, "nonexistent.arv"));

        Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await repository.LoadAsync(filePath);
        });
    }

    [Test]
    public async Task LoadAsync_WithValidArvFile_ReturnsPicture()
    {
        var arvPath = Path.Combine(_tempDirectory, "sample.arv");
        byte[] arvBytes = CreateValidArvBytes(width: 8, height: 8);
        await File.WriteAllBytesAsync(arvPath, arvBytes);

        var repository = new LocalPictureRepository();
        var filePath = new FilePath(arvPath);

        Picture picture = await repository.LoadAsync(filePath);

        Assert.That(picture, Is.Not.Null);
        Assert.That(picture.Width, Is.EqualTo(8));
        Assert.That(picture.Height, Is.EqualTo(8));
    }

    private static byte[] CreateValidArvBytes(ushort width, ushort height)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);

        // 1. ReadHeaderFlags: 16 skipped bytes + 6 flag bytes ("IR\0\0\0\0") + 2 skipped bytes
        writer.Write(new byte[16]);
        writer.Write(new byte[] { (byte)'I', (byte)'R', 0, 0, 0, 0 });
        writer.Write(new byte[2]);

        // 2. ReadDimensions: UInt16 width, UInt16 height
        writer.Write(width);
        writer.Write(height);

        // 3. SkipBytes: 12 bytes
        writer.Write(new byte[12]);

        // 4. ReadPalette:
        // ReadImageData: flag[0] == 'I' -> imageDataLength (UInt16 = 2)
        writer.Write((ushort)2);
        // ValidatePaletteFlag: flag[1] == 'R' (already 'R' in arvHeaderFlags)
        // ReadPaletteBytes: paletteLength (byte = 98), skip 1 byte, 96 palette bytes
        writer.Write((byte)98);
        writer.Write((byte)0);
        writer.Write(new byte[96]);

        // 5. ReadBody: 4 planes, vramPlaneSize = width * height / 8
        int vramPlaneSize = width * height / 8;
        for (int plane = 0; plane < 4; plane++)
        {
            for (int i = 0; i < vramPlaneSize; i++)
            {
                // Alternating values so RLE decoder sees individual bytes
                writer.Write((byte)(i % 2 == 0 ? 0x00 : 0xFF));
            }
        }

        return ms.ToArray();
    }
}
