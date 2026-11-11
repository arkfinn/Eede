using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Blending;

[TestFixture]
public class AlphaImageBlenderTests
{
    [Test]
    public void TestBlend()
    {
        Picture src = ReadPicture(@"ImageEditing\Blending\test\blend.png");
        AlphaImageBlender blender = new();
        Picture dst = ReadPicture(@"ImageEditing\Blending\test\base.png");

        Picture result = dst.Blend(blender, src, new Position(0, 0));

        // result.ToImage().Save(@"ImageEditing\Blending\test\alpha_blend.png", ImageFormat.Png);
        Picture expected = ReadPicture(@"ImageEditing\Blending\test\alpha_blend.png");
        Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

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

        AlphaImageBlender blender = new();

        // Act
        // (-1, -1) に転送
        Picture result = dst.Blend(blender, src, new Position(-1, -1));

        // Assert
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            Assert.That(resultPixels[0], Is.EqualTo(255));
            Assert.That(resultPixels[4], Is.EqualTo(0));
            Assert.That(resultPixels[8], Is.EqualTo(0));
            Assert.That(resultPixels[12], Is.EqualTo(0));
        });
    }

    [Test]
    public void Blend_WithPositiveOffset_BlendsOnlyTargetPixel()
    {
        // Arrange: 3x3 の黒画像 (A=255)
        byte[] dstPixels = new byte[3 * 3 * 4];
        for (int i = 0; i < dstPixels.Length; i += 4) { dstPixels[i + 3] = 255; }
        Picture dst = Picture.Create(new PictureSize(3, 3), dstPixels);

        // 1x1 の赤画像 (R=255, A=255)
        byte[] srcPixels = [255, 0, 0, 255];
        Picture src = Picture.Create(new PictureSize(1, 1), srcPixels);

        AlphaImageBlender blender = new();

        // Act: (1, 1) に転送
        Picture result = dst.Blend(blender, src, new Position(1, 1));

        // Assert: (1, 1) [インデックス (1 + 1 * 3) * 4 = 16] だけが赤になり、他は黒のまま
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            // (0, 0) -> 黒
            Assert.That(resultPixels[0], Is.EqualTo(0));
            Assert.That(resultPixels[3], Is.EqualTo(255));
            // (1, 0) -> 黒 (startX / 4 バグがあるとここが汚染される)
            Assert.That(resultPixels[4], Is.EqualTo(0));
            // (0, 1) -> 黒
            Assert.That(resultPixels[12], Is.EqualTo(0));
            // (1, 1) -> 赤
            Assert.That(resultPixels[16], Is.EqualTo(255));
            Assert.That(resultPixels[17], Is.EqualTo(0));
            Assert.That(resultPixels[18], Is.EqualTo(0));
            Assert.That(resultPixels[19], Is.EqualTo(255));
            // (2, 2) -> 黒
            Assert.That(resultPixels[32], Is.EqualTo(0));
        });
    }

    [Test]
    public void Blend_SemiTransparentColors_CalculatesCorrectBlendedColorAndAlpha()
    {
        // Arrange:
        // dst: 1x1 (R:100, G:150, B:200, A:255)
        byte[] dstPixels = [100, 150, 200, 255];
        Picture dst = Picture.Create(new PictureSize(1, 1), dstPixels);

        // src: 1x1 (R:200, G:50, B:0, A:128)
        byte[] srcPixels = [200, 50, 0, 128];
        Picture src = Picture.Create(new PictureSize(1, 1), srcPixels);

        AlphaImageBlender blender = new();

        // Act
        Picture result = dst.Blend(blender, src, new Position(0, 0));

        // Assert
        // exactOutA = 128 * 255 + 255 * 255 - 128 * 255 = 65025
        // A = (65025 + 127) / 255 = 255
        // fromA255 = 128 * 255 = 32640, toAFactor = 255 * (255 - 128) = 32385
        // R = (200 * 32640 + 100 * 32385 + 32512) / 65025 = 9799012 / 65025 = 150
        // G = (50 * 32640 + 150 * 32385 + 32512) / 65025 = 6522262 / 65025 = 100
        // B = (0 * 32640 + 200 * 32385 + 32512) / 65025 = 6509512 / 65025 = 100
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            Assert.That(resultPixels[0], Is.EqualTo(150), "Red should blend correctly");
            Assert.That(resultPixels[1], Is.EqualTo(100), "Green should blend correctly");
            Assert.That(resultPixels[2], Is.EqualTo(100), "Blue should blend correctly");
            Assert.That(resultPixels[3], Is.EqualTo(255), "Alpha should blend correctly");
        });
    }

    [Test]
    public void Blend_WhenSourceAlphaIsZero_LeavesDestinationPixelUnchanged()
    {
        // Arrange: dst: (R:100, G:150, B:200, A:255)
        byte[] dstPixels = [100, 150, 200, 255];
        Picture dst = Picture.Create(new PictureSize(1, 1), dstPixels);

        // src: A=0 (完全透明)
        byte[] srcPixels = [255, 255, 255, 0];
        Picture src = Picture.Create(new PictureSize(1, 1), srcPixels);

        AlphaImageBlender blender = new();

        // Act
        Picture result = dst.Blend(blender, src, new Position(0, 0));

        // Assert
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            Assert.That(resultPixels[0], Is.EqualTo(100));
            Assert.That(resultPixels[1], Is.EqualTo(150));
            Assert.That(resultPixels[2], Is.EqualTo(200));
            Assert.That(resultPixels[3], Is.EqualTo(255));
        });
    }

    [Test]
    public void Blend_WhenDestinationAlphaIsZero_CopiesSourceDirectly()
    {
        // Arrange: dst: 完全透明 (A=0)
        byte[] dstPixels = [0, 0, 0, 0];
        Picture dst = Picture.Create(new PictureSize(1, 1), dstPixels);

        // src: 半透明 (R:200, G:100, B:50, A:180)
        byte[] srcPixels = [200, 100, 50, 180];
        Picture src = Picture.Create(new PictureSize(1, 1), srcPixels);

        AlphaImageBlender blender = new();

        // Act
        Picture result = dst.Blend(blender, src, new Position(0, 0));

        // Assert
        byte[] resultPixels = result.CloneImage();
        Assert.Multiple(() =>
        {
            Assert.That(resultPixels[0], Is.EqualTo(200));
            Assert.That(resultPixels[1], Is.EqualTo(100));
            Assert.That(resultPixels[2], Is.EqualTo(50));
            Assert.That(resultPixels[3], Is.EqualTo(180));
        });
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
