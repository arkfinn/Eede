using System;
using System.Diagnostics;
using Eede.Application.Infrastructure;

namespace Eede.Infrastructure.Services;

public class ExternalBrowserService : IExternalBrowserService
{
    public void OpenUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL scheme. Only http and https are allowed.", nameof(url));
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
