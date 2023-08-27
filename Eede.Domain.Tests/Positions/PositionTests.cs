using Eede.Domain.Pictures;
using NUnit.Framework;
using System.Drawing;

namespace Eede.Domain.Positions.Tests
{
    [TestFixture]
    public class PositionTests
    {
        [Test]
        public void PositionTest()
        {
            var p = new Position(1, 2);
            Assert.AreEqual(1, p.X);
            Assert.AreEqual(2, p.Y);
        }

        [Test]
        public void PositionCreateFromPoint()
        {
            var p = new Position(1, 2);
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