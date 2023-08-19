using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using NUnit.Framework;
using System.Drawing;
using System.Security.Cryptography;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class DirectImageTransferTests
    {
        [Test]
        public void ARGB値をコピー()
        {
            using var src = new Bitmap(@"ImageTransfers\test\blend.png");

            var transfer = new DirectImageTransfer();
            var picture = new Picture(PictureData.CreateBuffer(src));

            var data = picture.Transfer(transfer, new Magnification(2));
     
            // dst.Save(@"ImageTransfers\test\direct.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\direct.png");
            var md5 = MD5.Create();
            Assert.That(md5.ComputeHash(data.CloneImage()), Is.EqualTo(md5.ComputeHash(PictureData.CreateBuffer(expected).CloneImage())));
        }
    }
}
