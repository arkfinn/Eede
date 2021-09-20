using Eede.Positions;
using NUnit.Framework;
using System.Drawing;

namespace Eede.Tests
{
    [TestFixture]
    public class HalfBoxPositionTests
    {
        [Test]
        public void BoxPositionTest()
        {
            var pos = new HalfBoxPosition(new Size(16, 32), new Point(17, 47));
            Assert.AreEqual(new Position(2, 2), pos.Position);
            Assert.AreEqual(new Position(16, 32), pos.RealPosition);
        }

        [Test]
        public void CreateRaalRectangleTest()
        {
            var pos = new HalfBoxPosition(new Size(16, 32), new Point(17, 47));
            var rect = pos.CreateRealRectangle(new Size(5, 6));
            Assert.AreEqual(new Point(16, 32), rect.Location);
            Assert.AreEqual(new Size(5, 6), rect.Size);
        }
    }
}