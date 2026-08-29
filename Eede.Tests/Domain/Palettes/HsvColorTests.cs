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
        public void FromRgbTest(int r, int g, int b, int expectedH, int expectedS, int expectedV)
        {
            var hsv = HsvColor.FromRgb(r, g, b);
            Assert.That(hsv.Hue, Is.EqualTo(expectedH));
            Assert.That(hsv.Saturation, Is.EqualTo(expectedS));
            Assert.That(hsv.Value, Is.EqualTo(expectedV));
        }
    }
}