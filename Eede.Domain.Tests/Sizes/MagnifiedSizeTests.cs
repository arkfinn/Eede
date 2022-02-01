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
            var size = new MagnifiedSize(new Size(8, 4), new Magnification(8));
            Assert.AreEqual(64, size.Width);
            Assert.AreEqual(32, size.Height);
            Assert.AreEqual(new Size(64, 32), size.ToSize());
        }

        [Test]
        public void 引数nullによる作成を許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var size = new MagnifiedSize(new Size(0, 0), null);
            });
        }
    }
}