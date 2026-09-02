using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture()]
    public class PenStyleTests
    {
        [Test()]
        public void 正常生成()
        {
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            Assert.That(Tuple.Create(
                penStyle.Color.Alpha,
                penStyle.Color.Red,
                penStyle.Color.Green,
                penStyle.Color.Blue,
                penStyle.Width),
                Is.EqualTo(Tuple.Create(255, 0, 0, 0, 1)));
        }

        [Test()]
        public void Blenderはnullを許容しない()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new PenStyle(null!, new ArgbColor(255, 0, 0, 0), 1);
            });
        }

        [Test()]
        public void Widthは1以上でなければならない()
        {
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 0);
            });
        }

        [Test()]
        public void UpdateBlender_UpdatesBlenderCorrectly()
        {
            var originalBlender = new DirectImageBlender();
            var newBlender = new DirectImageBlender();
            var originalColor = new ArgbColor(255, 10, 20, 30);
            var originalWidth = 5;

            var penStyle = new PenStyle(originalBlender, originalColor, originalWidth);
            var updatedPenStyle = penStyle.UpdateBlender(newBlender);

            Assert.That(updatedPenStyle.Blender, Is.SameAs(newBlender));
            Assert.That(updatedPenStyle.Color, Is.EqualTo(originalColor));
            Assert.That(updatedPenStyle.Width, Is.EqualTo(originalWidth));
            Assert.That(updatedPenStyle.Blender, Is.Not.SameAs(originalBlender));
        }

        [Test()]
        public void UpdateColor_UpdatesColorCorrectly()
        {
            var blender = new DirectImageBlender();
            var originalColor = new ArgbColor(255, 10, 20, 30);
            var newColor = new ArgbColor(128, 40, 50, 60);
            var width = 5;

            var penStyle = new PenStyle(blender, originalColor, width);
            var updatedPenStyle = penStyle.UpdateColor(newColor);

            Assert.That(updatedPenStyle.Blender, Is.SameAs(blender));
            Assert.That(updatedPenStyle.Color, Is.EqualTo(newColor));
            Assert.That(updatedPenStyle.Width, Is.EqualTo(width));
        }

        [Test()]
        public void UpdateWidth_UpdatesWidthCorrectly()
        {
            var blender = new DirectImageBlender();
            var color = new ArgbColor(255, 10, 20, 30);
            var originalWidth = 5;
            var newWidth = 10;

            var penStyle = new PenStyle(blender, color, originalWidth);
            var updatedPenStyle = penStyle.UpdateWidth(newWidth);

            Assert.That(updatedPenStyle.Blender, Is.SameAs(blender));
            Assert.That(updatedPenStyle.Color, Is.EqualTo(color));
            Assert.That(updatedPenStyle.Width, Is.EqualTo(newWidth));
        }
    }
}