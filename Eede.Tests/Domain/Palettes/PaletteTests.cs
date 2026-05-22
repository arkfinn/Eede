using System;
using Eede.Domain.Palettes;
using NUnit.Framework;

namespace Eede.Domain.Tests.Palettes
{
    [TestFixture]
    public class PaletteTests
    {
        [Test]
        public void Fetch_ReturnsCorrectColor()
        {
            var palette = Palette.Create();
            var expectedColor = new ArgbColor(0, 0, 0, 0);

            var color = palette.Fetch(0);

            Assert.That(color.EqualsArgb(expectedColor), Is.True);
        }

        [Test]
        public void Fetch_OutOfBounds_ThrowsException()
        {
            var palette = Palette.Create();

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => palette.Fetch(-1));
                Assert.Throws<ArgumentOutOfRangeException>(() => palette.Fetch(Palette.MAX_LENGTH));
            });
        }

        [Test]
        public void Apply_ReturnsNewPaletteWithUpdatedColor()
        {
            var palette = Palette.Create();
            var newColor = new ArgbColor(255, 255, 0, 0);

            var updatedPalette = palette.Apply(10, newColor);

            Assert.Multiple(() =>
            {
                Assert.That(updatedPalette, Is.Not.SameAs(palette));
                Assert.That(updatedPalette.Fetch(10).EqualsArgb(newColor), Is.True);
                Assert.That(palette.Fetch(10).EqualsArgb(new ArgbColor(0, 0, 0, 0)), Is.True);
            });
        }

        [Test]
        public void Apply_OutOfBounds_ThrowsException()
        {
            var palette = Palette.Create();
            var newColor = new ArgbColor(255, 255, 0, 0);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => palette.Apply(-1, newColor));
                Assert.Throws<ArgumentOutOfRangeException>(() => palette.Apply(Palette.MAX_LENGTH, newColor));
            });
        }
    }
}
