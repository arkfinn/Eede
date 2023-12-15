using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;

namespace Eede.Domain.Tests.Pictures
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
    }
}
