using Eede.Domain.Colors;
using NUnit.Framework;
using System.IO;

namespace Eede.Domain.Tests.Colors;

[TestFixture]
public class ActFileReaderTests
{
    [Test]
    public void Read_ShouldReturnCorrectPalette_WhenValidActFileStreamProvided()
    {
        // Arrange
        byte[] mockActFile = new byte[768]; // RGB 256色、各色3バイト
        for (int i = 0; i < 256; i++)
        {
            mockActFile[i * 3] = (byte)i;      // R
            mockActFile[(i * 3) + 1] = (byte)(255 - i);  // G
            mockActFile[(i * 3) + 2] = (byte)(i / 2);    // B
        }

        using MemoryStream memoryStream = new(mockActFile);
        ActFileReader actFileReader = new();

        // Act
        Palette result = actFileReader.Read(memoryStream);

        // Assert
        for (int i = 0; i < 256; i++)
        {
            ArgbColor color = result.Fetch(i);
            Assert.That(color.Alpha, Is.EqualTo(255));             // Alphaは常に255
            Assert.That(color.Red, Is.EqualTo((byte)i));         // Rの値を確認
            Assert.That(color.Green, Is.EqualTo((byte)(255 - i))); // Gの値を確認
            Assert.That(color.Blue, Is.EqualTo((byte)(i / 2)));   // Bの値を確認
        }
    }

    [Test]
    public void Read_ShouldThrowException_WhenFileStreamIsTooShort()
    {
        // Arrange
        byte[] invalidFile = new byte[500]; // 正常なファイルよりも短いデータ
        using MemoryStream memoryStream = new(invalidFile);
        ActFileReader actFileReader = new();

        // Act & Assert
        _ = Assert.Throws<EndOfStreamException>(() => actFileReader.Read(memoryStream));
    }
}

