using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using NUnit.Framework;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

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

            // dst.Save(@"ImageTransfers\test\alpha.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\alpha.png");
            // File.WriteAllBytes(@"ImageTransfers\test\alpha.dat", PictureData.CreateBuffer(expected).CloneImage());
            // var ex = File.ReadAllBytes(@"ImageTransfers\test\alpha.dat");
            var md5 = MD5.Create();
            Assert.That(md5.ComputeHash(data.CloneImage()), Is.EqualTo(md5.ComputeHash(PictureData.CreateBuffer(expected).CloneImage())));
        }
    }
}
