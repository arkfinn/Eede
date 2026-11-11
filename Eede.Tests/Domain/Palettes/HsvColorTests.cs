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
            var c1 = HsvColor.FromHsv(1, 2, 3);
            var c2 = HsvColor.FromHsv(1, 2, 3);
            var diffH = HsvColor.FromHsv(99, 2, 3);
            var diffS = HsvColor.FromHsv(1, 99, 3);
            var diffV = HsvColor.FromHsv(1, 2, 99);

            Assert.Multiple(() =>
            {
                Assert.That(c1.Equals(c2), Is.True);
                Assert.That(c1!.Equals(null), Is.False);
                Assert.That(c1.Equals("not an HsvColor"), Is.False);
                Assert.That(c1.Equals(diffH), Is.False);
                Assert.That(c1.Equals(diffS), Is.False);
                Assert.That(c1.Equals(diffV), Is.False);
            });
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
            var c1 = HsvColor.FromHsv(1, 2, 3);
            var c2 = HsvColor.FromHsv(1, 2, 3);
            var c3 = HsvColor.FromHsv(10, 20, 30);
            Assert.Multiple(() =>
            {
                Assert.That(c1.GetHashCode(), Is.EqualTo(c2.GetHashCode()));
                Assert.That(c1.GetHashCode(), Is.Not.EqualTo(0));
                Assert.That(c1.GetHashCode(), Is.Not.EqualTo(c3.GetHashCode()));
            });
        }

        [TestCase(0, 255, 255, 255, 0, 0)] // Red
        [TestCase(60, 255, 255, 255, 255, 0)] // Yellow
        [TestCase(120, 255, 255, 0, 255, 0)] // Green
        [TestCase(180, 255, 255, 0, 255, 255)] // Cyan
        [TestCase(240, 255, 255, 0, 0, 255)] // Blue
        [TestCase(300, 255, 255, 255, 0, 255)] // Magenta
        [TestCase(360, 255, 255, 255, 0, 0)] // Hue 360 is same as 0 (Red)
        [TestCase(0, 0, 0, 0, 0, 0)] // Black
        [TestCase(0, 0, 255, 255, 255, 255)] // White
        [TestCase(0, 0, 128, 128, 128, 128)] // Gray
        [TestCase(30, 128, 200, 200, 149, 99)] // Hi=0 Intermediate
        [TestCase(90, 128, 200, 149, 200, 99)] // Hi=1 Intermediate
        [TestCase(150, 128, 200, 99, 200, 149)] // Hi=2 Intermediate
        [TestCase(210, 128, 200, 99, 149, 200)] // Hi=3 Intermediate
        [TestCase(270, 128, 200, 149, 99, 200)] // Hi=4 Intermediate
        [TestCase(330, 128, 200, 200, 99, 149)] // Hi=5 Intermediate
        public void ToArgbColorTest(int h, int s, int v, byte expectedR, byte expectedG, byte expectedB)
        {
            var hsv = HsvColor.FromHsv(h, s, v);
            var argb = hsv.ToArgbColor();

            Assert.That(argb.Alpha, Is.EqualTo(255));
            Assert.That(argb.Red, Is.EqualTo(expectedR));
            Assert.That(argb.Green, Is.EqualTo(expectedG));
            Assert.That(argb.Blue, Is.EqualTo(expectedB));
        }

        [TestCase(255, 0, 0, 0, 255, 255)] // Red
        [TestCase(255, 255, 0, 60, 255, 255)] // Yellow
        [TestCase(0, 255, 0, 120, 255, 255)] // Green
        [TestCase(0, 255, 255, 180, 255, 255)] // Cyan
        [TestCase(0, 0, 255, 240, 255, 255)] // Blue
        [TestCase(255, 0, 255, 300, 255, 255)] // Magenta
        [TestCase(0, 0, 0, 0, 0, 0)] // Black
        [TestCase(255, 255, 255, 0, 0, 255)] // White
        [TestCase(128, 128, 128, 0, 0, 128)] // Gray
        [TestCase(200, 50, 100, 340, 191, 200)] // max=r, negative hue correction (h < 0 -> +360)
        [TestCase(100, 200, 50, 100, 191, 200)] // max=g
        [TestCase(50, 100, 200, 220, 191, 200)] // max=b
        public void FromRgbTest(int r, int g, int b, int expectedH, int expectedS, int expectedV)
        {
            var hsv = HsvColor.FromRgb(r, g, b);
            Assert.That(hsv.Hue, Is.EqualTo(expectedH));
            Assert.That(hsv.Saturation, Is.EqualTo(expectedS));
            Assert.That(hsv.Value, Is.EqualTo(expectedV));
        }
    }
}