using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Blending;

[TestFixture]
public class DirectImageBlenderTests
{
    [Test]
    public void BlendAtNegativePositionShouldNotThrowAndShouldClip()
    {
        // Arrange
        // 2x2 の真っ赤な(R=255)転送元
        byte[] srcPixels = new byte[2 * 2 * 4];
        for (int i = 0; i < srcPixels.Length; i += 4) { srcPixels[i] = 255; srcPixels[i + 3] = 255; }
        Picture src = Picture.Create(new PictureSize(2, 2), srcPixels);

        // 2x2 の真っ黒な(R=0)転送先
        byte[] dstPixels = new byte[2 * 2 * 4];
        for (int i = 0; i < dstPixels.Length; i += 4) { dstPixels[i + 3] = 255; }
        Picture dst = Picture.Create(new PictureSize(2, 2), dstPixels);

        DirectImageBlender blender = new();

        // Act
        // (-1, -1) に転送。dst[0,0] に src[1,1] が転送されるはず。
        Picture result = dst.Blend(blender, src, new Position(-1, -1));

        // Assert
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            // dst[0,0] (index 0) は src[1,1] の色(255)
            Assert.That(resultPixels[0], Is.EqualTo(255));
            // dst[1,0] (index 4) は範囲外なので元の色(0)
            Assert.That(resultPixels[4], Is.EqualTo(0));
            // dst[0,1] (index 8) は範囲外なので元の色(0)
            Assert.That(resultPixels[8], Is.EqualTo(0));
            // dst[1,1] (index 12) は範囲外なので元の色(0)
            Assert.That(resultPixels[12], Is.EqualTo(0));
        });
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
