using System;
using Eede.Presentation.Services;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Services;

[TestFixture]
public class WebExternalBrowserServiceTests
{
    [Test]
    public void OpenUrl_WithValidHttpUrl_InvokesOpenAction()
    {
        string openedUrl = string.Empty;
        var service = new WebExternalBrowserService(url => openedUrl = url);

        service.OpenUrl("https://github.com/arkfinn/Eede");

        Assert.That(openedUrl, Is.EqualTo("https://github.com/arkfinn/Eede"));
    }

    [Test]
    public void OpenUrl_WithInvalidUrlScheme_ThrowsArgumentException()
    {
        var service = new WebExternalBrowserService(_ => { });

        Assert.Throws<ArgumentException>(() => service.OpenUrl("javascript:alert(1)"));
        Assert.Throws<ArgumentException>(() => service.OpenUrl("file:///C:/path"));
        Assert.Throws<ArgumentException>(() => service.OpenUrl("not a url"));
    }
}
