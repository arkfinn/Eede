using System;
using Eede.Infrastructure.Services;
using NUnit.Framework;

namespace Eede.Infrastructure.Tests.Services;

[TestFixture]
public class ExternalBrowserServiceTests
{
    private TestExternalBrowserService _service;

    private class TestExternalBrowserService : ExternalBrowserService
    {
        public bool ProcessStarted { get; set; }
        public string LastUrl { get; set; } = string.Empty;

        protected override void StartProcess(string url)
        {
            ProcessStarted = true;
            LastUrl = url;
        }
    }

    [SetUp]
    public void Setup()
    {
        _service = new TestExternalBrowserService();
    }

    [TestCase("http://example.com")]
    [TestCase("https://example.com")]
    public void OpenUrl_ShouldNotThrow_WhenUrlIsSafe(string url)
    {
        try
        {
            _service.OpenUrl(url);
            Assert.That(_service.ProcessStarted, Is.True, "Should trigger process start");
            Assert.That(_service.LastUrl, Is.EqualTo(url), "Should pass the correct URL");
        }
        catch (ArgumentException ex)
        {
            Assert.Fail($"Should not throw ArgumentException for safe URL: {url}. Error: {ex.Message}");
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
