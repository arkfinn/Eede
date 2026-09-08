using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Converters;
using NUnit.Framework;
using System;
using System.Globalization;

namespace Eede.Tests.Presentation.Common.Converters;

[TestFixture]
public class BitmapConvertersTests
{
    [AvaloniaTest]
    public void PictureToBitmapConverter_Convert_WithPicture_ReturnsBitmap()
    {
        var converter = new PictureToBitmapConverter();
        var picture = Picture.Create(new PictureSize(4, 4), new byte[4 * 4 * 4]);

        var result = converter.Convert(picture, typeof(Bitmap), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.InstanceOf<Bitmap>());
        using var bitmap = (Bitmap)result!;
        Assert.That(bitmap.PixelSize.Width, Is.EqualTo(4));
        Assert.That(bitmap.PixelSize.Height, Is.EqualTo(4));
    }

    [TestCase(null)]
    [TestCase("not a picture")]
    [TestCase(123)]
    public void PictureToBitmapConverter_Convert_WithInvalidValue_ReturnsNull(object? invalidInput)
    {
        var converter = new PictureToBitmapConverter();
        var result = converter.Convert(invalidInput, typeof(Bitmap), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void PictureToBitmapConverter_ConvertBack_ThrowsNotSupportedException()
    {
        var converter = new PictureToBitmapConverter();
        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(null, typeof(Picture), null, CultureInfo.InvariantCulture));
    }

    [TestCase(null)]
    [TestCase(123)]
    [TestCase(true)]
    public void BitmapConverter_Convert_WithNonString_ReturnsNull(object? invalidInput)
    {
        var converter = new BitmapConverter();
        var result = converter.Convert(invalidInput, typeof(Bitmap), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void BitmapConverter_ConvertBack_ThrowsNotSupportedException()
    {
        var converter = new BitmapConverter();
        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(null, typeof(string), null, CultureInfo.InvariantCulture));
    }
}
