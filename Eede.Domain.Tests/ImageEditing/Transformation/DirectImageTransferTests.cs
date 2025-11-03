using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Pictures;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Transformation;

[TestFixture]
public class DirectImageTransferTests
{
    [Test]
    public void ARGB値をコピー()
    {
        Picture src = ReadPicture(@"ImageEditing\Transformation\test\blend.png");
        DirectImageTransfer transfer = new();

        Picture data = src.Transfer(transfer, new Magnification(2));

        // dst.Save(@"ImageEditing\Transformation\test\direct.png", ImageFormat.Png);
        Picture expected = ReadPicture(@"ImageEditing\Transformation\test\direct.png");
        Assert.That(data.CloneImage(), Is.EqualTo(expected.CloneImage()));
    }

    private Picture ReadPicture(string path)
    {
        return PictureHelper.ReadBitmap(path);
    }
}
