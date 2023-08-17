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
            using var bmp = new Bitmap(10, 10);
            var b = new Picture(bmp);
            var p = new PicturePulledEventArgs(b, new Rectangle(2, 2, 3, 3));
            var image = p.CutOutImage();

            Assert.That(Tuple.Create(image.Size.Width, image.Size.Height), Is.EqualTo(Tuple.Create(3, 3)));
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