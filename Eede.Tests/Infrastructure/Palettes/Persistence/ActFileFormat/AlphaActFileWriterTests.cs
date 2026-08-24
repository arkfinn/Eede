using System.IO;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence.ActFileFormat;
using NUnit.Framework;

namespace Eede.Infrastructure.Tests.Palettes.Persistence.ActFileFormat
{
    [TestFixture]
    public class AlphaActFileWriterTests
    {
        [Test]
        public void Write_ValidPalette_WritesExpectedBytes()
        {
            // Arrange
            var writer = new AlphaActFileWriter();
            var expectedBytes = new byte[1024];
            var colors = new ArgbColor[256];

            for (int i = 0; i < 256; i++)
            {
                byte red = (byte)i;
                byte green = (byte)(255 - i);
                byte blue = (byte)(i / 2);
                byte alpha = (byte)(i + 50);

                colors[i] = new ArgbColor(alpha, red, green, blue);

                // Alpha Act format writes RGBA
                expectedBytes[i * 4] = red;
                expectedBytes[i * 4 + 1] = green;
                expectedBytes[i * 4 + 2] = blue;
                expectedBytes[i * 4 + 3] = alpha;
            }

            var palette = Palette.FromColors(colors);

            using var stream = new MemoryStream();

            // Act
            writer.Write(stream, palette);

            // Assert
            var actualBytes = stream.ToArray();
            Assert.That(actualBytes.Length, Is.EqualTo(1024));
            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }
    }
}
