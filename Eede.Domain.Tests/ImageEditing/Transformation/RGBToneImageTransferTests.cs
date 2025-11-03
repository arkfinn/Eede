using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Transformation;

[TestFixture]
public class RGBToneImageTransferTests
{
    [Test]
    public void RGB値のみをコピー()
    {
        Picture src = ReadPicture(@"ImageEditing\Transformation\test\blend.png");
        RGBToneImageTransfer transfer = new();

        Picture data = src.Transfer(transfer, new Magnification(2));

        // dst.Save(@"ImageEditing\Transformation\test\rgb.png", ImageFormat.Png);
        Picture expected = ReadPicture(@"ImageEditing\Transformation\test\rgb.png");
        Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
