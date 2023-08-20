using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class AlphaToneImageTransferTests
    {
        [Test]
        public void A値のみをコピー()
        {
            var src = ReadPicture(@"ImageTransfers\test\blend.png");
            var transfer = new AlphaToneImageTransfer();

            var data = src.Transfer(transfer, new Magnification(2));

            // dst.Save(@"ImageTransfers\test\alpha.png", ImageFormat.Png);
            var expected = ReadPicture(@"ImageTransfers\test\alpha.png");
            Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
