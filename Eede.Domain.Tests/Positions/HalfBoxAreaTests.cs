using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.Positions
{
    [TestFixture]
    public class HalfBoxAreaTests
    {
        [Test]
        public void BoxPositionTest()
        {
            HalfBoxArea pos = HalfBoxArea.Create(new PictureSize(16, 32), At(17, 47));
            Assert.That(Tuple.Create(pos.GridPosition, pos.RealPosition),
                Is.EqualTo(Tuple.Create(At(2, 2), At(16, 32))));
        }

        [Test]
        public void CreateRealRectangleTest()
        {
            HalfBoxArea pos = HalfBoxArea.Create(new PictureSize(16, 32), At(17, 47));
            PictureArea area = pos.CreateRealArea(new PictureSize(5, 6));
            Assert.That(Tuple.Create(area.X, area.Y, area.Width, area.Height),
                Is.EqualTo(Tuple.Create(16, 32, 5, 6)));
        }

        [TestCaseSource(nameof(UpdatePositionTestSource))]
        public void UpdatePositionTest(Position startPos, Position updatePos, int[] expected)
        {
            HalfBoxArea pos = HalfBoxArea.Create(new PictureSize(16, 16), startPos);
            HalfBoxArea updated = pos.UpdatePosition(updatePos, new PictureSize(100, 100));

            Assert.That(new int[] { updated.RealPosition.X, updated.RealPosition.Y, updated.BoxSize.Width, updated.BoxSize.Height },
                Is.EqualTo(expected));
        }

        private static readonly
        object[] UpdatePositionTestSource = {
            new object[]{ At(17, 47), At(25, 47), new int[] { 16, 40, 24, 16 } },
            new object[]{ At(17, 47), At(17, 55), new int[] { 16, 40, 16, 24 } },
            new object[]{ At(17, 47), At(16, 47), new int[] { 16, 40, 16, 16 } },
            new object[]{ At(17, 47), At(15, 47), new int[] { 8, 40, 24, 16 } },
            new object[]{ At(17, 47), At(8, 47), new int[] { 8, 40, 24, 16 } },
            new object[]{ At(17, 47), At(7, 47), new int[] { 0, 40, 32, 16 } },
            new object[]{ At(17, 47), At(17, 39), new int[] { 16, 32, 16, 24 } },
            new object[]{ At(17, 47), At(17, 32), new int[] { 16, 32, 16, 24 } },
            new object[]{ At(17, 47), At(17, 31), new int[] { 16, 24, 16, 32 } },
            new object[]{ At(17, 47), At(0, 0), new int[] { 0, 0, 32, 56 } },
            new object[]{ At(17, 47), At(-1, -1), new int[] { 0, 0, 32, 56 } },
            new object[]{ At(17, 47), At(-9, -9), new int[] { 0, 0, 32, 56 } },
            new object[]{ At(17, 47), At(120, 120), new int[] { 16, 40, 120, 96 } },
            new object[]{ At(99, 99), At(0, 0), new int[] { 0, 0, 112, 112 } },
        };

        private static Position At(int x, int y)
        {
            return new Position(x, y);
        }
    }
}
