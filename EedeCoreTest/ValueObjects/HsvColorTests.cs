using Eede.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eede.ValueObjects.Tests
{
    [TestClass()]
    public class HsvColorTests
    {
        [TestMethod()]
        public void EqualsTest()
        {
            Assert.AreEqual(HsvColor.FromHsv(1, 2, 3), HsvColor.FromHsv(1, 2, 3));
        }

        [TestMethod]
        public void TestCreate()
        {
            HsvColor hsv = new HsvColor(1, 2, 3);
            Assert.AreEqual(1, hsv.H);
            Assert.AreEqual(2, hsv.S);
            Assert.AreEqual(3, hsv.V);
        }

        [TestMethod()]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(HsvColor.FromHsv(1, 2, 3).GetHashCode(), HsvColor.FromHsv(1, 2, 3).GetHashCode());
        }
    }
}
