using NUnit.Framework;
using System;

namespace Eede.Positions.Tests
{
    [TestFixture]
    public class MinifiedPositionTests
    {
        [Test]
        public void MinifiedPositionTest()
        {
            var pos = new MinifiedPosition(new Position(64, 32), new Magnification(8));
            Assert.AreEqual(8, pos.X);
            Assert.AreEqual(4, pos.Y);
            Assert.AreEqual(new Position(8, 4), pos.ToPosition());
        }

        [Test]
        public void 引数positionはnullによる作成を許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var size = new MinifiedPosition(null, new Magnification(8));
            });
        }

        [Test]
        public void 引数magnificationはnullによる作成を許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var size = new MinifiedPosition(new Position(64, 32), null);
            });
        }
    }
}