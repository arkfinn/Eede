using Eede.Domain.Pictures;
using NUnit.Framework;
using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class DirectImageTransferTests
    {
        [Test]
        public void ARGB値をコピー()
        {
            var src = new Bitmap(@"ImageTransfers\test\blend.png");

            var transfer = new DirectImageTransfer();
            var picture = new Picture(src);

            var data = picture.Transfer(transfer, new Sizes.MagnifiedSize(src.Size, new Scales.Magnification(2)));
            var dst = PictureData.CreateBitmap(data);

            // dst.Save(@"ImageTransfers\test\direct.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\direct.png");
            Assert.IsTrue(ImageComparer.Equals(dst, expected));
        }
    }
}
