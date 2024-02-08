using Eede.Domain.Positions;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.Positions
{
    [TestFixture]
    public class PositionHistoryTests
    {
        [Test]
        public void Updateすると入力値がNowに入り直前の値がLastに入る()
        {
            PositionHistory h = new(new Position(1, 2));
            Assert.AreEqual(new Position(1, 2), h.Start);
            Assert.AreEqual(new Position(1, 2), h.Last);
            Assert.AreEqual(new Position(1, 2), h.Now);

            PositionHistory h2 = h.Update(new Position(3, 4));
            Assert.AreEqual(new Position(1, 2), h2.Start);
            Assert.AreEqual(new Position(1, 2), h2.Last);
            Assert.AreEqual(new Position(3, 4), h2.Now);

            PositionHistory h3 = h2.Update(new Position(5, 6));
            Assert.AreEqual(new Position(1, 2), h3.Start);
            Assert.AreEqual(new Position(3, 4), h3.Last);
            Assert.AreEqual(new Position(5, 6), h3.Now);

            PositionHistory h4 = h3.Update(new Position(7, 8));
            Assert.AreEqual(new Position(1, 2), h4.Start);
            Assert.AreEqual(new Position(5, 6), h4.Last);
            Assert.AreEqual(new Position(7, 8), h4.Now);
        }

        [Test]
        public void 引数nullによる作成を許容しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                PositionHistory h = new(null);
            });
        }
    }
}