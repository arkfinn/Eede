using Eede.Domain.Pictures;
using Eede.Domain.Scales;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Domain.Sizes.Tests
{
    [TestFixture]
    public class MagnifiedSizeTests
    {
        [Test]
        public void MagnifiedSizeTest()
        {
            var size = new MagnifiedSize(new PictureSize(8, 4), new Magnification(8));
            Assert.That(new int[] { size.Width, size.Height }, Is.EqualTo(new int[] { 64, 32 }));
        }

        [Test]
        public void 引数nullによる作成を許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var size = new MagnifiedSize(new PictureSize(0, 0), null);
            });
        }
    }
}