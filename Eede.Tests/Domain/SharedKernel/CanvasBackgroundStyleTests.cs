using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class CanvasBackgroundStyleTests
    {
        [Test]
        public void Default_PropertiesAreExpected()
        {
            var style = CanvasBackgroundStyle.Default;

            Assert.Multiple(() =>
            {
                Assert.That(style.ShowGrid, Is.True);
                Assert.That(style.GridColor, Is.EqualTo(new ArgbColor(255, 200, 200, 200)));
                Assert.That(style.Color1, Is.EqualTo(new ArgbColor(255, 255, 255, 255)));
                Assert.That(style.Color2, Is.EqualTo(new ArgbColor(255, 240, 240, 240)));
            });
        }

        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var gridColor = new ArgbColor(255, 1, 2, 3);
            var color1 = new ArgbColor(255, 4, 5, 6);
            var color2 = new ArgbColor(255, 7, 8, 9);
            var style = new CanvasBackgroundStyle(false, gridColor, color1, color2);

            Assert.Multiple(() =>
            {
                Assert.That(style.ShowGrid, Is.False);
                Assert.That(style.GridColor, Is.EqualTo(gridColor));
                Assert.That(style.Color1, Is.EqualTo(color1));
                Assert.That(style.Color2, Is.EqualTo(color2));
            });
        }
    }
}
