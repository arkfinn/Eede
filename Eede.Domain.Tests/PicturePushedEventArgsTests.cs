using Eede.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Tests
{
    [TestFixture]
    public class PicturePushedEventArgsTests
    {
        [Test]
        public void PicturePushedEventArgsTest()
        {
            var b = new Bitmap(1, 1);
            var p = new PicturePushedEventArgs(b, new Position(2, 2));
            Assert.AreEqual(b, p.Bitmap);
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
                var h = new PicturePushedEventArgs(new Bitmap(1, 1), null);
            });
        }
    }
}