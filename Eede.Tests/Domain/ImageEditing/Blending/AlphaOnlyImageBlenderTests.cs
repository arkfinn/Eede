using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Blending;

[TestFixture]
public class AlphaOnlyImageBlenderTests
{
    [Test]
    public void TestBlend()
    {
        Picture src = ReadPicture(@"ImageEditing\Blending\test\blend.png");
        AlphaOnlyImageBlender blender = new();
        Picture dst = ReadPicture(@"ImageEditing\Blending\test\base.png");

        Picture result = dst.Blend(blender, src, new Position(0, 0));

        // result.ToImage().Save(@"ImageEditing\Blending\test\alpha_only_blend.png", ImageFormat.Png);
        Picture expected = ReadPicture(@"ImageEditing\Blending\test\alpha_only_blend.png");
        Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

    [Test]
    public void Blend_WithOffsetPosition_UpdatesOnlyTargetAlphaAndPreservesRgb()
    {
        // Arrange: 3x3 の青画像 (R:0, G:0, B:255, A:255)
        byte[] dstPixels = new byte[3 * 3 * 4];
        for (int i = 0; i < dstPixels.Length; i += 4)
        {
            dstPixels[i + 2] = 255; // B
            dstPixels[i + 3] = 255; // A
        }
        Picture dst = Picture.Create(new PictureSize(3, 3), dstPixels);

        // 1x1 の白・半透明 (R:255, G:255, B:255, A:120)
        byte[] srcPixels = [255, 255, 255, 120];
        Picture src = Picture.Create(new PictureSize(1, 1), srcPixels);

        AlphaOnlyImageBlender blender = new();

        // Act: (1, 1) に転送
        Picture result = dst.Blend(blender, src, new Position(1, 1));

        // Assert: (1, 1) [インデックス 16..19] のみ Alpha が 120 に更新され、RGB は元の 0, 0, 255 のまま
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            // (0, 0)
            Assert.That(resultPixels[0], Is.EqualTo(0));
            Assert.That(resultPixels[2], Is.EqualTo(255));
            Assert.That(resultPixels[3], Is.EqualTo(255));

            // (1, 0)
            Assert.That(resultPixels[4], Is.EqualTo(0));
            Assert.That(resultPixels[6], Is.EqualTo(255));
            Assert.That(resultPixels[7], Is.EqualTo(255));

            // (1, 1) - 目標ピクセル
            Assert.That(resultPixels[16], Is.EqualTo(0), "R must remain unchanged");
            Assert.That(resultPixels[17], Is.EqualTo(0), "G must remain unchanged");
            Assert.That(resultPixels[18], Is.EqualTo(255), "B must remain unchanged");
            Assert.That(resultPixels[19], Is.EqualTo(120), "A must be updated from src");

            // (2, 2)
            Assert.That(resultPixels[32], Is.EqualTo(0));
            Assert.That(resultPixels[34], Is.EqualTo(255));
            Assert.That(resultPixels[35], Is.EqualTo(255));
        });
    }

    [Test]
    public void BlendAtNegativePosition_ClipsCorrectly()
    {
        // Arrange: 2x2 の青画像 (A=255)
        byte[] dstPixels = new byte[2 * 2 * 4];
        for (int i = 0; i < dstPixels.Length; i += 4)
        {
            dstPixels[i + 2] = 255;
            dstPixels[i + 3] = 255;
        }
        Picture dst = Picture.Create(new PictureSize(2, 2), dstPixels);

        // 2x2 の半透明画像 (A=80)
        byte[] srcPixels = new byte[2 * 2 * 4];
        for (int i = 0; i < srcPixels.Length; i += 4)
        {
            srcPixels[i + 3] = 80;
        }
        Picture src = Picture.Create(new PictureSize(2, 2), srcPixels);

        AlphaOnlyImageBlender blender = new();

        // Act: (-1, -1) に転送
        Picture result = dst.Blend(blender, src, new Position(-1, -1));

        // Assert: (0, 0) のみ A=80 に更新され、他は 255 のまま
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            Assert.That(resultPixels[3], Is.EqualTo(80));
            Assert.That(resultPixels[7], Is.EqualTo(255));
            Assert.That(resultPixels[11], Is.EqualTo(255));
            Assert.That(resultPixels[15], Is.EqualTo(255));
        });
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
