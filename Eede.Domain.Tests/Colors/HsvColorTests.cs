using Eede.Domain.Colors;
using NUnit.Framework;

namespace Eede.Domain.Tests.Colors
{
    [TestFixture]
    public class HsvColorTests
    {
        [Test]
        public void EqualsTest()
        {
            Assert.AreEqual(HsvColor.FromHsv(1, 2, 3), HsvColor.FromHsv(1, 2, 3));
        }

        [Test]
        public void TestCreate()
        {
            HsvColor hsv = new(1, 2, 3);
            Assert.AreEqual(1, hsv.Hue);
            Assert.AreEqual(2, hsv.Saturation);
            Assert.AreEqual(3, hsv.Value);
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(HsvColor.FromHsv(1, 2, 3).GetHashCode(), HsvColor.FromHsv(1, 2, 3).GetHashCode());
        }
    }
}