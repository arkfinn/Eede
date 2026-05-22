using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.UseCase.Pictures;

[TestFixture]
public class TransformImageUseCaseTests
{
    private Picture initialPicture;

    [SetUp]
    public void SetUp()
    {
        // 4x1 pixels: Red, Green, Blue, Yellow (BGRA)
        byte[] data = new byte[] {
            0, 0, 255, 255, // Red (0,0) (B=0, G=0, R=255, A=255)
            0, 255, 0, 255, // Green (1,0) (B=0, G=255, R=0, A=255)
            255, 0, 0, 255, // Blue (2,0) (B=255, G=0, R=0, A=255)
            0, 255, 255, 255 // Yellow (3,0) (B=0, G=255, R=255, A=255)
        };
        initialPicture = Picture.Create(new PictureSize(4, 1), data);
    }

    [Test]
    public void Execute_NoSelection_MirrorCopyRight_MirrorsEntireCanvas()
    {
        // Arrange
        var useCase = new TransformImageUseCase();

        // Act
        var result = useCase.Execute(initialPicture, PictureActions.MirrorCopyRight);

        // Assert: Expected Red, Green, Green, Red
        AssertColor(result, 0, 0, 255, 0, 0, 255); // Red
        AssertColor(result, 1, 0, 0, 255, 0, 255); // Green
        AssertColor(result, 2, 0, 0, 255, 0, 255); // Green (mirrored from 1)
        AssertColor(result, 3, 0, 255, 0, 0, 255); // Red (mirrored from 0)
    }

    [Test]
    public void Execute_WithSelection_MirrorCopyBottom_MirrorsSelectedRegion()
    {
        // Arrange
        // 1x4 pixels: Red, Green, Blue, Yellow (vertical, BGRA)
        byte[] data = new byte[] {
            0, 0, 255, 255, // y=0 (Red)
            0, 255, 0, 255, // y=1 (Green)
            255, 0, 0, 255, // y=2 (Blue)
            0, 255, 255, 255 // y=3 (Yellow)
        };
        var source = Picture.Create(new PictureSize(1, 4), data);
        var selection = new PictureArea(new Position(0, 0), new PictureSize(1, 4));
        var useCase = new TransformImageUseCase();

        // Act
        var result = useCase.Execute(source, PictureActions.MirrorCopyBottom, selection);

        // Assert: Expected Red, Green, Green, Red (vertically)
        AssertColor(result, 0, 0, 255, 0, 0, 255); // Red
        AssertColor(result, 0, 1, 0, 255, 0, 255); // Green
        AssertColor(result, 0, 2, 0, 255, 0, 255); // Green (mirrored from 1)
        AssertColor(result, 0, 3, 255, 0, 0, 255); // Red (mirrored from 0)
    }

    private void AssertColor(Picture pic, int x, int y, byte r, byte g, byte b, byte a)
    {
        var color = pic.PickColor(new Position(x, y));
        Assert.Multiple(() =>
        {
            Assert.That(color.Red, Is.EqualTo(r), $"Red at {x},{y}");
            Assert.That(color.Green, Is.EqualTo(g), $"Green at {x},{y}");
            Assert.That(color.Blue, Is.EqualTo(b), $"Blue at {x},{y}");
            Assert.That(color.Alpha, Is.EqualTo(a), $"Alpha at {x},{y}");
        });
    }
}
