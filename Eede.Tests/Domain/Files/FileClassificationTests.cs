using Eede.Domain.Files;
using NUnit.Framework;

namespace Eede.Tests.Domain.Files
{
    [TestFixture]
    public class FileClassificationTests
    {
        [TestCase("test.png", FileKind.PngImage)]
        [TestCase("photo.PNG", FileKind.PngImage)]
        [TestCase("sample.bmp", FileKind.BmpImage)]
        [TestCase("animation.arv", FileKind.ArvImage)]
        [TestCase("colors.act", FileKind.ActPalette)]
        [TestCase("advanced.aact", FileKind.AactPalette)]
        [TestCase("unknown.txt", FileKind.Unknown)]
        [TestCase("no_ext", FileKind.Unknown)]
        [TestCase("", FileKind.Unknown)]
        [TestCase(null, FileKind.Unknown)]
        [TestCase("blob:http://localhost/550e8400-e29b.png", FileKind.PngImage)]
        public void Classify_ReturnsCorrectFileKind(string? input, FileKind expected)
        {
            Assert.That(FileClassification.Classify(input), Is.EqualTo(expected));
        }

        [TestCase("test.png", true)]
        [TestCase("test.bmp", true)]
        [TestCase("test.arv", true)]
        [TestCase("test.act", false)]
        [TestCase("test.txt", false)]
        public void IsSupportedImage_ReturnsExpected(string input, bool expected)
        {
            Assert.That(FileClassification.IsSupportedImage(input), Is.EqualTo(expected));
        }

        [TestCase("test.act", true)]
        [TestCase("test.aact", true)]
        [TestCase("test.png", false)]
        public void IsSupportedPalette_ReturnsExpected(string input, bool expected)
        {
            Assert.That(FileClassification.IsSupportedPalette(input), Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class FileIdentityTests
    {
        [Test]
        public void FileIdentity_ExtractsNameAndExtension_FromLocalPath()
        {
            var identity = new FileIdentity(@"C:\Users\ark\Documents\my_dot.png");
            Assert.That(identity.Name, Is.EqualTo("my_dot.png"));
            Assert.That(identity.Extension, Is.EqualTo(".png"));
            Assert.That(identity.IsSupportedImage, Is.True);
        }

        [Test]
        public void FileIdentity_SupportsExplicitName_ForBrowserBlobUri()
        {
            var identity = new FileIdentity("blob:http://localhost:5000/guid-1234", "avatar.png");
            Assert.That(identity.Path, Is.EqualTo("blob:http://localhost:5000/guid-1234"));
            Assert.That(identity.Name, Is.EqualTo("avatar.png"));
            Assert.That(identity.Extension, Is.EqualTo(".png"));
            Assert.That(identity.IsSupportedImage, Is.True);
        }
    }
}
