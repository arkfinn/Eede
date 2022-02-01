using Eede.Domain.Pictures;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Application.Pictures.Tests
{
    [TestFixture]
    public class PicturePulledEventArgsTests
    {
        [Test]
        public void PicturePulledEventArgsTest()
        {
            var b = new Picture(new Bitmap(10, 10));
            var p = new PicturePulledEventArgs(b, new Rectangle(2, 2, 3, 3));
            var image = p.CutOutImage();

            Assert.AreEqual(image.Size, new Size(3, 3));
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var p = new PicturePulledEventArgs(null, new Rectangle(2, 2, 3, 3));
            });
        }
    }
}