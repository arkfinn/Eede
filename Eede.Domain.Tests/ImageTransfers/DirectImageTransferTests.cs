﻿using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.ImageTransfers
{
    [TestFixture]
    public class DirectImageTransferTests
    {
        [Test]
        public void ARGB値をコピー()
        {
            var src = ReadPicture(@"ImageTransfers\test\blend.png");
            var transfer = new DirectImageTransfer();

            var data = src.Transfer(transfer, new Magnification(2));

            // dst.Save(@"ImageTransfers\test\direct.png", ImageFormat.Png);
            var expected = ReadPicture(@"ImageTransfers\test\direct.png");
            Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
