﻿using Eede.Domain.ImageTransfers;
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
    public class AlphaToneImageTransferTests
    {
        [Test]
        public void A値のみをコピー()
        {
            var src = new Bitmap(@"ImageTransfers\test\blend.png");
            var dst = new Bitmap(src.Width, src.Height);

            var transfer = new AlphaToneImageTransfer();
            var picture = new Picture(src);

            using var graphics = Graphics.FromImage(dst);
            graphics.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), dst.Size));
            picture.Transfer(transfer, graphics);

            // dst.Save(@"ImageTransfers\test\alpha.png", ImageFormat.Png);
            var expected = new Bitmap(@"ImageTransfers\test\alpha.png");
            Assert.IsTrue(ImageComparer.Equals(dst, expected));
        }
    }
}
