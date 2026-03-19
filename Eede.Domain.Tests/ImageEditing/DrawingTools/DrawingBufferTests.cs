using System;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture]
    public class DrawingBufferTests
    {
        private Picture CreateDummyPicture(int width, int height)
        {
            return Picture.CreateEmpty(new PictureSize(width, height));
        }

        [Test]
        public void Clone_IdleBuffer_ReturnsNewInstanceWithSamePrevious()
        {
            // Arrange
            var previous = CreateDummyPicture(32, 32);
            var buffer = new DrawingBuffer(previous);

            // Act
            var clone = buffer.Clone();

            // Assert
            Assert.That(clone, Is.Not.SameAs(buffer));
            Assert.That(clone.Previous, Is.SameAs(buffer.Previous));
            Assert.That(clone.Drawing, Is.Null);
            Assert.That(clone.IsDrawing(), Is.False);
            Assert.That(clone.Fetch(), Is.SameAs(previous));
        }

        [Test]
        public void Clone_ActiveBuffer_ReturnsNewInstanceWithSamePreviousAndDrawing()
        {
            // Arrange
            var previous = CreateDummyPicture(32, 32);
            var drawing = CreateDummyPicture(32, 32);
            var buffer = new DrawingBuffer(previous).UpdateDrawing(drawing);

            // Act
            var clone = buffer.Clone();

            // Assert
            Assert.That(clone, Is.Not.SameAs(buffer));
            Assert.That(clone.Previous, Is.SameAs(buffer.Previous));
            Assert.That(clone.Drawing, Is.SameAs(buffer.Drawing));
            Assert.That(clone.IsDrawing(), Is.True);
            Assert.That(clone.Fetch(), Is.SameAs(drawing));
        }

        [Test]
        public void Reset_ReturnsNewInstanceWithNewPictureAndIdleState()
        {
            // Arrange
            var initialPicture = CreateDummyPicture(32, 32);
            var buffer = new DrawingBuffer(initialPicture);

            var newPicture = CreateDummyPicture(64, 64);

            // Act
            var resetBuffer = buffer.Reset(newPicture);

            // Assert
            Assert.That(resetBuffer, Is.Not.SameAs(buffer));
            Assert.That(resetBuffer.Previous, Is.SameAs(newPicture));
            Assert.That(resetBuffer.Drawing, Is.Null);
            Assert.That(resetBuffer.IsDrawing(), Is.False);
            Assert.That(resetBuffer.Fetch(), Is.SameAs(newPicture));
        }

        [Test]
        public void Reset_FromActiveState_ReturnsIdleState()
        {
            // Arrange
            var initialPicture = CreateDummyPicture(32, 32);
            var drawingPicture = CreateDummyPicture(32, 32);
            var buffer = new DrawingBuffer(initialPicture).UpdateDrawing(drawingPicture);

            var newPicture = CreateDummyPicture(64, 64);

            // Act
            var resetBuffer = buffer.Reset(newPicture);

            // Assert
            Assert.That(resetBuffer, Is.Not.SameAs(buffer));
            Assert.That(resetBuffer.Previous, Is.SameAs(newPicture));
            Assert.That(resetBuffer.Drawing, Is.Null);
            Assert.That(resetBuffer.IsDrawing(), Is.False);
            Assert.That(resetBuffer.Fetch(), Is.SameAs(newPicture));
        }
    }
}
