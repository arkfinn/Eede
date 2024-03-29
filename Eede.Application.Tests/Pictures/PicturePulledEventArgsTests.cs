﻿using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class PicturePulledEventArgsTests
    {
        [Test]
        public void PicturePulledEventArgsTest()
        {
            using Bitmap bmp = new(10, 10);
            Picture b = BitmapConverter.ConvertBack(bmp);
            PicturePushEventArgs p = new(b, new PictureArea(new Domain.Positions.Position(2, 2), new PictureSize(3, 3)));
            Picture image = p.CutOutImage();

            Assert.That(Tuple.Create(image.Size.Width, image.Size.Height), Is.EqualTo(Tuple.Create(3, 3)));
        }

        [Test]
        public void 引数graphicsについてnullによる作成を許可しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                PicturePushEventArgs p = new(null, new PictureArea(new Domain.Positions.Position(2, 2), new PictureSize(3, 3)));
            });
        }
    }
}