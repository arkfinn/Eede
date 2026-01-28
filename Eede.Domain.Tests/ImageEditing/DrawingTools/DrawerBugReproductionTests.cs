using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools;

[TestFixture]
public class DrawerBugReproductionTests
{
    [Test]
    public void DrawRectangle_OutOfBounds_ShouldNotThrowException()
    {
        // Arrange
        Picture src = Picture.CreateEmpty(new PictureSize(32, 32));
        Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            // Case 1: Negative coordinates
            drawer.DrawRectangle(new Position(-10, -10), new Position(10, 10));
            // Case 2: Exceeding width/height
            drawer.DrawRectangle(new Position(20, 20), new Position(40, 40));
            // Case 3: Completely outside
            drawer.DrawRectangle(new Position(40, 40), new Position(50, 50));
        });
    }

    [Test]
    public void DrawFillRectangle_OutOfBounds_ShouldNotThrowException()
    {
        // Arrange
        Picture src = Picture.CreateEmpty(new PictureSize(32, 32));
        Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            drawer.DrawFillRectangle(new Position(-10, -10), new Position(10, 10));
            drawer.DrawFillRectangle(new Position(20, 20), new Position(40, 40));
            drawer.DrawFillRectangle(new Position(40, 40), new Position(50, 50));
        });
    }
}
