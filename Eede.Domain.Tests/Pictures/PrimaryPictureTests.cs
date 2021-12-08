using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Domain.Pictures.Tests
{
    [TestFixture]
    public class PrimaryPictureTests
    {
        [Test]
        public void 引数imageがnullでnewはできない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var p = new Picture(null);
            });
        }

        [Test]
        public void PickColorTest()
        {
            using (var b = new Bitmap(10, 10))
            {
                b.SetPixel(1, 2, Color.DarkSeaGreen);
                b.SetPixel(2, 3, Color.Beige);
                var d = new Picture(b);
                Assert.AreEqual(Color.DarkSeaGreen.ToArgb(), d.PickColor(new Position(1, 2)).ToArgb());
                Assert.AreEqual(Color.Beige.ToArgb(), d.PickColor(new Position(2, 3)).ToArgb());
            }
        }

        [Test]
        public void PickColorの引数posはbitmapの範囲外を許容しない()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (var b = new Bitmap(10, 10))
                {
                    var d = new Picture(b);
                    d.PickColor(new Position(10, 10));
                }
            });
        }
    }
}