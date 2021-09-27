using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Application.Pictures.Tests
{
    [TestFixture]
    public class PicturePushedEventArgsTests
    {
        [Test]
        public void PicturePushedEventArgsTest()
        {
            var b = new Picture(new Bitmap(1, 1));
            var p = new PicturePushedEventArgs(b, new Position(2, 2));
            Assert.AreEqual(b, p.Picture);
            Assert.AreEqual(new Position(2, 2), p.Position);
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var h = new PicturePushedEventArgs(null, new Position(2, 2));
            });
        }

        [Test]
        public void 引数positionについてnullによる作成を許可しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var b = new Picture(new Bitmap(1, 1));
                var h = new PicturePushedEventArgs(b, null);
            });
        }
    }
}