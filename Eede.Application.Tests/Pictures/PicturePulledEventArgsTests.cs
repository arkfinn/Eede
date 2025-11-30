using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class PicturePulledEventArgsTests
    {
        [Test]
        public void PicturePulledEventArgsTest()
        {
            Picture b = Picture.CreateEmpty(new PictureSize(10, 10));
            PicturePushEventArgs p = new(b, new PictureArea(new Position(2, 2), new PictureSize(3, 3)));
            Picture image = p.CutOutImage();

            Assert.That(Tuple.Create(image.Size.Width, image.Size.Height), Is.EqualTo(Tuple.Create(3, 3)));
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