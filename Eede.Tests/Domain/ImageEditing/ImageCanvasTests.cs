using Eede.Domain.ImageEditing;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class ImageCanvasTests
    {
        [Test]
        public void NewImageCanvas_ShouldHaveInitialState()
        {
            // Arrange
            // Act
            ImageCanvas canvas = new();

            // Assert
            Assert.That(canvas.IsDirty, Is.False);
            Assert.That(canvas.Id, Is.Null);
            Assert.That(canvas.Picture, Is.Null);
            Assert.That(canvas.History, Is.Null);
            Assert.That(canvas.SourceFile, Is.Null);
        }
    }
}
