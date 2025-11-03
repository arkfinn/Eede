using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Pictures;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Blending;

[TestFixture]
public class RGBOnlyImageBlenderTests
{
    [Test]
    public void TestBlend()
    {
        Picture src = ReadPicture(@"ImageEditing\Blending\test\blend.png");
        RGBOnlyImageBlender blender = new();
        Picture dst = ReadPicture(@"ImageEditing\Blending\test\base.png");

        Picture result = dst.Blend(blender, src, new Position(0, 0));

        // result.ToImage().Save(@"ImageEditing\Blending\test\rgb_blend.png", ImageFormat.Png);
        Picture expected = ReadPicture(@"ImageEditing\Blending\test\rgb_blend.png");
        Assert.That(result.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
