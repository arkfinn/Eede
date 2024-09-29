using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageTransfers
{
    [TestFixture]
    public class RGBToneImageTransferTests
    {
        [Test]
        public void RGB値のみをコピー()
        {
            Picture src = ReadPicture(@"ImageTransfers\test\blend.png");
            RGBToneImageTransfer transfer = new();

            Picture data = src.Transfer(transfer, new Magnification(2));

            // dst.Save(@"ImageTransfers\test\rgb.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageTransfers\test\rgb.png");
            Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
