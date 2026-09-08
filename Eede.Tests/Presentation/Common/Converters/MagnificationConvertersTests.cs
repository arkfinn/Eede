using Eede.Domain.ImageEditing;
using Eede.Presentation.Common.Converters;
using NUnit.Framework;
using System;
using System.Globalization;

namespace Eede.Tests.Presentation.Common.Converters;

[TestFixture]
public class MagnificationConvertersTests
{
    private MagnificationToPercentageConverter _percentageConverter;
    private MagnificationToDoubleConverter _doubleConverter;

    [SetUp]
    public void SetUp()
    {
        _percentageConverter = new MagnificationToPercentageConverter();
        _doubleConverter = new MagnificationToDoubleConverter();
    }

    [TestCase(1.0, "100%")]
    [TestCase(0.5, "50%")]
    [TestCase(2.0, "200%")]
    [TestCase(3.5, "350%")]
    public void PercentageConverter_Convert_WithMagnification_ReturnsFormattedPercentage(double factor, string expected)
    {
        var mag = new Magnification((float)factor);
        var result = _percentageConverter.Convert(mag, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("not a mag")]
    [TestCase(123)]
    public void PercentageConverter_Convert_WithNonMagnification_ReturnsDefault100Percent(object? invalidInput)
    {
        var result = _percentageConverter.Convert(invalidInput, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("100%"));
    }

    [Test]
    public void PercentageConverter_ConvertBack_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _percentageConverter.ConvertBack("100%", typeof(Magnification), null, CultureInfo.InvariantCulture));
    }

    [TestCase(1.0, 1.0)]
    [TestCase(0.5, 0.5)]
    [TestCase(2.0, 2.0)]
    [TestCase(3.5, 3.5)]
    public void DoubleConverter_Convert_WithMagnification_ReturnsDoubleValue(double factor, double expected)
    {
        var mag = new Magnification((float)factor);
        var result = _doubleConverter.Convert(mag, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    [TestCase(null)]
    [TestCase("not a mag")]
    [TestCase(123)]
    public void DoubleConverter_Convert_WithNonMagnification_ReturnsDefault1Point0(object? invalidInput)
    {
        var result = _doubleConverter.Convert(invalidInput, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(1.0));
    }

    [Test]
    public void DoubleConverter_ConvertBack_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _doubleConverter.ConvertBack(1.0, typeof(Magnification), null, CultureInfo.InvariantCulture));
    }
}
