using System;
using System.Diagnostics;
using Eede.Infrastructure.Services;
using NUnit.Framework;

namespace Eede.Infrastructure.Tests.Services;

[TestFixture]
public class ExternalBrowserServiceTests
{
    private ExternalBrowserService _service;
    private ProcessStartInfo _capturedPsi;

    [SetUp]
    public void SetUp()
    {
        _service = new ExternalBrowserService();
        _capturedPsi = null;
        _service.ProcessStarter = (psi) => _capturedPsi = psi;
    }

    [Test]
    public void OpenUrl_ShouldStartProcess_WhenHttpUrlProvided()
    {
        // Arrange
        var url = "http://example.com";

        // Act
        _service.OpenUrl(url);

        // Assert
        Assert.That(_capturedPsi, Is.Not.Null);
        Assert.That(_capturedPsi.FileName, Is.EqualTo(url));
        Assert.That(_capturedPsi.UseShellExecute, Is.True);
    }

    [Test]
    public void OpenUrl_ShouldStartProcess_WhenHttpsUrlProvided()
    {
        // Arrange
        var url = "https://example.com";

        // Act
        _service.OpenUrl(url);

        // Assert
        Assert.That(_capturedPsi, Is.Not.Null);
        Assert.That(_capturedPsi.FileName, Is.EqualTo(url));
        Assert.That(_capturedPsi.UseShellExecute, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void OpenUrl_ShouldThrowArgumentException_WhenUrlIsNullOrEmptyOrWhitespace(string url)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.OpenUrl(url));
        Assert.That(ex.ParamName, Is.EqualTo("url"));
    }

    [Test]
    [TestCase("ftp://example.com")]
    [TestCase("file://C:/test.txt")]
    [TestCase("javascript:alert(1)")]
    public void OpenUrl_ShouldThrowArgumentException_WhenUrlIsInvalidScheme(string url)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.OpenUrl(url));
        Assert.That(ex.Message, Does.StartWith("URL must start with http:// or https://"));
    }
}
