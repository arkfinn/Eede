using Eede.Domain.ImageEditing;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageTransfers
{
    [TestFixture]
    public class DirectImageTransferTests
    {
        [Test]
        public void ARGB値をコピー()
        {
            Picture src = ReadPicture(@"ImageTransfers\test\blend.png");
            DirectImageTransfer transfer = new();

            Picture data = src.Transfer(transfer, new Magnification(2));

            // dst.Save(@"ImageTransfers\test\direct.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageTransfers\test\direct.png");
            Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
