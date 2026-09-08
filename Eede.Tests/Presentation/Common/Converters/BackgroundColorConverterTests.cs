using Avalonia.Media;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Presentation.Common.Converters;
using NUnit.Framework;
using System.Globalization;

namespace Eede.Tests.Presentation.Common.Converters;

[TestFixture]
public class BackgroundColorConverterTests
{
    private BackgroundColorConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new BackgroundColorConverter();
    }

    [Test]
    public void Convert_WithBackgroundColor_ReturnsAvaloniaColor()
    {
        var argb = new ArgbColor(255, 12, 34, 56);
        var bg = new BackgroundColor(argb);

        var result = _converter.Convert(bg, typeof(Color), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.InstanceOf<Color>());
        var color = (Color)result!;
        Assert.That(color.A, Is.EqualTo(255));
        Assert.That(color.R, Is.EqualTo(12));
        Assert.That(color.G, Is.EqualTo(34));
        Assert.That(color.B, Is.EqualTo(56));
    }

    [TestCase(null)]
    [TestCase("invalid")]
    [TestCase(123)]
    public void Convert_WithInvalidValue_ReturnsNull(object? invalidInput)
    {
        var result = _converter.Convert(invalidInput, typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ConvertBack_WithAvaloniaColor_ReturnsBackgroundColor()
    {
        var color = Color.FromArgb(200, 50, 60, 70);

        var result = _converter.ConvertBack(color, typeof(BackgroundColor), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.InstanceOf<BackgroundColor>());
        var bg = (BackgroundColor)result!;
        Assert.That(bg.Value.Alpha, Is.EqualTo(200));
        Assert.That(bg.Value.Red, Is.EqualTo(50));
        Assert.That(bg.Value.Green, Is.EqualTo(60));
        Assert.That(bg.Value.Blue, Is.EqualTo(70));
    }

    [TestCase(null)]
    [TestCase("invalid")]
    [TestCase(123)]
    public void ConvertBack_WithInvalidValue_ReturnsNull(object? invalidInput)
    {
        var result = _converter.ConvertBack(invalidInput, typeof(BackgroundColor), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Null);
    }
}
