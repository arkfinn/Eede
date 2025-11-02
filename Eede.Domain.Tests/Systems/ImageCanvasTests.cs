using NUnit.Framework;
using Eede.Domain;

namespace Eede.Domain.Tests.Systems
{
    [TestFixture]
    public class ImageCanvasTests
    {
        [Test]
        public void NewImageCanvas_ShouldHaveInitialState()
        {
            // Arrange
            // Act
            var canvas = new ImageCanvas();

            // Assert
            Assert.That(canvas.IsDirty, Is.False);
            Assert.That(canvas.Id, Is.Null);
            Assert.That(canvas.Picture, Is.Null);
            Assert.That(canvas.History, Is.Null);
            Assert.That(canvas.SourceFile, Is.Null);
        }
    }
}
