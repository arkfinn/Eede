using Eede.Domain.Palettes;
using NUnit.Framework;

namespace Eede.Domain.Tests.Palettes
{
    [TestFixture]
    public class ArgbColorTests
    {
        [Test]
        public void LuminanceTest()
        {
            Assert.Multiple(() =>
            {
                // Black
                Assert.That(new ArgbColor(255, 0, 0, 0).Luminance, Is.EqualTo(0).Within(0.001));
                // White
                Assert.That(new ArgbColor(255, 255, 255, 255).Luminance, Is.EqualTo(1.0).Within(0.001));
                // Pure Red (0.299)
                Assert.That(new ArgbColor(255, 255, 0, 0).Luminance, Is.EqualTo(0.299).Within(0.001));
                // Pure Green (0.587)
                Assert.That(new ArgbColor(255, 0, 255, 0).Luminance, Is.EqualTo(0.587).Within(0.001));
                // Pure Blue (0.114)
                Assert.That(new ArgbColor(255, 0, 0, 255).Luminance, Is.EqualTo(0.114).Within(0.001));
            });
        }

        [Test]
        public void EqualsTest()
        {
            var color1 = new ArgbColor(255, 10, 20, 30);
            var color2 = new ArgbColor(255, 10, 20, 30);
            var color3 = new ArgbColor(128, 10, 20, 30);

            Assert.Multiple(() =>
            {
                Assert.That(color1.EqualsArgb(color2), Is.True);
                Assert.That(color1.EqualsRgb(color3), Is.True);
                Assert.That(color1.EqualsArgb(color3), Is.False);
            });
        }
    }
}
