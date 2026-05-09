using NUnit.Framework;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class SelectionPreviewInfoTests
{
    [Test]
    public void Constructor_WithoutSourcePixels_SetsSourcePixelsToPixels()
    {
        // Arrange
        var pixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var position = new Position(5, 5);

        // Act
        var info = new SelectionPreviewInfo(pixels, position);

        // Assert
        Assert.That(info.SourcePixels, Is.SameAs(pixels));
    }

    [Test]
    public void Constructor_WithSourcePixels_SetsSourcePixelsToProvidedValue()
    {
        // Arrange
        var pixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var sourcePixels = Picture.CreateEmpty(new PictureSize(20, 20));
        var position = new Position(5, 5);

        // Act
        var info = new SelectionPreviewInfo(pixels, position, SourcePixels: sourcePixels);

        // Assert
        Assert.That(info.SourcePixels, Is.SameAs(sourcePixels));
    }
}
