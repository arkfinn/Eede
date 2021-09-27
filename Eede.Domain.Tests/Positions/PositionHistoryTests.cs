using NUnit.Framework;
using System;

namespace Eede.Domain.Positions.Tests
{
    [TestFixture]
    public class PositionHistoryTests
    {
        [Test]
        public void Updateすると入力値がNowに入り直前の値がLastに入る()
        {
            var h = new PositionHistory(new Position(1, 2));
            Assert.AreEqual(new Position(1, 2), h.Start);
            Assert.AreEqual(new Position(1, 2), h.Last);
            Assert.AreEqual(new Position(1, 2), h.Now);

            var h2 = h.Update(new Position(3, 4));
            Assert.AreEqual(new Position(1, 2), h2.Start);
            Assert.AreEqual(new Position(1, 2), h2.Last);
            Assert.AreEqual(new Position(3, 4), h2.Now);

            var h3 = h2.Update(new Position(5, 6));
            Assert.AreEqual(new Position(1, 2), h3.Start);
            Assert.AreEqual(new Position(3, 4), h3.Last);
            Assert.AreEqual(new Position(5, 6), h3.Now);

            var h4 = h3.Update(new Position(7, 8));
            Assert.AreEqual(new Position(1, 2), h4.Start);
            Assert.AreEqual(new Position(5, 6), h4.Last);
            Assert.AreEqual(new Position(7, 8), h4.Now);
        }

        [Test]
        public void 引数nullによる作成を許容しない()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var h = new PositionHistory(null);
            });
        }
    }
}