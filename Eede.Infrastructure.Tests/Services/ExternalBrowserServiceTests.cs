using System;
using Eede.Infrastructure.Services;
using NUnit.Framework;

namespace Eede.Infrastructure.Tests.Services;

[TestFixture]
public class ExternalBrowserServiceTests
{
    private ExternalBrowserService _service;

    [SetUp]
    public void Setup()
    {
        _service = new ExternalBrowserService();
    }

    [TestCase("http://example.com")]
    [TestCase("https://example.com")]
    public void OpenUrl_ShouldNotThrow_WhenUrlIsSafe(string url)
    {
        // We can't easily verify Process.Start actually ran in this environment,
        // but we can verify it doesn't throw a validation exception for safe URLs.
        // If Process.Start fails due to environment (no browser), it might throw,
        // so we'll just check it doesn't throw our ArgumentException.
        try
        {
            _service.OpenUrl(url);
        }
        catch (ArgumentException ex)
        {
            Assert.Fail($"Should not throw ArgumentException for safe URL: {url}. Error: {ex.Message}");
        }
        catch (Exception)
        {
            // Other exceptions (like Process.Start failing in Linux) are acceptable here
            // as we are testing the validation logic.
        }
    }

    [TestCase("file:///C:/Windows/System32/calc.exe")]
    [TestCase("ftp://example.com")]
    [TestCase("javascript:alert(1)")]
    [TestCase("C:\\Windows\\System32\\calc.exe")]
    [TestCase("/etc/passwd")]
    public void OpenUrl_ShouldThrowArgumentException_WhenUrlIsUnsafe(string url)
    {
        var ex = Assert.Throws<ArgumentException>(() => _service.OpenUrl(url));
        Assert.That(ex.Message, Does.Contain("Invalid URL scheme"));
    }
}
