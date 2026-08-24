using System.IO;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence.ActFileFormat;
using NUnit.Framework;

namespace Eede.Infrastructure.Tests.Palettes.Persistence.ActFileFormat
{
    [TestFixture]
    public class ActFileWriterTests
    {
        [Test]
        public void Write_ValidPalette_WritesExpectedBytes()
        {
            // Arrange
            var writer = new ActFileWriter();
            var expectedBytes = new byte[768];
            var colors = new ArgbColor[256];

            for (int i = 0; i < 256; i++)
            {
                byte alpha = 255;
                byte red = (byte)i;
                byte green = (byte)(255 - i);
                byte blue = (byte)(i / 2);

                colors[i] = new ArgbColor(alpha, red, green, blue);

                // Act format only writes RGB
                expectedBytes[i * 3] = red;
                expectedBytes[i * 3 + 1] = green;
                expectedBytes[i * 3 + 2] = blue;
            }

            var palette = Palette.FromColors(colors);

            using var stream = new MemoryStream();

            // Act
            writer.Write(stream, palette);

            // Assert
            var actualBytes = stream.ToArray();
            Assert.That(actualBytes.Length, Is.EqualTo(768));
            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }
    }
}
