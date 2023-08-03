using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class RGBToneImageTransferTests
    {
        [Test]
        public void RGB値のみをコピー()
        {
            var src = new Bitmap(@"ImageTransfers\test\blend.png");
            var dst = new Bitmap(src.Width * 2, src.Height * 2);

            var transfer = new RGBToneImageTransfer();
            var picture = new Picture(src);

            using var graphics = Graphics.FromImage(dst);
            graphics.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), dst.Size));
            picture.Transfer(transfer, graphics, new Sizes.MagnifiedSize(src.Size, new Scales.Magnification(2)));

            // dst.Save(@"ImageTransfers\test\rgb.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\rgb.png");
            Assert.IsTrue(ImageComparer.Equals(dst, expected));
        }
    }
}
