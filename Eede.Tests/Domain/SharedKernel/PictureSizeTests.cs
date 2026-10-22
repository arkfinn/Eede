using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.SharedKernel
{
    public class PictureSizeTests
    {
        [TestCase(true, 0, 0)]
        [TestCase(false, 1, 0)]
        [TestCase(false, 0, 1)]
        [TestCase(false, -1, 0)]
        [TestCase(false, 0, -1)]
        public void ContainsTest(bool expected, int x, int y)
        {
            Assert.That(new PictureSize(1, 1).Contains(new Position(x, y)), Is.EqualTo(expected));
        }

        [TestCase(16385, 10)]
        [TestCase(10, 16385)]
        [TestCase(-1, 10)]
        [TestCase(10, -1)]
        public void Constructor_ThrowsException_WhenDimensionIsOutOfRange(int width, int height)
        {
            Assert.Throws<ArgumentException>(() => new PictureSize(width, height));
        }
    }
}
