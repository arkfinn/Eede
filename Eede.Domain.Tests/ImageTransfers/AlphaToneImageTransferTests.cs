using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using NUnit.Framework;
using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class AlphaToneImageTransferTests
    {
        [Test]
        public void A値のみをコピー()
        {
            var src = new Bitmap(@"ImageTransfers\test\blend.png");

            var transfer = new AlphaToneImageTransfer();
            var picture = new Picture(src);

            var data = picture.Transfer(transfer, new Magnification(2));
            var dst = PictureData.CreateBitmap(data);

            // dst.Save(@"ImageTransfers\test\alpha.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\alpha.png");
            Assert.IsTrue(ImageComparer.Equals(dst, expected));
        }
    }
}
