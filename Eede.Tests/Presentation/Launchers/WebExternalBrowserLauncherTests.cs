using System;
using Eede.Presentation.Launchers;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Launchers;

[TestFixture]
public class WebExternalBrowserLauncherTests
{
    [Test]
    public void OpenUrl_WithValidHttpUrl_InvokesOpenAction()
    {
        string openedUrl = string.Empty;
        var launcher = new WebExternalBrowserLauncher(url => openedUrl = url);

        launcher.OpenUrl("https://github.com/arkfinn/Eede");

        Assert.That(openedUrl, Is.EqualTo("https://github.com/arkfinn/Eede"));
    }

    [Test]
    public void OpenUrl_WithInvalidUrlScheme_ThrowsArgumentException()
    {
        var launcher = new WebExternalBrowserLauncher(_ => { });

        Assert.Throws<ArgumentException>(() => launcher.OpenUrl("javascript:alert(1)"));
        Assert.Throws<ArgumentException>(() => launcher.OpenUrl("file:///C:/path"));
        Assert.Throws<ArgumentException>(() => launcher.OpenUrl("not a url"));
    }
}

