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
    public void TestBlend2()
    {
        Picture src = ReadPicture(@"ImageEditing\Blending\test\alpha_rect.png");
        AlphaImageBlender blender = new();
        Picture dst = ReadPicture(@"ImageEditing\Blending\test\alpha_base.png");

        Picture result = dst.Blend(blender, src, new Position(0, 0));

        Picture expected = ReadPicture(@"ImageEditing\Blending\test\alpha_rect.png");
        Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
