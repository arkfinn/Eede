using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class SelectionPreviewInfoTests
{
    [Test]
    public void Constructor_WithoutSourcePixels_SetsSourcePixelsToPixels()
    {
        var pixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var position = new Position(5, 5);

        var info = new SelectionPreviewInfo(pixels, position);

        Assert.That(info.SourcePixels, Is.SameAs(pixels));
    }

    [Test]
    public void Constructor_WithSourcePixels_SetsSourcePixelsToProvidedValue()
    {
        var pixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var sourcePixels = Picture.CreateEmpty(new PictureSize(20, 20));
        var position = new Position(5, 5);

        var info = new SelectionPreviewInfo(pixels, position, SelectionPreviewType.CutAndMove, null, sourcePixels);

        Assert.That(info.SourcePixels, Is.SameAs(sourcePixels));
        Assert.That(info.SourcePixels, Is.Not.SameAs(pixels));
    }

    [Test]
    public void Constructor_WithInit_WithoutSourcePixels_SetsSourcePixelsToPixels()
    {
        var pixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var position = new Position(5, 5);
        var sourcePixels = Picture.CreateEmpty(new PictureSize(20, 20));

        var info = new SelectionPreviewInfo(pixels, position)
        {
            SourcePixels = sourcePixels
        };

        Assert.That(info.SourcePixels, Is.SameAs(sourcePixels));
        Assert.That(info.SourcePixels, Is.Not.SameAs(pixels));
    }

    [Test]
    public void Constructor_WithCopy_WithoutSourcePixels_SetsSourcePixelsToPixels()
    {
        var pixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var position = new Position(5, 5);

        var info = new SelectionPreviewInfo(pixels, position);
        var info2 = info with { Position = new Position(10, 10) };

        Assert.That(info2.SourcePixels, Is.SameAs(pixels));
        Assert.That(info2.Position, Is.EqualTo(new Position(10, 10)));
    }
}
