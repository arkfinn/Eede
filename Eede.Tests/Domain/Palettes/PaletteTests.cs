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

        [Test]
        public void FromColors_WithInvalidLength_ThrowsInvalidOperationException()
        {
            var tooFew = new ArgbColor[Palette.MAX_LENGTH - 1];
            var tooMany = new ArgbColor[Palette.MAX_LENGTH + 1];

            Assert.Multiple(() =>
            {
                Assert.Throws<InvalidOperationException>(() => Palette.FromColors(tooFew));
                Assert.Throws<InvalidOperationException>(() => Palette.FromColors(tooMany));
            });
        }

        [Test]
        public void Create_InitializesAll256ColorsToTransparent()
        {
            var palette = Palette.Create();
            var transparent = new ArgbColor(0, 0, 0, 0);

            for (int i = 0; i < Palette.MAX_LENGTH; i++)
            {
                Assert.That(palette.Fetch(i).EqualsArgb(transparent), Is.True, $"Index {i} should be transparent");
            }
        }

        [Test]
        public void HasSameColorSet_IgnoresOrderAndEmptyTransparentSlots()
        {
            var red = new ArgbColor(255, 255, 0, 0);
            var blue = new ArgbColor(255, 0, 0, 255);

            var p1 = Palette.Create().Apply(0, red).Apply(1, blue);
            var p2 = Palette.Create().Apply(10, blue).Apply(20, red).Apply(30, red);

            Assert.Multiple(() =>
            {
                Assert.That(p1.HasSameColorSet(p2), Is.True);
                Assert.That(p2.HasSameColorSet(p1), Is.True);
                Assert.That(p1.Equals(p2), Is.False, "スロット位置は異なるため厳密等価性は false");
                Assert.That(Palette.Create().HasSameColorSet(Palette.Create()), Is.False, "空パレット同士は重複判定対象外");
            });
        }

        [Test]
        public void ContainsAllColorsOf_And_IsProperSupersetColorSetOf_WorkAcrossOrders()
        {
            var red = new ArgbColor(255, 255, 0, 0);
            var green = new ArgbColor(255, 0, 255, 0);
            var blue = new ArgbColor(255, 0, 0, 255);

            var small = Palette.Create().Apply(5, blue).Apply(15, red);
            var large = Palette.Create().Apply(0, red).Apply(1, green).Apply(2, blue);
            var empty = Palette.Create();

            Assert.Multiple(() =>
            {
                Assert.That(large.ContainsAllColorsOf(small), Is.True);
                Assert.That(large.IsProperSupersetColorSetOf(small), Is.True);
                Assert.That(small.ContainsAllColorsOf(large), Is.False);
                Assert.That(small.IsProperSupersetColorSetOf(large), Is.False);
                Assert.That(large.IsProperSupersetColorSetOf(large), Is.False);
                Assert.That(large.ContainsAllColorsOf(empty), Is.False, "空パレットは内包判定対象外");
            });
        }
    }
}
