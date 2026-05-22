using Eede.Application.Drawings;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.Drawings
{
    [TestFixture]
    public class DrawingResultTests
    {
        [Test]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var picture = Eede.Domain.ImageEditing.Picture.CreateEmpty(new PictureSize(10, 10));
            var buffer = new DrawingBuffer(picture);
            var drawableArea = new DrawableArea(new Eede.Domain.ImageEditing.Magnification(1.0f), new PictureSize(16, 16), null);
            var region = new PictureRegion(new PictureArea(new Position(1, 1), new PictureSize(5, 5)));

            // Act
            var result = new DrawingResult(buffer, drawableArea, region);

            // Assert
            Assert.That(result.PictureBuffer, Is.SameAs(buffer));
            Assert.That(result.DrawableArea, Is.SameAs(drawableArea));
            Assert.That(result.AffectedArea, Is.EqualTo(region));
        }

        [Test]
        public void Constructor_WithDefaultAffectedArea_InitializesPropertiesCorrectly()
        {
            // Arrange
            var picture = Eede.Domain.ImageEditing.Picture.CreateEmpty(new PictureSize(10, 10));
            var buffer = new DrawingBuffer(picture);
            var drawableArea = new DrawableArea(new Eede.Domain.ImageEditing.Magnification(1.0f), new PictureSize(16, 16), null);

            // Act
            var result = new DrawingResult(buffer, drawableArea);

            // Assert
            Assert.That(result.PictureBuffer, Is.SameAs(buffer));
            Assert.That(result.DrawableArea, Is.SameAs(drawableArea));
            Assert.That(result.AffectedArea.IsEmpty, Is.True);
        }

        [Test]
        public void Constructor_WithNullBuffer_InitializesPropertiesCorrectly()
        {
            // Arrange
            var drawableArea = new DrawableArea(new Eede.Domain.ImageEditing.Magnification(1.0f), new PictureSize(16, 16), null);

            // Act
            var result = new DrawingResult(null, drawableArea);

            // Assert
            Assert.That(result.PictureBuffer, Is.Null);
            Assert.That(result.DrawableArea, Is.SameAs(drawableArea));
            Assert.That(result.AffectedArea.IsEmpty, Is.True);
        }
    }
}
