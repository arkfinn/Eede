using NUnit.Framework;

namespace Eede.ValueObjects.Tests
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
            HsvColor hsv = new HsvColor(1, 2, 3);
            Assert.AreEqual(1, hsv.H);
            Assert.AreEqual(2, hsv.S);
            Assert.AreEqual(3, hsv.V);
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(HsvColor.FromHsv(1, 2, 3).GetHashCode(), HsvColor.FromHsv(1, 2, 3).GetHashCode());
        }
    }
}