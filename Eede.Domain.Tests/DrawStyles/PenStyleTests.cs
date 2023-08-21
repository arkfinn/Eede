using Eede.Domain.Colors;
using Eede.Domain.ImageBlenders;
using NUnit.Framework;
using System;

namespace Eede.Domain.DrawStyles.Tests
{
    [TestFixture()]
    public class PenStyleTests
    {
        [Test()]
        public void 正常生成()
        {
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            Assert.That(Tuple.Create(
                penStyle.Color.Alpha,
                penStyle.Color.Red,
                penStyle.Color.Green,
                penStyle.Color.Blue,
                penStyle.Width),
                Is.EqualTo(Tuple.Create(255, 0, 0, 0, 1)));
        }

        [Test()]
        public void Blenderはnullを許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new PenStyle(null, new ArgbColor(255, 0, 0, 0), 1);
            });
        }

        [Test()]
        public void Widthは1以上でなければならない()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 0);
            });
        }
    }
}