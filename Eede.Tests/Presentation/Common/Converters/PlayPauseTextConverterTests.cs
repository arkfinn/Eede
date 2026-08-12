using NUnit.Framework;
using System;
using System.Globalization;
using Eede.Presentation.Common.Converters;

namespace Eede.Tests.Presentation.Common.Converters;

[TestFixture]
public class PlayPauseTextConverterTests
{
    private PlayPauseTextConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new PlayPauseTextConverter();
    }

    [Test]
    public void Convert_WithBoolTrue_ReturnsStop()
    {
        var result = _converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Stop"));
    }

    [Test]
    public void Convert_WithBoolFalse_ReturnsPlay()
    {
        var result = _converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Play"));
    }

    [Test]
    public void Convert_WithBoolTrueAndArrowParameter_ReturnsDownArrow()
    {
        var result = _converter.Convert(true, typeof(string), "arrow", CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("▼"));
    }

    [Test]
    public void Convert_WithBoolFalseAndArrowParameter_ReturnsUpArrow()
    {
        var result = _converter.Convert(false, typeof(string), "arrow", CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("▲"));
    }

    [Test]
    public void Convert_WithNonBoolValue_ReturnsPlay()
    {
        var result = _converter.Convert("not a bool", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Play"));
    }

    [Test]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack("Play", typeof(bool), null, CultureInfo.InvariantCulture));
    }
}
