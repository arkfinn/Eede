using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Tests
{
    [TestFixture]
    public class ColorDropperTests
    {
        [Test]
        public void 引数nullによる作成を許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ColorDropper(null);
            });
        }

        [Test]
        public void DropTest()
        {
            var b = new Bitmap(10, 10);
            b.SetPixel(1, 2, Color.DarkSeaGreen);
            b.SetPixel(2, 3, Color.Beige);

            var d = new ColorDropper(b);

            Assert.AreEqual(Color.DarkSeaGreen.ToArgb(), d.Drop(new Position(1, 2)).ToArgb());
            Assert.AreEqual(Color.Beige.ToArgb(), d.Drop(new Position(2, 3)).ToArgb());
        }

        [Test]
        public void Dropの引数posはbitmapの範囲外を許容しない()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var b = new Bitmap(10, 10);
                var d = new ColorDropper(b);
                d.Drop(new Position(10, 10));
            });
        }
    }
}