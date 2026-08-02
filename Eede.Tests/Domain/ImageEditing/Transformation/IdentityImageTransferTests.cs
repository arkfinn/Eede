using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.Transformation;

[TestFixture]
public class IdentityImageTransferTests
{
    [Test]
    public void Transfer_ReturnsSamePicture()
    {
        // Arrange
        PictureSize size = new PictureSize(2, 2);
        byte[] pixels = new byte[] {
            255, 0, 0, 255,   0, 255, 0, 255,
            0, 0, 255, 255,   255, 255, 255, 255
        };
        Picture src = Picture.Create(size, pixels);
        IdentityImageTransfer transfer = new();
        Magnification magnification = new(1);

        // Act
        Picture result = transfer.Transfer(src, magnification);

        // Assert
        Assert.That(result, Is.SameAs(src), "IdentityImageTransfer should return the exact same Picture instance.");
    }
}
