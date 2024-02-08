using Eede.Domain.Positions;
using NUnit.Framework;

namespace Eede.Domain.Tests.Positions
{
    [TestFixture]
    public class PositionTests
    {
        [Test]
        public void PositionTest()
        {
            Position p = new(1, 2);
            Assert.AreEqual(1, p.X);
            Assert.AreEqual(2, p.Y);
        }

        [Test]
        public void PositionCreateFromPoint()
        {
            Position p = new(1, 2);
            Assert.AreEqual(1, p.X);
            Assert.AreEqual(2, p.Y);
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(new Position(1, 2), new Position(1, 2));
        }

        [Test]
        public void ToPointTest()
        {
            Assert.AreEqual(new Position(1, 2).GetHashCode(), new Position(1, 2).GetHashCode());
        }
    }
}