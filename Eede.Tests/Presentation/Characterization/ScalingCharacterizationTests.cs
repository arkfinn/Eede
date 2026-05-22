using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Presentation.Tests.Characterization;

public class ScalingCharacterizationTests
{
    [Test]
    public void AfterScalingMigration_ShouldProduce1xPicture()
    {
        // 1x1 の赤いピクセルの画像
        var pixels = new byte[] { 255, 0, 0, 255 };
        var source = Picture.Create(new PictureSize(1, 1), pixels);
        var mag = new Magnification(2);
        var identityTransfer = new IdentityImageTransfer();

        // 改善後のレイヤーをシミュレート (PaintBufferLayer)
        var magnified = source.Transfer(identityTransfer, new Magnification(1));

        Assert.That(magnified.Width, Is.EqualTo(1));
        Assert.That(magnified.Height, Is.EqualTo(1));

        var resultPixels = magnified.CloneImage();
        // 1x1 = 1 pixel * 4 bytes = 4 bytes
        Assert.That(resultPixels.Length, Is.EqualTo(4));
        Assert.That(resultPixels[0], Is.EqualTo(255)); // B
        Assert.That(resultPixels[1], Is.EqualTo(0));   // G
        Assert.That(resultPixels[2], Is.EqualTo(0));   // R
        Assert.That(resultPixels[3], Is.EqualTo(255)); // A
    }
}
