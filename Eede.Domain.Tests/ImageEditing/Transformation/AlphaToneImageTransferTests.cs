using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Pictures;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Transformation;

[TestFixture]
public class AlphaToneImageTransferTests
{
    [Test]
    public void A値のみをコピー()
    {
        Picture src = ReadPicture(@"ImageEditing\Transformation\test\blend.png");
        AlphaToneImageTransfer transfer = new();

        Picture data = src.Transfer(transfer, new Magnification(2));

        // dst.Save(@"ImageEditing\Transformation\test\alpha.png", ImageFormat.Png);
        Picture expected = ReadPicture(@"ImageEditing\Transformation\test\alpha.png");
        Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
