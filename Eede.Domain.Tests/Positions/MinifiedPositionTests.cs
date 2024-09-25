using Eede.Domain.Positions;
using Eede.Domain.Scales;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.Positions
{
    [TestFixture]
    public class MinifiedPositionTests
    {
        [Test]
        public void MinifiedPositionTest()
        {
            MinifiedPosition pos = new(new Position(64, 32), new Magnification(8));
            Assert.That(pos.X, Is.EqualTo(8));
            Assert.That(pos.Y, Is.EqualTo(4));
            Assert.That(pos.ToPosition(), Is.EqualTo(new Position(8, 4)));
        }

        [Test]
        public void 引数positionはnullによる作成を許容しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                MinifiedPosition size = new(null, new Magnification(8));
            });
        }

        [Test]
        public void 引数magnificationはnullによる作成を許容しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                MinifiedPosition size = new(new Position(64, 32), null);
            });
        }
    }
}