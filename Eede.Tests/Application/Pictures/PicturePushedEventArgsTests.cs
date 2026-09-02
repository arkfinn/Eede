using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class PicturePushedEventArgsTests
    {
        [Test]
        public void PicturePushedEventArgsTest()
        {
            Picture b = Picture.CreateEmpty(new PictureSize(1, 1));
            PicturePushEventArgs p = new(b, new Position(2, 2));
            Assert.That(p.Picture, Is.EqualTo(b));
            Assert.That(p.Position, Is.EqualTo(new Position(2, 2)));
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                PicturePushEventArgs h = new(null!, new Position(2, 2));
            });
        }
    }
}