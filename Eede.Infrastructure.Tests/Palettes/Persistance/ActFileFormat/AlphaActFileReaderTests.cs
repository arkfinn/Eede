using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence.ActFileFormat;
using NUnit.Framework;
using System.IO;

namespace Eede.Infrastructure.Tests.Palettes.Persistance.ActFileFormat;

internal class AlphaActFileReaderTests
{
    [TestFixture]
    public class PaletteReaderTests
    {
        [Test]
        public void Read_ShouldReturnCorrectPalette_WhenValidPaletteFileProvided()
        {
            // Arrange: RGBA形式の256色分のバイナリデータを用意
            byte[] mockPaletteFile = new byte[1024]; // RGBA 256色、各色4バイト
            for (int i = 0; i < 256; i++)
            {
                mockPaletteFile[i * 4] = (byte)i;        // R
                mockPaletteFile[i * 4 + 1] = (byte)(255 - i);  // G
                mockPaletteFile[i * 4 + 2] = (byte)(i / 2);    // B
                mockPaletteFile[i * 4 + 3] = (byte)(i + 50);   // A
            }

            using MemoryStream memoryStream = new(mockPaletteFile);
            AlphaActFileReader paletteReader = new(); // クラス名に応じて変更

            // Act: メソッドを実行してパレットを読み込む
            Palette result = paletteReader.Read(memoryStream);

            // Assert: ファイルの中身を確認
            // 各色の値を確認
            for (int i = 0; i < 256; i++)
            {
                ArgbColor color = result.Fetch(i);
                Assert.That(color.Red, Is.EqualTo((byte)i));         // Rの値
                Assert.That(color.Green, Is.EqualTo((byte)(255 - i))); // Gの値
                Assert.That(color.Blue, Is.EqualTo((byte)(i / 2)));   // Bの値
                Assert.That(color.Alpha, Is.EqualTo((byte)(i + 50)));  // Aの値
            }
        }

        [Test]
        public void Read_ShouldThrowException_WhenFileStreamIsTooShort()
        {
            // Arrange: 不完全なパレットデータを準備
            byte[] invalidPaletteFile = new byte[1000]; // 必要なバイト数よりも少ない
            using MemoryStream memoryStream = new(invalidPaletteFile);
            AlphaActFileReader paletteReader = new(); // クラス名に応じて変更

            // Act & Assert: EndOfStreamExceptionがスローされることを確認
            Assert.That(() => paletteReader.Read(memoryStream), Throws.InstanceOf<EndOfStreamException>());
        }
    }
}
