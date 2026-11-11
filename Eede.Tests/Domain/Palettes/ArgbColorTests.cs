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

                // IEquatable<ArgbColor>.Equals
                Assert.That(color1.Equals(color2), Is.True);
                Assert.That(color1 == color2, Is.True);
                Assert.That(color1 != color3, Is.True);

                // 個別コンポーネントが異なる場合の不一致検証 (Logical mutation 撃破)
                var diffAlpha = new ArgbColor(0, 10, 20, 30);
                var diffRed = new ArgbColor(255, 99, 20, 30);
                var diffGreen = new ArgbColor(255, 10, 99, 30);
                var diffBlue = new ArgbColor(255, 10, 20, 99);

                Assert.That(color1.Equals(diffAlpha), Is.False);
                Assert.That(color1.Equals(diffRed), Is.False);
                Assert.That(color1.Equals(diffGreen), Is.False);
                Assert.That(color1.Equals(diffBlue), Is.False);

                // EqualsRgb と EqualsArgb の各チャンネル不一致検証 (Logical mutation 撃破)
                Assert.That(color1.EqualsRgb(diffRed), Is.False);
                Assert.That(color1.EqualsRgb(diffGreen), Is.False);
                Assert.That(color1.EqualsRgb(diffBlue), Is.False);
                Assert.That(color1.EqualsRgb(diffAlpha), Is.True);

                Assert.That(color1.EqualsArgb(diffRed), Is.False);
                Assert.That(color1.EqualsArgb(diffGreen), Is.False);
                Assert.That(color1.EqualsArgb(diffBlue), Is.False);
                Assert.That(color1.EqualsArgb(diffAlpha), Is.False);
            });
        }
    }
}
