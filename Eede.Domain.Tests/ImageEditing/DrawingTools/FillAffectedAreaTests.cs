using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture]
    public class FillAffectedAreaTests
    {
        [Test]
        public void AffectedArea_ShouldBeCorrectBoundingBox()
        {
            // Arrange
            // 10x10 empty picture
            var initialPicture = Picture.Create(new PictureSize(10, 10), new byte[10 * 10 * 4]);
            var buffer = new DrawingBuffer(initialPicture);
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            var tool = new Fill();

            // Act
            var pos = new CoordinateHistory(new CanvasCoordinate(5, 5));
            var b1 = tool.DrawStart(buffer, penStyle, pos, false);
            var result = tool.DrawEnd(b1, penStyle, pos, false);

            // Assert
            Assert.That(result.AffectedArea.IsEmpty, Is.False);
            var boundingBox = result.AffectedArea.GetBoundingBox();

            // Expected: Returns whole canvas (0, 0, 10, 10) because whole canvas is filled
            Assert.That(boundingBox.X, Is.EqualTo(0));
            Assert.That(boundingBox.Y, Is.EqualTo(0));
            Assert.That(boundingBox.Width, Is.EqualTo(10));
            Assert.That(boundingBox.Height, Is.EqualTo(10));
        }

        [Test]
        public void AffectedArea_ShouldBePreciseBoundingBox()
        {
            // Arrange
            // 10x10 image with a 4x4 square in the middle
            var initialPixels = new byte[10 * 10 * 4];
            // Fill middle 4x4 with white (255, 255, 255, 255)
            // Area: (3, 3) to (6, 6)
            for (int y = 3; y <= 6; y++)
            {
                for (int x = 3; x <= 6; x++)
                {
                    int index = (x * 4) + (y * 40);
                    initialPixels[index] = 255;
                    initialPixels[index + 1] = 255;
                    initialPixels[index + 2] = 255;
                    initialPixels[index + 3] = 255;
                }
            }
            var initialPicture = Picture.Create(new PictureSize(10, 10), initialPixels);
            var buffer = new DrawingBuffer(initialPicture);
            // Pen color is black (0, 0, 0, 255)
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            var tool = new Fill();

            // Act: Fill at (4, 4) which is part of the white square
            var pos = new CoordinateHistory(new CanvasCoordinate(4, 4));
            var b1 = tool.DrawStart(buffer, penStyle, pos, false);
            var result = tool.DrawEnd(b1, penStyle, pos, false);

            // Assert
            Assert.That(result.AffectedArea.IsEmpty, Is.False);
            var boundingBox = result.AffectedArea.GetBoundingBox();

            // Expected: (3, 3, 4, 4)
            Assert.That(boundingBox.X, Is.EqualTo(3), "X mismatch");
            Assert.That(boundingBox.Y, Is.EqualTo(3), "Y mismatch");
            Assert.That(boundingBox.Width, Is.EqualTo(4), "Width mismatch");
            Assert.That(boundingBox.Height, Is.EqualTo(4), "Height mismatch");
        }
    }
}
