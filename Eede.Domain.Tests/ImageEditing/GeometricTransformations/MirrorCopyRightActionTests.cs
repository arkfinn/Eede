using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.GeometricTransformations;

[TestFixture]
internal class MirrorCopyRightActionTests
{
    [Test]
    public void TestExecuteEvenWidth()
    {
        // 4x1 pixels
        byte[] beforeData = new byte[] {
            255, 0, 0, 255, // Red
            0, 255, 0, 255, // Green
            0, 0, 255, 255, // Blue
            255, 255, 0, 255 // Yellow
        };
        byte[] expectedData = new byte[] {
            255, 0, 0, 255, // Red
            0, 255, 0, 255, // Green
            0, 255, 0, 255, // Green (mirrored)
            255, 0, 0, 255  // Red (mirrored)
        };

        Picture before = Picture.Create(new PictureSize(4, 1), beforeData);
        MirrorCopyRightAction action = new(before);
        Picture after = action.Execute();

        Assert.That(after.CloneImage(), Is.EqualTo(expectedData));
    }

    [Test]
    public void TestExecuteOddWidth()
    {
        // 5x1 pixels
        byte[] beforeData = new byte[] {
            255, 0, 0, 255,   // Red
            0, 255, 0, 255,   // Green
            255, 255, 255, 255, // White (center)
            0, 0, 255, 255,   // Blue
            0, 0, 0, 255      // Black
        };
        byte[] expectedData = new byte[] {
            255, 0, 0, 255,   // Red
            0, 255, 0, 255,   // Green
            255, 255, 255, 255, // White (center, preserved)
            0, 255, 0, 255,   // Green (mirrored)
            255, 0, 0, 255    // Red (mirrored)
        };

        Picture before = Picture.Create(new PictureSize(5, 1), beforeData);
        MirrorCopyRightAction action = new(before);
                Picture after = action.Execute();
        
                Assert.That(after.CloneImage(), Is.EqualTo(expectedData));
            }
        }
