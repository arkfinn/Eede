using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.Blending;

[TestFixture]
public class ImageBlenderBaseTests
{
    private class DummyImageBlender : ImageBlenderBase
    {
        public bool BlendInternalCalled { get; private set; }
        public int LastX { get; private set; }
        public int LastY { get; private set; }

        protected override void BlendInternal(ReadOnlySpan<byte> fromSpan, byte[] toPixels, int fromPos, int toPos)
        {
            BlendInternalCalled = true;
            // Just a dummy copy
            toPixels[toPos] = fromSpan[fromPos];
            toPixels[toPos + 1] = fromSpan[fromPos + 1];
            toPixels[toPos + 2] = fromSpan[fromPos + 2];
            toPixels[toPos + 3] = fromSpan[fromPos + 3];
        }
    }

    [Test]
    public void Blend_WithoutPosition_UsesZeroPosition()
    {
        // Arrange
        byte[] srcPixels = new byte[1 * 1 * 4];
        srcPixels[0] = 255; srcPixels[3] = 255;
        Picture src = Picture.Create(new PictureSize(1, 1), srcPixels);

        byte[] dstPixels = new byte[1 * 1 * 4];
        dstPixels[3] = 255;
        Picture dst = Picture.Create(new PictureSize(1, 1), dstPixels);

        var blender = new DummyImageBlender();

        // Act
        Picture result = blender.Blend(src, dst);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(blender.BlendInternalCalled, Is.True);
            byte[] resultPixels = result.CloneImage();
            Assert.That(resultPixels[0], Is.EqualTo(255));
        });
    }
}
