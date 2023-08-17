﻿using Eede.Domain.Pictures;
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
            using var bmp = new Bitmap(1, 1);
            var b = new Picture(bmp);
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
                using var bmp = new Bitmap(1, 1);
                var b = new Picture(bmp);
                var h = new PicturePushedEventArgs(b, null);
            });
        }
    }
}