using Eede.Application.Drawings;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Application.Tests.Drawings
{
    [TestFixture]
    public class DrawableAreaTests
    {
        private Picture CreateFilledPicture(PictureSize size, ArgbColor color)
        {
            byte[] data = new byte[size.Width * size.Height * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = color.Blue;
                data[i + 1] = color.Green;
                data[i + 2] = color.Red;
                data[i + 3] = color.Alpha;
            }
            return Picture.Create(size, data);
        }

        [Test]
        public void PickColor_WithValidPosition_ReturnsCorrectColor()
        {
            // Arrange
            var magnification = new Magnification(2.0f);
            var gridSize = new PictureSize(16, 16);
            var drawableArea = new DrawableArea(magnification, gridSize, null);

            var expectedColor = new ArgbColor(255, 128, 64, 32);
            var picture = CreateFilledPicture(new PictureSize(10, 10), expectedColor);

            // Act
            // (5, 5) on display with magnification 2.0 corresponds to (2, 2) on real picture.
            var color = drawableArea.PickColor(picture, new Position(5, 5));

            // Assert
            Assert.That(color.Alpha, Is.EqualTo(expectedColor.Alpha));
            Assert.That(color.Red, Is.EqualTo(expectedColor.Red));
            Assert.That(color.Green, Is.EqualTo(expectedColor.Green));
            Assert.That(color.Blue, Is.EqualTo(expectedColor.Blue));
        }

        [Test]
        public void PickColor_WithNullPicture_ThrowsArgumentNullException()
        {
            // Arrange
            var magnification = new Magnification(1.0f);
            var gridSize = new PictureSize(16, 16);
            var drawableArea = new DrawableArea(magnification, gridSize, null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => drawableArea.PickColor(null, new Position(0, 0)));
        }

        [Test]
        public void DisplaySizeOf_ReturnsCorrectSizeBasedOnMagnification()
        {
            // Arrange
            var magnification = new Magnification(2.0f);
            var gridSize = new PictureSize(16, 16);
            var drawableArea = new DrawableArea(magnification, gridSize, null);

            var picture = Picture.CreateEmpty(new PictureSize(10, 15));

            // Act
            var displaySize = drawableArea.DisplaySizeOf(picture);

            // Assert
            Assert.That(displaySize.Width, Is.EqualTo(20));
            Assert.That(displaySize.Height, Is.EqualTo(30));
        }

        [Test]
        public void DisplaySizeOf_WithNullPicture_ThrowsArgumentNullException()
        {
            // Arrange
            var magnification = new Magnification(1.0f);
            var gridSize = new PictureSize(16, 16);
            var drawableArea = new DrawableArea(magnification, gridSize, null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => drawableArea.DisplaySizeOf(null));
        }

        [Test]
        public void UpdateMagnification_ReturnsNewInstanceWithUpdatedMagnification()
        {
            // Arrange
            var initialMagnification = new Magnification(1.0f);
            var newMagnification = new Magnification(3.0f);
            var gridSize = new PictureSize(16, 16);
            var drawableArea = new DrawableArea(initialMagnification, gridSize, null);

            // Act
            var updatedArea = drawableArea.UpdateMagnification(newMagnification);

            // Assert
            Assert.That(updatedArea, Is.Not.Null);
            Assert.That(updatedArea, Is.Not.SameAs(drawableArea));
            Assert.That(updatedArea.Magnification, Is.EqualTo(newMagnification));
            Assert.That(drawableArea.Magnification, Is.EqualTo(initialMagnification), "Original instance should remain unchanged.");
        }
    }
}
