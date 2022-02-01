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
            var p = new Position(new Point(1, 2));
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

        [Test]
        public void ToPointTest1()
        {
            Assert.AreEqual(new Point(1, 2), new Position(1, 2).ToPoint());
        }

        [TestCase(true, 0, 0)]
        [TestCase(false, 1, 0)]
        [TestCase(false, 0, 1)]
        [TestCase(false, -1, 0)]
        [TestCase(false, 0, -1)]
        public void IsInnerOfTest(bool expected, int x, int y)
        {
            Assert.AreEqual(expected, new Position(x, y).IsInnerOf(new Size(1, 1)));
        }
    }
}