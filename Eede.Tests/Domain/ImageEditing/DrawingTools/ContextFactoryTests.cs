using System;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools;

[TestFixture]
public class ContextFactoryTests
{
    private Picture CreateDummyPicture(int width, int height)
    {
        return Picture.CreateEmpty(new PictureSize(width, height));
    }

    [Test]
    public void Create_WithValidPicture_ReturnsDrawingBufferWithSamePicture()
    {
        // Arrange
        var picture = CreateDummyPicture(10, 10);

        // Act
        var drawingBuffer = ContextFactory.Create(picture);

        // Assert
        Assert.That(drawingBuffer, Is.Not.Null);
        Assert.That(drawingBuffer.Previous, Is.EqualTo(picture));
        Assert.That(drawingBuffer.IsDrawing(), Is.False); // Just to verify Idle context since that's what DrawingBuffer constructor does
    }
}
