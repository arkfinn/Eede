using Eede.Domain.ImageBlenders;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Domain.DrawStyles.Tests
{
    [TestFixture()]
    public class PenStyleTests
    {
        [Test()]
        public void 正常生成()
        {
            var penStyle = new PenStyle(new DirectImageBlender(), Color.Black, 1);
            Assert.AreEqual(Color.Black, penStyle.Color);
            Assert.AreEqual(1, penStyle.Width);
        }

        [Test()]
        public void Blenderはnullを許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new PenStyle(null, Color.Black, 1);
            });
        }

        [Test()]
        public void Widthは1以上でなければならない()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new PenStyle(new DirectImageBlender(), Color.Black, 0);
            });
        }
    }
}