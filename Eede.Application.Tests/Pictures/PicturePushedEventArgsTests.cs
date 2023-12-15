using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class PicturePushedEventArgsTests
    {
        [Test]
        public void PicturePushedEventArgsTest()
        {
            using Bitmap bmp = new(1, 1);
            Picture b = BitmapConverter.ConvertBack(bmp);
            PicturePushedEventArgs p = new(b, new Position(2, 2));
            Assert.AreEqual(b, p.Picture);
            Assert.AreEqual(new Position(2, 2), p.Position);
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                PicturePushedEventArgs h = new(null, new Position(2, 2));
            });
        }

        [Test]
        public void 引数positionについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                using Bitmap bmp = new(1, 1);
                Picture b = BitmapConverter.ConvertBack(bmp);
                PicturePushedEventArgs h = new(b, null);
            });
        }
    }
}