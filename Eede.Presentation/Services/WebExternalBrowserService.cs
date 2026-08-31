using System;
using System.Runtime.InteropServices.JavaScript;
using Eede.Application.Infrastructure;

namespace Eede.Presentation.Services;

public partial class WebExternalBrowserService : IExternalBrowserService
{
    private readonly Action<string>? _openAction;

    public WebExternalBrowserService(Action<string>? openAction = null)
    {
        _openAction = openAction;
    }

    public void OpenUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL scheme. Only http and https are allowed.", nameof(url));
        }

        if (_openAction != null)
        {
            _openAction(uri.AbsoluteUri);
            return;
        }

        if (OperatingSystem.IsBrowser())
        {
            OpenUrlInBrowser(uri.AbsoluteUri);
        }
        else
        {
            System.Diagnostics.Trace.WriteLine($"WebExternalBrowserService: {uri.AbsoluteUri}");
        }
    }

    private static void OpenUrlInBrowser(string url)
    {
        try
        {
            // Browser環境でのJS呼び出し
            JsInterop.Open(url);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Failed to open URL via JS: {ex.Message}");
        }
    }

    private static partial class JsInterop
    {
        [JSImport("globalThis.open")]
        internal static partial void Open(string url, string target = "_blank");
    }
}
