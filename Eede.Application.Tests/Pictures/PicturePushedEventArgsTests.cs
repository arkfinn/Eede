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
            Picture b = Picture.CreateEmpty(new PictureSize(1, 1));
            PicturePullEventArgs p = new(b, new Position(2, 2));
            Assert.That(p.Picture, Is.EqualTo(b));
            Assert.That(p.Position, Is.EqualTo(new Position(2, 2)));
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                PicturePullEventArgs h = new(null, new Position(2, 2));
            });
        }

        [Test]
        public void 引数positionについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                Picture b = Picture.CreateEmpty(new PictureSize(1, 1));
                PicturePullEventArgs h = new(b, null);
            });
        }
    }
}