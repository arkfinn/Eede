using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageTransfers
{
    [TestFixture]
    public class AlphaToneImageTransferTests
    {
        [Test]
        public void A値のみをコピー()
        {
            Picture src = ReadPicture(@"ImageTransfers\test\blend.png");
            AlphaToneImageTransfer transfer = new();

            Picture data = src.Transfer(transfer, new Magnification(2));

            // dst.Save(@"ImageTransfers\test\alpha.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageTransfers\test\alpha.png");
            Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
