using Eede.Domain.Pictures;
using NUnit.Framework;
using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class RGBToneImageTransferTests
    {
        [Test]
        public void RGB値のみをコピー()
        {
            var src = new Bitmap(@"ImageTransfers\test\blend.png");

            var transfer = new RGBToneImageTransfer();
            var picture = new Picture(src);

            var data = picture.Transfer(transfer, new Sizes.MagnifiedSize(src.Size, new Scales.Magnification(2)));
            var dst = PictureData.CreateBitmap(data);

            // dst.Save(@"ImageTransfers\test\rgb.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\rgb.png");
            Assert.IsTrue(ImageComparer.Equals(dst, expected));
        }
    }
}
