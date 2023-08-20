using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class RGBToneImageTransferTests
    {
        [Test]
        public void RGB値のみをコピー()
        {
            var src = ReadPicture(@"ImageTransfers\test\blend.png");
            var transfer = new RGBToneImageTransfer();

            var data = src.Transfer(transfer, new Magnification(2));

            // dst.Save(@"ImageTransfers\test\rgb.png", ImageFormat.Png);
            var expected = ReadPicture(@"ImageTransfers\test\rgb.png");
            Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
