using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Domain.Positions
{
    [TestFixture]
    public class HalfBoxAreaTests
    {
        [Test]
        public void BoxPositionTest()
        {
            var pos = HalfBoxArea.Create(new PictureSize(16, 32), At(17, 47));
            Assert.That(Tuple.Create(pos.GridPosition, pos.RealPosition),
                Is.EqualTo(Tuple.Create(At(2, 2), At(16, 32))));
        }

        [Test]
        public void CreateRaalRectangleTest()
        {
            var pos = HalfBoxArea.Create(new PictureSize(16, 32), At(17, 47));
            var area = pos.CreateRealRectangle(new PictureSize(5, 6));
            Assert.That(Tuple.Create(area.X, area.Y, area.Width, area.Height),
                Is.EqualTo(Tuple.Create(16, 32, 5, 6)));
        }

        static public TestCaseData[] UpdatePositionTestSource = new[]{
            new TestCaseData(At(17, 47), At(25, 47)).Returns(new int[] { 16, 40, 24, 16 }).SetName("X + 8"),
            new TestCaseData(At(17, 47), At(17, 55)).Returns(new int[] { 16, 40, 16, 24 }).SetName("Y + 8"),
            new TestCaseData(At(17, 47), At(16, 47)).Returns(new int[] { 16, 40, 16, 16 }).SetName("X - 0"),
            new TestCaseData(At(17, 47), At(15, 47)).Returns(new int[] { 8, 40, 24, 16 }).SetName("X - 1"),
            new TestCaseData(At(17, 47), At(8, 47)).Returns(new int[] { 8, 40, 24, 16 }).SetName("X - 8"),
            new TestCaseData(At(17, 47), At(7, 47)).Returns(new int[] { 0, 40, 32, 16 }).SetName("X - 16"),
            new TestCaseData(At(17, 47), At(17, 39)).Returns(new int[] { 16, 32, 16, 24 }).SetName("Y - 1"),
            new TestCaseData(At(17, 47), At(17, 32)).Returns(new int[] { 16, 32, 16, 24 }).SetName("Y - 8"),
            new TestCaseData(At(17, 47), At(17, 31)).Returns(new int[] { 16, 24, 16, 32 }).SetName("Y - 16"),
            new TestCaseData(At(17, 47), At(-1, -1)).Returns(new int[] { 0, 0, 32, 56 }).SetName("min 0"),
            new TestCaseData(At(17, 47), At(-9, -9)).Returns(new int[] { 0, 0, 32, 56 }).SetName("min -9"),
            new TestCaseData(At(17, 47), At(120, 120)).Returns(new int[] { 16, 40, 84, 60 }).SetName("max")
        };

        [TestCaseSource(nameof(UpdatePositionTestSource))]
        public int[] UpdatePositionTest(Position startPos, Position updatePos)
        {
            var pos = HalfBoxArea.Create(new PictureSize(16, 16), startPos);
            var updated = pos.UpdatePosition(updatePos, new PictureSize(100, 100));
            return new int[] { updated.RealPosition.X, updated.RealPosition.Y, updated.BoxSize.Width, updated.BoxSize.Height };
        }

        private static Position At(int x, int y)
        {
            return new Position(x, y);
        }

        private static PictureSize SizeOf(int width, int height)
        {
            return new PictureSize(width, height);
        }
    }
}