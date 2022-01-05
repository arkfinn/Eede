using Eede.Domain.Positions;
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

        static public TestCaseData[] UpdatePositionTestSource = new[]{
            new TestCaseData(new Point(17, 47), new Point(25, 47)).Returns(new Rectangle(new Point(16, 40), new Size(24, 16))).SetName("X + 8"),
            new TestCaseData(new Point(17, 47), new Point(17, 55)).Returns(new Rectangle(new Point(16, 40),new Size(16, 24))).SetName("Y + 8"),
            new TestCaseData(new Point(17, 47), new Point(16, 47)).Returns(new Rectangle(new Point(16, 40),new Size(16, 16))).SetName("X - 0"),
            new TestCaseData(new Point(17, 47), new Point(15, 47)).Returns(new Rectangle(new Point(8, 40),new Size(24, 16))).SetName("X - 1"),
            new TestCaseData(new Point(17, 47), new Point(9, 47)).Returns(new Rectangle(new Point(8, 40),new Size(24, 16))).SetName("X - 8"),
            new TestCaseData(new Point(17, 47), new Point(8, 47)).Returns(new Rectangle(new Point(8, 40),new Size(24, 16))).SetName("X - 16"),
            new TestCaseData(new Point(17, 47), new Point(17, 39)).Returns(new Rectangle(new Point(16, 32),new Size(16, 24))).SetName("Y - 8")
        };

        [TestCaseSource(nameof(UpdatePositionTestSource))]
        public Rectangle UpdatePositionTest(Point startPos, Point updatePos)
        {
            var pos = new HalfBoxPosition(new Size(16, 16), startPos);
            var updated = pos.UpdatePosition(updatePos);
            return new Rectangle(updated.RealPosition.ToPoint(), updated.BoxSize);
        }
    }
}