using Eede.Domain.Sizes;
using Eede.Domain.Scales;
using Eede.Domain.Pictures;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.Sizes
{
    [TestFixture]
    public class MagnifiedSizeTests
    {
        [Test]
        public void MagnifiedSizeTest()
        {
            MagnifiedSize size = new(new PictureSize(8, 4), new Magnification(8));
            Assert.That(new int[] { size.Width, size.Height }, Is.EqualTo(new int[] { 64, 32 }));
        }

        [Test]
        public void 引数nullによる作成を許容しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                MagnifiedSize size = new(new PictureSize(0, 0), null);
            });
        }
    }
}