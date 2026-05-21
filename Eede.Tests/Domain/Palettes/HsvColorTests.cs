using Eede.Domain.Palettes;
using NUnit.Framework;

namespace Eede.Domain.Tests.Palettes
{
    [TestFixture]
    public class HsvColorTests
    {
        [Test]
        public void EqualsTest()
        {
            Assert.That(HsvColor.FromHsv(1, 2, 3), Is.EqualTo(HsvColor.FromHsv(1, 2, 3)));
        }

        [Test]
        public void TestCreate()
        {
            HsvColor hsv = new(1, 2, 3);
            Assert.That(hsv.Hue, Is.EqualTo(1));
            Assert.That(hsv.Saturation, Is.EqualTo(2));
            Assert.That(hsv.Value, Is.EqualTo(3));
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.That(HsvColor.FromHsv(1, 2, 3).GetHashCode(), Is.EqualTo(HsvColor.FromHsv(1, 2, 3).GetHashCode()));
        }
    }
}