using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.GeometricTransformations;

[TestFixture]
internal class MirrorCopyBottomActionTests
{
    [Test]
    public void TestExecuteEvenHeight()
    {
        // 1x4 pixels
        byte[] beforeData = new byte[] {
            255, 0, 0, 255, // Red (y=0)
            0, 255, 0, 255, // Green (y=1)
            0, 0, 255, 255, // Blue (y=2)
            255, 255, 0, 255 // Yellow (y=3)
        };
        byte[] expectedData = new byte[] {
            255, 0, 0, 255, // Red
            0, 255, 0, 255, // Green
            0, 255, 0, 255, // Green (mirrored from y=1)
            255, 0, 0, 255  // Red (mirrored from y=0)
        };

        Picture before = Picture.Create(new PictureSize(1, 4), beforeData);
        MirrorCopyBottomAction action = new(before);
        Picture after = action.Execute();

        Assert.That(after.CloneImage(), Is.EqualTo(expectedData));
    }

    [Test]
    public void TestExecuteOddHeight()
    {
        // 1x5 pixels
        byte[] beforeData = new byte[] {
            255, 0, 0, 255,   // Red
            0, 255, 0, 255,   // Green
            255, 255, 255, 255, // White (center, y=2)
            0, 0, 255, 255,   // Blue
            0, 0, 0, 255      // Black
        };
        byte[] expectedData = new byte[] {
            255, 0, 0, 255,   // Red
            0, 255, 0, 255,   // Green
            255, 255, 255, 255, // White (center preserved)
            0, 255, 0, 255,   // Green (mirrored)
            255, 0, 0, 255    // Red (mirrored)
        };

        Picture before = Picture.Create(new PictureSize(1, 5), beforeData);
        MirrorCopyBottomAction action = new(before);
        Picture after = action.Execute();

        Assert.That(after.CloneImage(), Is.EqualTo(expectedData));
    }
}
