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
            Picture b = Picture.CreateEmpty(new PictureSize(10, 10));
            PicturePushEventArgs p = new(b, new PictureArea(new Position(2, 2), new PictureSize(3, 3)));
            Picture image = p.CutOutImage();

            Assert.That(image.Size.Width, Is.EqualTo(3));
            Assert.That(image.Size.Height, Is.EqualTo(3));
            Assert.That(p.Picture, Is.EqualTo(b));
            Assert.That(p.Rect, Is.EqualTo(new PictureArea(new Position(2, 2), new PictureSize(3, 3))));
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                PicturePushEventArgs p = new(null, new PictureArea(new Position(2, 2), new PictureSize(3, 3)));
            });
        }
    }
}