using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

        StartProcess(uri.AbsoluteUri);
    }

    protected virtual void StartProcess(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var psi = new ProcessStartInfo
            {
                FileName = "xdg-open",
                UseShellExecute = false
            };
            psi.ArgumentList.Add(url);
            Process.Start(psi);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var psi = new ProcessStartInfo
            {
                FileName = "open",
                UseShellExecute = false
            };
            psi.ArgumentList.Add(url);
            Process.Start(psi);
        }
        else
        {
            throw new PlatformNotSupportedException("Operating system not supported for opening external URLs.");
        }
    }
}
